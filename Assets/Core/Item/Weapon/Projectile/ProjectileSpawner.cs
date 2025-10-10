using System;
using FishNet.Component.Ownership;
using FishNet.Object;
using UnityEngine;

public class ProjectileSpawner : NetworkBehaviour
{
    [SerializeField]
    GameObject _projectile;
    [SerializeField]
    Transform _spawnPoint;

    bool _usePredictedSpawn = false;

    void Awake()
    {
        if (_projectile == null)
        {
            Debug.Log("`_projectile` wasn't set.");
            throw new Exception();
        }
        if (_projectile.GetComponent<NetworkObject>() == null)
        {
            Debug.Log("`_projectile` is not a networked object. Is this intended?");
            Debug.Log("We do not support local projectiles.");
            throw new Exception();
        }
        if (_projectile.GetComponent<PredictedSpawn>()?.GetAllowSpawning() is true)
            _usePredictedSpawn = true;

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
            Debug.Log("Non-predicted spawning.");
            Spawn(
                Instantiate(_projectile, _spawnPoint.position, _spawnPoint.rotation),
                base.Owner,
                gameObject.scene
            );
            return;
        }
        // We are using predictive spawning.
        // Spawn is technically possible on the server too, but we'll be spawning on the owning client only.

        // Skip spawning if we are not the owner.
        if (!base.IsOwner)
            return;
        Debug.Log("Predicted spawning.");
        Spawn(
            Instantiate(_projectile, _spawnPoint.position, _spawnPoint.rotation),
            base.Owner,
            gameObject.scene
        );
    }
}
