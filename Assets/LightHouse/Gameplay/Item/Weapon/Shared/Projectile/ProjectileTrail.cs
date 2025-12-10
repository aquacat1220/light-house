namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;

    public class ProjectileTrail : NetworkBehaviour
    {
        [SerializeField]
        TrailRenderer _trailRenderer;

        void Awake()
        {
            if (_trailRenderer == null)
            {
                Debug.Log("`_trailRenderer` wasn't set.");
                throw new Exception();
            }
        }

        public override void OnStopNetwork()
        {
            _trailRenderer.Clear();
        }
    }
}