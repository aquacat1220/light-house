namespace LightHouse
{
    using System;
    using System.Collections.Generic;
    using FishNet.Connection;
    using FishNet.Managing.Timing;
    using FishNet.Object;
    using FishNet.Observing;
    using LightHouse.Fn;
    using NaughtyAttributes;
    using UnityEngine;
    using UnityEngine.Events;

    public class ProjectileSpawner : NetworkBehaviour
    {
        [SerializeField]
        GameObject _projectile;
        [SerializeField]
        [Required]
        Transform _spawnPoint;

        [SerializeField]
        static int _waitQueueCapacity = 5;
        [SerializeField]
        static float _maxWaitTime = 0.025f;

        [SerializeField]
        Event<int> _counterChange;
        [SerializeField]
        Event<int> _predictedCounterChange;

        // The number of projectiles that were spawned across the network.
        // On clients, this is the number of authoritative-spawned projectiles + accepted predicted-spawned projectiles.
        // On the server, this is the number of authoritative-spawned projectiles.
        // Thus even on the server, this value is not incremented immediately after `SpawnProjectile()`, since we wait for `_maxWaitTime` for the client to send predicted spawns.
        // If you want values that are incremented immediately, see `PredictedCounter`.
        int _counter = 0;
        public int Counter
        {
            get { return _counter; }
        }

        // The number of projectiles *this instance* wants spawned, but hasn't been network-spawned yet.
        // On clients, this is the number of network-spawned projectiles (`_counter`) + not-yet-validated predicted-spawned projectiles.
        // On the server, this is the number of authoritative-spawned projectiles + waitlisted tickets that will eventually spawn.
        // This value will be incremented immediately after `SpawnProjectile()`.
        // On clients, the value might rollback due to a server rejection.
        // On servers, the value will never rollback.
        int _predictedDelta = 0;
        public int PredictedCounter
        {
            get { return _counter + _predictedDelta; }
        }

        int _oldCounter = 0;
        int _oldPredictedCounter = 0;

        bool _usePredictedSpawn = false;

        Queue<(PreciseTick Tick, ProjectileTransform Projectile)> _waitingProjectiles = new Queue<(PreciseTick Tick, ProjectileTransform Projectile)>(_waitQueueCapacity);
        Queue<(PreciseTick Tick, Vector2 Position, float Rotation)> _waitingTickets = new Queue<(PreciseTick Tick, Vector2 Position, float Rotation)>(_waitQueueCapacity);

        Alarm _clearWaitlistAlarm;

        void Awake()
        {
            if (_projectile == null)
            {
                Debug.Log("`_projectile` wasn't set.");
                throw new Exception();
            }
            var projectile = _projectile.GetComponent<ProjectileTransform>();
            if (projectile == null)
            {
                Debug.Log("`_projectile` does not have the `ProjectileTransform` component.");
                throw new Exception();
            }

            if (projectile.NetworkObject.PredictedSpawn?.GetAllowSpawning() is true)
            {
                if (_projectile.GetComponent<NetworkObserver>().GetObserverCondition<AlwaysFalseCondition>() == null)
                {
                    Debug.Log("`_projectile` was set to predicted-spawn, but doesn't have the `AlwaysFalseCondition` observer condition. We need it to disable observation!");
                    throw new Exception();
                }
                _usePredictedSpawn = true;
            }

            if (_spawnPoint == null)
            {
                Debug.Log("`_spawnPoint` wasn't set.");
                throw new Exception();
            }
        }

        public override void OnStartServer()
        {
            _clearWaitlistAlarm = TimerManager.Singleton.AddAlarm(
                cooldown: _maxWaitTime / 2f,
                callback: ClearWaitlist,
                startImmediately: false,
                initialCooldown: _maxWaitTime / 2f
            );
        }

        public override void OnStopServer()
        {
            _clearWaitlistAlarm?.Remove();
        }

        public override void OnStopClient()
        {
            // Reset `_predictedDelta` to 0, conceptually rejecting all predictions we have requested for.
            _predictedDelta = 0;
            FlushCounterChanges();
        }

        void ClearWaitlist(float _)
        {
            while (_waitingProjectiles.Count > 0)
            {
                (var projectileTick, var projectile) = _waitingProjectiles.Peek();
                var waitTime = TimeManager.TicksToTime(TimeManager.GetPreciseTick(TickType.Tick)) - TimeManager.TicksToTime(projectileTick);
                if (waitTime > _maxWaitTime)
                {
                    Debug.Log($"Clearing old projectile from projectile waitlist.");
                    (var evictedProjectileTick, var evictedProjectile) = _waitingProjectiles.Dequeue();
                    Debug.Log($"Projectile spawn request (arrived at {evictedProjectileTick}) was denied due to waitlist eviction (wait timeout).");
                    // Mark the projectile to be "predicted-spawn rejected", so the spawning client can see this projectile was "rejected" instead of being "normally despawned".
                    evictedProjectile.RejectProjectile();
                    var nob = evictedProjectile.NetworkObject;
                    // An eviction can happen if a ticket never arrived.

                    // Make the spawning client the owner of the projectile, so that it receives the despawn message.
                    var predictedSpawner = nob.PredictedSpawner;
                    nob.GiveOwnership(predictedSpawner);
                    nob.Despawn();
                    // Send a NACK to the predicted spawner.
                    TargetSyncCounter(predictedSpawner);
                    continue;
                }
                break;
            }

            while (_waitingTickets.Count > 0)
            {
                var ticket = _waitingTickets.Peek();
                var waitTime = TimeManager.TicksToTime(TimeManager.GetPreciseTick(TickType.Tick)) - TimeManager.TicksToTime(ticket.Tick);
                if (waitTime > _maxWaitTime)
                {
                    Debug.Log($"Clearing old ticket from ticket waitlist.");
                    var evictedTicket = _waitingTickets.Dequeue();
                    var projectileGameObject = Instantiate(_projectile, evictedTicket.Position, Quaternion.Euler(0f, 0f, evictedTicket.Rotation));
                    var projectile = projectileGameObject.GetComponent<ProjectileTransform>();
                    projectile.ProjectileSpawner = this;
                    Spawn(
                        projectileGameObject,
                        null,
                        gameObject.scene
                    );
                    projectile.ResetSpawn(evictedTicket.Tick, evictedTicket.Position, evictedTicket.Rotation);

                    // Disable the alwaysfalse condition to make the projectile observable to everyone.
                    var nob = projectile.NetworkObject;
                    nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
                    // One waitlisted ticket has been officially spawned!
                    _counter += 1;
                    _predictedDelta -= 1;
                    FlushCounterChanges();
                    // Completely authoritative, no PS involved.
                    GlobalSyncCounter(null);
                    continue;
                }
                break;
            }

            if (_waitingProjectiles.Count == 0 && _waitingTickets.Count == 0)
            {
                _clearWaitlistAlarm.Stop();
            }
        }

        [Serializable]
        public class SpawnProjectileFn : IFn<Fn.Tuple, Fn.Tuple>
        {
            public ProjectileSpawner ProjectileSpawner;
            public Fn.Tuple Invoke(Fn.Tuple param)
            {
                ProjectileSpawner?.SpawnProjectile();
                return Fn.Tuple.Unit;
            }
        }

        public void SpawnProjectile()
        {
            if (!base.IsSpawned)
            {
                Debug.Log("`SpawnProjectile()` was called but component was not network initialized.");
                throw new Exception();
            }
            if (!_usePredictedSpawn)
            {
                // We are not using predictive spawning.
                // Spawn is only possible on server.
                if (!base.IsServerInitialized)
                    return;

                var projectileGameObject = Instantiate(_projectile, _spawnPoint.position, _spawnPoint.rotation);
                var projectile = projectileGameObject.GetComponent<ProjectileTransform>();
                projectile.ProjectileSpawner = this;
                var nob = projectile.NetworkObject;
                Spawn(
                    projectileGameObject,
                    null,
                    gameObject.scene
                );
                // Make sure to disable the alwaysfalse condition to ensure the projectile observable to everyone.
                nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
                _counter += 1;
                // Flush counter changes and trigger events.
                FlushCounterChanges();
                // And let all observers receive this change.
                GlobalSyncCounter(null);
                return;
            }
            // We are using predictive spawning.
            // Spawn is technically possible on the server too, but we'll be spawning on the owning client only.

            // If we are the owning host, we are the authority anyway; just do normal spawning.
            if (base.IsServerInitialized && base.IsOwner)
            {
                var projectileGameObject = Instantiate(_projectile, _spawnPoint.position, _spawnPoint.rotation);
                var projectile = projectileGameObject.GetComponent<ProjectileTransform>();
                projectile.ProjectileSpawner = this;
                var nob = projectile.NetworkObject;
                Spawn(
                    projectileGameObject,
                    null,
                    gameObject.scene
                );
                // Make sure to disable the alwaysfalse condition to ensure the projectile observable to everyone.
                nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
                _counter += 1;
                // Flush counter changes and trigger events.
                FlushCounterChanges();
                // And let all observers receive this change.
                GlobalSyncCounter(null);
                return;
            }

            // If we are the server but not a client, add a ticket.
            if (base.IsServerInitialized)
            {
                AddTicketToWaitlist();
                return;
            }
            else
            {
                // If we are a non-owner client, we shouldn't have reached this far. How TF did this happen?
                if (!base.IsOwner)
                {
                    Debug.Log("`SpawnProjectile()` was called on a non-owner client.");
                    throw new Exception();
                }
                // If we are the owning, non-server client, initiate predictive spawn!
                var projectileGameObject = Instantiate(_projectile, _spawnPoint.position, _spawnPoint.rotation);
                var projectile = projectileGameObject.GetComponent<ProjectileTransform>();
                projectile.ProjectileSpawner = this;
                Spawn(
                    projectileGameObject,
                    null,
                    gameObject.scene
                );
                _predictedDelta += 1;
                FlushCounterChanges();
                return;
            }
        }

        [Server]
        void AddTicketToWaitlist()
        {
            if (!_usePredictedSpawn)
            {
                Debug.Log($"{TimeManager.Tick}: A ticket was added to waitlist, but the projectile spawner is not set to perform predicted spawning.");
                throw new Exception();
            }

            PreciseTick tick = TimeManager.GetPreciseTick(TickType.Tick);
            Vector2 position = _spawnPoint.position;
            float rotation = _spawnPoint.rotation.eulerAngles.z;

            // Check projectile waitlist first before considering adding the projectile to waitlist.
            if (_waitingProjectiles.Count > 0)
            {
                // We have a waiting projectile.
                (var projectileTick, var projectile) = _waitingProjectiles.Dequeue();
                projectile.transform.position = position;
                projectile.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
                projectile.ResetSpawn(tick, position, rotation);
                projectile.SetActive(true);

                // Disable the alwaysfalse condition to make the projectile observable to everyone.
                var nob = projectile.NetworkObject;
                nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
                // One non-waitlisted ticket has been spawned.
                _counter += 1;
                FlushCounterChanges();
                // We are ACKing a PSed projectile.
                GlobalSyncCounter(nob.PredictedSpawner);
                return;
            }

            // Then check if the ticket waitlist has an empty spot.
            // If it is full, evict a ticket to make room.
            // The evicted ticket will be spawned immediately.
            if (_waitingTickets.Count == _waitQueueCapacity)
            {
                Debug.Log($"Attempting to add a ticket to an already full waitlist. Evicting the oldest ticket.");
                var ticket = _waitingTickets.Dequeue();
                var projectileGameObject = Instantiate(_projectile, ticket.Position, Quaternion.Euler(0f, 0f, ticket.Rotation));
                var projectile = projectileGameObject.GetComponent<ProjectileTransform>();
                projectile.ProjectileSpawner = this;
                Spawn(
                    projectileGameObject,
                    null,
                    gameObject.scene
                );
                projectile.ResetSpawn(ticket.Tick, ticket.Position, ticket.Rotation);

                // Disable the alwaysfalse condition to make the projectile observable to everyone.
                var nob = projectile.NetworkObject;
                nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
                // One waitlisted ticket has been officially spawned!
                _counter += 1;
                _predictedDelta -= 1;
                FlushCounterChanges();
                // Completely authoritative, no PS involved.
                GlobalSyncCounter(null);
            }

            _waitingTickets.Enqueue(
                (TimeManager.GetPreciseTick(TickType.Tick), position, rotation)
            );
            _clearWaitlistAlarm.Start();
            // One ticket injected into waitlist.
            _predictedDelta += 1;
            FlushCounterChanges();
        }

        [Server]
        public void AddProjectileToWaitlist(ProjectileTransform projectile)
        {
            if (!_usePredictedSpawn)
            {
                Debug.Log($"{TimeManager.Tick}: A projectile was added to waitlist, but the projectile spawner is not set to perform predicted spawning.");
                throw new Exception();
            }

            // First do some basic checking; is the requesting client the owner?
            if (projectile.NetworkObject.PredictedSpawner != base.Owner)
            {
                Debug.Log($"{TimeManager.Tick}: A non-owner attempted to predicted-spawn a projectile. If this is repeated, we might need to kick this client.");
                // The PSed projectile was not spawned by the owner of this projectile spawner.
                var nob = projectile.NetworkObject;
                nob.GiveOwnership(nob.PredictedSpawner);
                nob.Despawn();
                // Send a NACK to the predicted spawner.
                TargetSyncCounter(nob.PredictedSpawner);
                return;
            }

            // Check ticket waitlist first before considering adding the projectile to waitlist.
            if (_waitingTickets.Count > 0)
            {
                // We have a waiting ticket.
                var ticket = _waitingTickets.Dequeue();
                projectile.transform.position = ticket.Position;
                projectile.transform.rotation = Quaternion.Euler(0f, 0f, ticket.Rotation);
                projectile.ResetSpawn(ticket.Tick, ticket.Position, ticket.Rotation);

                // Disable the alwaysfalse condition to make the projectile observable to everyone.
                var nob = projectile.GetComponent<NetworkObject>();
                nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
                // One waitlisted ticket has been officially spawned!
                _counter += 1;
                _predictedDelta -= 1;
                FlushCounterChanges();
                // We are ACKing a PSed projectile.
                GlobalSyncCounter(nob.PredictedSpawner);
                return;
            }

            // Then check if the projectile waitlist has an empty spot.
            // If it is full, evict a projectile to make room.
            // The evicted projectile will be despawned immediately.
            if (_waitingProjectiles.Count == _waitQueueCapacity)
            {
                Debug.Log($"Attempting to add a projectile to an already full waitlist. Evicting the oldest projectile.");
                (var tick, var evictedProjectile) = _waitingProjectiles.Dequeue();
                Debug.Log($"Projectile spawn request (arrived at {tick}) was denied due to waitlist eviction (waitlist full).");
                // Mark the projectile to be "predicted-spawn rejected", so the spawning client can see this projectile was "rejected" instead of being "normally despawned".
                evictedProjectile.RejectProjectile();
                var nob = evictedProjectile.NetworkObject;
                // An eviction can happen if a ticket never arrived.

                // Make the spawning client the owner of the projectile, so that it receives the despawn message.
                var predictedSpawner = nob.PredictedSpawner;
                nob.GiveOwnership(predictedSpawner);
                nob.Despawn();
                // Send a NACK to the predicted spawner.
                TargetSyncCounter(predictedSpawner);
            }

            // This line isn't needed, but just to make sure the condition is enabled before adding to the waitlist.
            projectile.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(true);
            projectile.SetActive(false);
            _waitingProjectiles.Enqueue((TimeManager.GetPreciseTick(TickType.Tick), projectile));
            _clearWaitlistAlarm.Start();
        }

        // Flush counter changes so event subscribers get notified.
        // This doesn't send network updates.
        void FlushCounterChanges()
        {
            if (_oldCounter != _counter)
            {
                _counterChange?.Invoke(_counter);
                // Debug.Log($"Counter changed: {_oldCounter} -> {_counter}.");
                _oldCounter = _counter;
            }
            if (_oldPredictedCounter != _counter + _predictedDelta)
            {
                _predictedCounterChange?.Invoke(_counter + _predictedDelta);
                // Debug.Log($"Predicted counter changed: {_oldPredictedCounter} -> {_counter + _predictedDelta}.");
                _oldPredictedCounter = _counter + _predictedDelta;
            }
        }

        // Sync the counter to all network observers.
        // When `predictedSpawner` is not null, this doubles as an acknowledgement for the predicted spawning client.
        void GlobalSyncCounter(NetworkConnection predictedSpawner)
        {
            SyncCounterRpc(null, _counter, predictedSpawner);
        }

        // Sync the counter to a specific target client.
        // This is for rejecting predicted spawns, as other observers don't need to get notified about a failed predicted spawn.
        // When `predictedSpawner` is not null, this doubles as an acknowledgement for the predicted spawning client.
        void TargetSyncCounter(NetworkConnection predictedSpawner)
        {
            SyncCounterRpc(predictedSpawner, _counter, predictedSpawner);
        }

        // `BufferLast = true` ensures late joining clients will still get the latest counter value.
        [ObserversRpc(BufferLast = true, ExcludeServer = true)]
        [TargetRpc(ExcludeServer = true)]
        void SyncCounterRpc(NetworkConnection target, int counter, NetworkConnection predictedSpawner)
        {
            _counter = counter;
            if (predictedSpawner != null && predictedSpawner.IsLocalClient)
            {
                // This RPC doubles as a response to our predicted spawn request.
                // Decrement delta only if it is positive.
                // Under normal conditions, the number of prediction responses will exactly match the number of prediction requests.
                // But if the client's observability changed during the sequence, we might ignore prediction responses.
                // To solve this, we reset `_predictedDelta` to 0 when the spawner becomes inobservable, and pretend all predictions were rejected.
                // Thus when a client becomes observable -> prediction request -> inobservable -> `_predictedDelta` reset -> observable -> prediction response, we might have no waiting predictions.
                // That means we've already determined that prediction to be rejected, so leave the `_predictedDelta` as it is.
                if (_predictedDelta >= 1)
                    _predictedDelta -= 1;
            }
            FlushCounterChanges();
        }
    }
}
