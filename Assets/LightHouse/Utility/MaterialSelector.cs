namespace LightHouse
{
    using System;
    using System.Collections.Generic;
    using FishNet.Connection;
    using FishNet.Object;
    using UnityEngine;

    public class MaterialSelector : NetworkBehaviour
    {

        // Material to use when the networkobject is owned.
        [SerializeField]
        Material _ownerMaterial;
        // Material to use when the networkobject is non-owned.
        [SerializeField]
        Material _nonownerMaterial;

        // List of sprite renderers to set materials based on ownership.
        [SerializeField]
        List<Renderer> _target_renderers;

        void Awake()
        {
            if (_ownerMaterial == null)
            {
                Debug.Log("`_owner_material` wasn't set.");
                throw new Exception();
            }

            if (_nonownerMaterial == null)
            {
                Debug.Log("`_nonowner_material` wasn't set.");
                throw new Exception();
            }

            if (_target_renderers.Count == 0)
            {
                Debug.Log("`_target_renderers` is an empty list, so the component won't be doing anything. Is this intentional?");
            }
        }

        // Set the material based on ownership.
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            if (base.IsOwner)
            {
                foreach (var renderer in _target_renderers)
                {
                    renderer.material = _ownerMaterial;
                }
            }
            else
            {
                foreach (var renderer in _target_renderers)
                {
                    renderer.material = _nonownerMaterial;
                }
            }
        }
    }
}
