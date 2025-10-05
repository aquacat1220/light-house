using System;
using FishNet.Object;
using UnityEngine;

public class ItemSlot : NetworkBehaviour
{
    // The item this slot is equipping. Defaults to `null`, which means the slot is unoccupied.
    public Item Item { get; private set; }

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
        // Here we use `base.IsServerStarted` instead of `base.IsServerOnlyStarted` because we need the logic to
        // run before the ownership removal on hosts too.
        // See 2025-09-29 17:06:28 for details.
        EquipRpc(null);
        if (base.IsServerStarted)
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
            // `Item.UnregisterInner()` must always come before the matching `ItemSlot.UnequipInner()`
            // to ensure `Item.ItemSlot` is set to the last itemslot during unregister callback.
            Item?.UnregisterInner();
            UnequipInner();

            ItemSlot oldItemSlot = item.ItemSlot;
            item.UnregisterInner();
            oldItemSlot?.UnequipInner();

            // Then link the item and slot together.
            // `Item.RegisterInner()` must always come after the matching `ItemSlot.EquipInner()`
            // to ensure `Item.ItemSlot` is set to the latest itemslot during register callback.
            EquipInner(item);
            item.RegisterInner(this);
        }
        else
        {
            // `item` is null. We are attempting to unequip any item (if it exists) from this slot.
            Item?.UnregisterInner();
            UnequipInner();
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
}
