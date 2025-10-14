using System;
using System.Collections.Generic;
using FishNet.Component.Observing;
using FishNet.Component.Ownership;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Observing;
using UnityEngine;

public class ProjectileSpawner : NetworkBehaviour
{
    [SerializeField]
    GameObject _projectile;
    [SerializeField]
    Transform _spawnPoint;

    [SerializeField]
    static int _waitQueueCapacity = 5;

    bool _usePredictedSpawn = false;

    Queue<(PreciseTick Tick, ProjectileTransform Projectile)> _waitingProjectiles = new Queue<(PreciseTick Tick, ProjectileTransform Projectile)>(_waitQueueCapacity);
    Queue<(PreciseTick Tick, Vector2 Position, float Rotation)> _waitingTickets = new Queue<(PreciseTick Tick, Vector2 Position, float Rotation)>(_waitQueueCapacity);

    void Awake()
    {
        if (_projectile == null)
        {
            Debug.Log("`_projectile` wasn't set.");
            throw new Exception();
        }
        var nob = _projectile.GetComponent<NetworkObject>();
        if (nob == null)
        {
            Debug.Log("`_projectile` is not a networked object. Is this intended?");
            Debug.Log("We do not support local projectiles.");
            throw new Exception();
        }

        if (nob.PredictedSpawn?.GetAllowSpawning() is true)
        {
            if (_projectile.GetComponent<NetworkObserver>().GetObserverCondition<AlwaysFalseCondition>() == null)
            {
                Debug.Log("`_projectile` was set to predicted-spawn, but doesn't have the `AlwaysFalseCondition` observer condition. We need it to disable observation!");
                throw new Exception();
            }
            if (_projectile.GetComponent<NetworkObserver>().GetObserverCondition<OwnerOnlyCondition>() == null)
            {
                Debug.Log("`_projectile` was set to predicted-spawn, but doesn't have the `OwnerOnlyCondition` observer condition. We need it to despawn on spawning client!");
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

    public void SpawnProjectile()
    {
        if (!_usePredictedSpawn)
        {
            // We are not using predictive spawning.
            // Spawn is only possible on server.
            if (!base.IsServerInitialized)
                return;

            var projectileGameObject = Instantiate(_projectile, _spawnPoint.position, _spawnPoint.rotation);
            var nob = projectileGameObject.GetComponent<ProjectileTransform>().NetworkObject;
            Spawn(
                projectileGameObject,
                null,
                gameObject.scene
            );
            nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
            nob.NetworkObserver.GetObserverCondition<OwnerOnlyCondition>().SetIsEnabled(false);
            return;
        }
        // We are using predictive spawning.
        // Spawn is technically possible on the server too, but we'll be spawning on the owning client only.

        // If we are the owning host, predictive spawning doesn't matter at all.
        if (base.IsServerInitialized && base.IsOwner)
        {
            var projectileGameObject = Instantiate(_projectile, _spawnPoint.position, _spawnPoint.rotation);
            var nob = projectileGameObject.GetComponent<ProjectileTransform>().NetworkObject;
            Spawn(
                projectileGameObject,
                null,
                gameObject.scene
            );
            nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
            nob.NetworkObserver.GetObserverCondition<OwnerOnlyCondition>().SetIsEnabled(false);
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

        Debug.Log($"{TimeManager.Tick}: Attempting to add a ticket to waitlist.");

        PreciseTick tick = TimeManager.GetPreciseTick(TickType.Tick);
        Vector2 position = _spawnPoint.position;
        float rotation = _spawnPoint.rotation.eulerAngles.z;
        Debug.Log($"{TimeManager.Tick}: Ticket looks like - {tick}, {position}, {rotation}.");

        // Check projectile waitlist first before considering adding the projectile to waitlist.
        if (_waitingProjectiles.Count > 0)
        {
            Debug.Log($"{TimeManager.Tick}: Consuming waiting projectile.");
            // We have a waiting projectile.
            (var projectileTick, var projectile) = _waitingProjectiles.Dequeue();
            // TODO: Optionally check if the projectile is too old.
            projectile.transform.position = position;
            projectile.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            projectile.ResetSpawn(tick, position, rotation);
            projectile.enabled = true;
            // TODO: Re-enable all components.

            // Disable both conditions to make the projectile observable to everyone.
            var nob = projectile.NetworkObject;
            nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
            nob.NetworkObserver.GetObserverCondition<OwnerOnlyCondition>().SetIsEnabled(false);
            return;
        }

        // Then check if the ticket waitlist has an empty spot.
        // If it is full, evict a ticket to make room.
        // The evicted ticket will be spawned immediately.
        if (_waitingTickets.Count == _waitQueueCapacity)
        {
            Debug.Log($"{TimeManager.Tick}: Ticket waitlist is full, evicting.");
            var ticket = _waitingTickets.Dequeue();
            var projectileGameObject = Instantiate(_projectile, ticket.Position, Quaternion.Euler(0f, 0f, ticket.Rotation));
            var projectile = projectileGameObject.GetComponent<ProjectileTransform>();
            Spawn(
                projectileGameObject,
                null,
                gameObject.scene
            );
            projectile.ResetSpawn(ticket.Tick, ticket.Position, ticket.Rotation);

            // Disable both conditions to make the projectile observable to everyone.
            var nob = projectile.NetworkObject;
            nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
            nob.NetworkObserver.GetObserverCondition<OwnerOnlyCondition>().SetIsEnabled(false);
            return;
        }

        Debug.Log($"{TimeManager.Tick}: Added a ticket to waitlist.");
        _waitingTickets.Enqueue(
            (TimeManager.GetPreciseTick(TickType.Tick), position, rotation)
        );

        // TODO: Add a timer to dequeue tickets after a set delay.
    }

    [Server]
    public void AddProjectileToWaitlist(ProjectileTransform projectile)
    {
        if (!_usePredictedSpawn)
        {
            Debug.Log($"{TimeManager.Tick}: A projectile was added to waitlist, but the projectile spawner is not set to perform predicted spawning.");
            throw new Exception();
        }

        Debug.Log($"{TimeManager.Tick}: Attempting to add a projectile to waitlist.");
        // Check ticket waitlist first before considering adding the projectile to waitlist.
        if (_waitingTickets.Count > 0)
        {
            Debug.Log($"{TimeManager.Tick}: Consuming waiting ticket.");
            // We have a waiting ticket.
            var ticket = _waitingTickets.Dequeue();
            Debug.Log($"{TimeManager.Tick}: Ticket looks like - {ticket.Tick}, {ticket.Position}, {ticket.Rotation}.");
            // TODO: Optionally check if the ticket is too old.
            projectile.transform.position = ticket.Position;
            projectile.transform.rotation = Quaternion.Euler(0f, 0f, ticket.Rotation);
            projectile.ResetSpawn(ticket.Tick, ticket.Position, ticket.Rotation);
            // TODO: Re-enable all components.

            // Disable both conditions to make the projectile observable to everyone.
            var nob = projectile.GetComponent<NetworkObject>();
            nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
            nob.NetworkObserver.GetObserverCondition<OwnerOnlyCondition>().SetIsEnabled(false);
            return;
        }

        // Then check if the projectile waitlist has an empty spot.
        // If it is full, evict a projectile to make room.
        // The evicted projectile will be despawned immediately.
        if (_waitingProjectiles.Count == _waitQueueCapacity)
        {
            Debug.Log($"{TimeManager.Tick}: Projectile waitlist is full, evicting.");
            (var tick, var evictedProjectile) = _waitingProjectiles.Dequeue();
            Debug.Log($"Projectile spawn request (arrived at {tick}) was denied due to queue eviction.");
            var nob = evictedProjectile.NetworkObject;
            // An eviction can happen if a ticket never arrived.

            // Disabling `AlwaysFalseCondition` will make the owner the only observer.
            // We do this to ensure the spawning client receives the despawn call.
            nob.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(false);
            nob.NetworkObserver.GetObserverCondition<OwnerOnlyCondition>().SetIsEnabled(true);
            nob.Despawn();
        }

        projectile.NetworkObserver.GetObserverCondition<AlwaysFalseCondition>().SetIsEnabled(true);
        projectile.NetworkObserver.GetObserverCondition<OwnerOnlyCondition>().SetIsEnabled(true);
        projectile.enabled = false;
        // TODO: Disable everything except the `NetworkObject` component.
        _waitingProjectiles.Enqueue((TimeManager.GetPreciseTick(TickType.Tick), projectile));
        Debug.Log($"{TimeManager.Tick}: Added a projectile to waitlist.");
    }
}
