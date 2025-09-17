using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class Item : NetworkBehaviour
{
    [SerializeField]
    UnityEvent<ItemSlot> _register;
    [SerializeField]
    UnityEvent _unregister;

    // The item slot this item is registered to. Defaults to `null`, which means the item isn't registered to anything.
    public ItemSlot ItemSlot;

    [Server]
    public void Register(ItemSlot itemSlot)
    {
        RegisterRpc(itemSlot);
    }

    [Server]
    public void Unregister()
    {
        RegisterRpc(null);
    }

    [ObserversRpc(BufferLast = true, RunLocally = true)]
    void RegisterRpc(ItemSlot itemSlot)
    {
        if (itemSlot == ItemSlot)
        {
            // Either of two cases:
            // 1. We are attempting to register an item slot that is already registered to this item.
            // 2. We are attempting to unregister everything from this item, which isn't registerd to anything in the first place.
            // Both are no-ops.
            return;
        }
        if (itemSlot != null)
        {
            // `itemSlot` is not null. We are attempting to register a slot to this item.

            // First unlink all items and item slots particiapting in this new link formation.
            ItemSlot?.UnequipInner();
            UnregisterInner();

            Item oldItem = itemSlot.Item;
            itemSlot.UnequipInner();
            oldItem?.UnregisterInner();

            // Then link the item and slot together.
            itemSlot.EquipInner(this);
            RegisterInner(itemSlot);
        }
        else
        {
            ItemSlot?.UnequipInner();
            UnregisterInner();
        }
    }

    public void RegisterInner(ItemSlot itemSlot)
    {
        if (ItemSlot == itemSlot)
            return;
        if (ItemSlot != null)
        {
            Debug.Log("`RegisterInner()` was called while having non-null `ItemSlot`.");
            throw new Exception();
        }
        ItemSlot = itemSlot;
        _register?.Invoke(itemSlot);
    }

    public void UnregisterInner()
    {
        if (ItemSlot == null)
            return;
        ItemSlot = null;
        _unregister?.Invoke();
    }
}
