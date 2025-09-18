using System;
using FishNet.Object;
using UnityEngine;

public class ItemSlot : NetworkBehaviour
{
    // The item this slot is equipping. Defaults to `null`, which means the slot is unoccupied.
    public Item Item;

    [Server]
    public bool Equip(Item item)
    {
        // If this slot is occupied, or the item we are trying to equip is null, or the item we are trying to equip is already equipped by another slot, return false and abort.
        if (Item != null || item == null || item.ItemSlot != null)
            return false;
        // Give ownership of item before any RPC calls, so clients will expect correct ownerships on their callbacks.
        item.GiveOwnership(base.Owner);
        // A `RunLocally = true` RPC seemingly does the same thing, but comes with a small issue.
        // On hosts, this causes the RPC to execute before the items have time to client-init.
        // It is reasonable for items to assume their callbacks will be triggered only after all network stuff is inited.
        // See 2025-09-18 11:22:40 for in depth info.
        EquipRpc(item);
        if (base.IsServerOnlyStarted)
            EquipLocal(item);
        item.Register(this);
        return true;
    }

    [Server]
    public bool Unequip()
    {
        if (Item == null)
            return false;
        Item oldItem = Item;
        EquipRpc(null);
        if (base.IsServerOnlyStarted)
            EquipLocal(null);
        oldItem.Unregister();
        // Remove ownership after RPC calls, so clients will still have ownership on their callbacks.
        oldItem.RemoveOwnership();
        return true;
    }

    [ObserversRpc(BufferLast = true)]
    void EquipRpc(Item item)
    {
        EquipLocal(item);
    }

    void EquipLocal(Item item)
    {
        if (item == Item)
        {
            // Either of two cases:
            // 1. We are attempting to equip an item that is already equipped to this slot.
            // 2. We are attempting to unequip items from this slot, which is already unoccupied.
            // Both are no-ops.
            return;
        }
        if (item != null)
        {
            // `item` is not null. We are attempting to equip an item to this slot.

            // First unlink all items and item slots participating in this new link formation.
            Item oldItem = Item;
            UnequipInner();
            oldItem?.UnregisterInner();

            item.ItemSlot?.UnequipInner();
            item.UnregisterInner();

            // Then link the item and slot together.
            EquipInner(item);
            item.RegisterInner(this);
        }
        else
        {
            // `item` is null. We are attempting to unequip any item (if it exists) from this slot.
            Item oldItem = Item;
            UnequipInner();
            oldItem?.UnregisterInner();
        }
    }

    public void EquipInner(Item item)
    {
        if (Item == item)
            return;
        if (Item != null)
        {
            Debug.Log("`EquipInner()` was called while having non-null `Item`.");
            throw new Exception();
        }
        Item = item;
    }

    public void UnequipInner()
    {
        if (Item == null)
            return;
        Item = null;
    }

    public T FindComponent<T>()
    {
        T fromSlot = GetComponent<T>();
        if (fromSlot != null)
            return fromSlot;
        return transform.parent.GetComponent<T>();
    }
}
