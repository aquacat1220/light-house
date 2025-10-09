using System;
using FishNet.Object;
using UnityEngine;

public class ProjectileSpawner : NetworkBehaviour
{
    [SerializeField]
    GameObject _projectile;
    [SerializeField]
    Transform _spawnPoint;

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
        if (_spawnPoint == null)
        {
            Debug.Log("`_spawnPoint` wasn't set.");
            throw new Exception();
        }
    }

    public void SpawnProjectile()
    {
        GameObject projectile = Instantiate(_projectile, _spawnPoint.position, _spawnPoint.rotation);
        Spawn(projectile, base.Owner, gameObject.scene);
    }
}
