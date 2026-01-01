namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;
    using UnityEngine.VFX;

    public class ProjectileTrailController : NetworkBehaviour
    {
        [SerializeField]
        VisualEffect _projectileTrail;

        void Awake()
        {
            if (_projectileTrail == null)
            {
                Debug.Log("`_projectileTrail` wasn't set.");
                throw new Exception();
            }
        }

        void OnEnable()
        {
            _projectileTrail.Play();
        }

        void OnDisable()
        {
            _projectileTrail.Stop();
        }
    }
}