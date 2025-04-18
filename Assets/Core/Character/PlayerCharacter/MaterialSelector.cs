using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class MaterialSelector : NetworkBehaviour
{

    // Material to use when the PlayerCharacter is owned.
    [SerializeField]
    Material _owner_material;

    // Material to use when the PlayerCharacter is non-owned.
    [SerializeField]
    Material _nonowner_material;

    // List of sprite renderers to set materials based on ownership.
    [SerializeField]
    List<SpriteRenderer> _sprite_renderers = new List<SpriteRenderer>();

    void Awake()
    {
        if (_owner_material == null)
        {
            Debug.Log("`_owner_material` wasn't set.");
            throw new Exception();
        }

        if (_nonowner_material == null)
        {
            Debug.Log("`_nonowner_material` wasn't set.");
            throw new Exception();
        }

        if (_sprite_renderers.Count == 0)
        {
            Debug.Log("`_sprite_renderers` is an empty list, so the component won't be doing anything. Is this intentional?");
        }
    }

    // Set the material based on ownership.
    // We do this in `OnStartClient()` but not on server because visibility and rendering only matters for the clients.
    // And hosts will run both `OnStartClient()` and `OnStartServer()`.
    public override void OnStartClient()
    {
        if (base.IsOwner)
        {
            foreach (var sprite_renderer in _sprite_renderers)
            {
                sprite_renderer.material = _owner_material;
            }
        }
        else
        {
            foreach (var sprite_renderer in _sprite_renderers)
            {
                sprite_renderer.material = _nonowner_material;
            }
        }
    }
}
