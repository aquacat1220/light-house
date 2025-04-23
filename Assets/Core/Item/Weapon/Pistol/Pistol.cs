using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

public class Pistol : NetworkBehaviour
{
    [SerializeField]
    Item _item;
    [SerializeField]
    SingleFire _singleFire;
    [SerializeField]
    Light2D _muzzleFlash;

    ItemSystem _itemSystem;
    Hand _hand;

    public void Awake()
    {
        if (_item == null)
        {
            Debug.Log("`_item` wasn't set.");
            throw new Exception();
        }
        _item.RegisterImpl = Register;
        _item.UnregisterImpl = Unregister;

        if (_singleFire == null)
        {
            Debug.Log("`_singleFire` wasn't set.");
            throw new Exception();
        }

        if (_muzzleFlash == null)
        {
            Debug.Log("`_muzzleFlash` wasn't set.");
            throw new Exception();
        }
    }

    bool Register(ItemRegisterContext registerContext)
    {
        if (registerContext is PlayerCharacterItemRegisterContext playerCharacterItemRegisterContext)
        {
            var itemSystem = playerCharacterItemRegisterContext.ItemSystem;
            var hand = playerCharacterItemRegisterContext.Hand;
            if (hand == Hand.Left)
            {
                transform.SetParent(itemSystem.LeftItemAnchor, false);
            }
            else
            {
                transform.SetParent(itemSystem.RightItemAnchor, false);
            }
            if (base.IsServerInitialized)
            {
                _singleFire.Register(Fire);
            }
            if (base.IsOwner)
            {
                if (hand == Hand.Left)
                {
                    itemSystem.LeftItemPrimary += OnPrimary;
                }
                else
                {
                    itemSystem.RightItemPrimary += OnPrimary;
                }
            }
            _itemSystem = itemSystem;
            _hand = hand;
            return true;

        }
        return false;
    }

    void Unregister()
    {
        Assert.IsNotNull(_itemSystem);
        if (_itemSystem is PlayerCharacterItemSystem itemSystem)
        {
            transform.SetParent(null, true);
            if (base.IsServerInitialized)
            {
                _singleFire.Unregister();
            }
            if (base.IsOwner)
            {
                if (_hand == Hand.Left)
                {
                    itemSystem.LeftItemPrimary -= OnPrimary;
                }
                else
                {
                    itemSystem.RightItemPrimary -= OnPrimary;
                }
            }
            _itemSystem = null;
            return;
        }

        Debug.Log("`Pistol` encountered unknown `ItemSystem` variant during `Unregister()`, which shouldn't have been `Register()`ed in the first place.");
        throw new Exception();
    }

    void OnPrimary()
    {
        _singleFire.TryFireClient();
    }

    void Fire()
    {
        // Due to `SingleFire` implementations, this function gets called only on the server.
        // It's our responsibility to sync the firing logic back to clients and observers.
        Debug.Log("Fired");
    }

}
