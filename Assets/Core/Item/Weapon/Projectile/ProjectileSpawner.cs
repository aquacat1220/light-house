using System;
using System.Collections.Generic;
using FishNet.Component.Observing;
using FishNet.Component.Ownership;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Observing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class ProjectileSpawner : NetworkBehaviour
{
    [SerializeField]
    GameObject _projectile;
    [SerializeField]
    Transform _spawnPoint;

    [SerializeField]
    static int _waitQueueCapacity = 5;
    [SerializeField]
    static float _maxWaitTime = 0.025f;

    // These two events are for correcting incorrect predictions.
    // This one is for over-prediction; we predicted a spawn, but it didn't happen on the server.
    [SerializeField]
    UnityEvent _overPredicted;
    // This one is for under-prediction; we never expected a spawn, but the server spawned one.
    [SerializeField]
    UnityEvent _underPredicted;

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

    public void InvokeOverPredicted()
    {
        if (base.IsServerInitialized)
        {
            Debug.Log("`InvokeOverPredicted()` can't be called on servers.");
            throw new Exception();
        }
        if (!_usePredictedSpawn)
        {
            // `InvokeOverPredicted()` can never be called on non-predicting spawners, as it is called only when our prediction is rejected.
            Debug.Log("`InvokeOverPredicted()` can't be called on non-predicting spawners.");
            throw new Exception();
        }
        _overPredicted?.Invoke();
    }

    public void InvokeUnderPredicted()
    {
        if (base.IsServerInitialized)
        {
            Debug.Log("`InvokeUnderPredicted()` can't be called on servers.");
            throw new Exception();
        }
        if (!_usePredictedSpawn)
        {
            // However `InvokeUnderPredicted()` can be called on non-predicting spawners, as it is called when a client received a non-predicted spawn.
            // `_underPredicted` should only be triggered when we under-predicted, not when we never attempted to predict at all. 
            return;
        }
        _underPredicted?.Invoke();
    }

    void ClearWaitlist()
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
                nob.GiveOwnership(nob.PredictedSpawner);
                nob.Despawn();
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
                continue;
            }
            break;
        }

        if (_waitingProjectiles.Count == 0 && _waitingTickets.Count == 0)
        {
            _clearWaitlistAlarm.Stop();
        }
    }

    public void SpawnProjectile()
    {
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
            return;
        }

        // If we are the server but not a client, add a ticket.
        if (base.IsServerInitialized)
        {
            AddTicketToWaitlist();
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
            return;
        }

        _waitingTickets.Enqueue(
            (TimeManager.GetPreciseTick(TickType.Tick), position, rotation)
        );
        _clearWaitlistAlarm.Start();
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
            nob.GiveOwnership(nob.PredictedSpawner);
            nob.Despawn();
        }

        // This line isn't needed, but just to make sure the condition is enabled before adding to the waitlist.
        projectile.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(true);
        projectile.SetActive(false);
        _waitingProjectiles.Enqueue((TimeManager.GetPreciseTick(TickType.Tick), projectile));
        _clearWaitlistAlarm.Start();
    }
}
