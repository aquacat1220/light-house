namespace LightHouse
{
    using System;
    using UnityEngine;
    using UnityEngine.VFX;

    public class MuzzleSparkController : MonoBehaviour
    {
        [SerializeField]
        ProjectileSpawner _projectileSpawner;
        [SerializeField]
        VisualEffect _muzzleSpark;

        int _oldPredictedCounter;

        void Awake()
        {
            if (_projectileSpawner == null)
            {
                Debug.Log("`_projectileSpawner` was not set.");
                throw new Exception();
            }
            _oldPredictedCounter = _projectileSpawner.PredictedCounter;
            _projectileSpawner.PredictedCounterChange += OnPredictedCounterChange;

            if (_muzzleSpark == null)
            {
                Debug.Log("`_muzzleSpark` was not set.");
                throw new Exception();
            }
        }

        void OnPredictedCounterChange(int newPredictedCounter)
        {
            if (_oldPredictedCounter < newPredictedCounter)
            {
                // On the server, this is when we spawn a projectile.
                // On the client, this is when we observe an authoritative spawn, or predict a spawn locally.
                _muzzleSpark.Play();
            }
            _oldPredictedCounter = newPredictedCounter;
        }
    }
}