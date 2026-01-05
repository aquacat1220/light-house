namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;

    public class Item : NetworkBehaviour
    {
        public event Action<ItemSlot> Register;
        public event Action Unregister;

        // The item slot this item is registered to. Defaults to `null`, which means the item isn't registered to anything.
        public ItemSlot ItemSlot { get; private set; }

        [Server]
        public void RegisterTo(ItemSlot itemSlot)
        {
            RegisterRpc(itemSlot);
            if (base.IsServerOnlyStarted)
                RegisterLocal(itemSlot);
        }

        [Server]
        public void UnregisterFrom()
        {
            RegisterRpc(null);
            if (base.IsServerStarted)
                RegisterLocal(null);
        }

        [ObserversRpc(BufferLast = true)]
        void RegisterRpc(ItemSlot itemSlot)
        {
            RegisterLocal(itemSlot);
        }

        void RegisterLocal(ItemSlot itemSlot)
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
                ItemSlot oldItemSlot = ItemSlot;
                UnregisterInner();
                oldItemSlot?.UnequipInner();

                itemSlot.Item?.UnregisterInner();
                itemSlot.UnequipInner();

                // Then link the item and slot together.
                itemSlot.EquipInner(this);
                RegisterInner(itemSlot);
            }
            else
            {
                ItemSlot oldItemSlot = ItemSlot;
                UnregisterInner();
                oldItemSlot?.UnequipInner();
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
            Register?.Invoke(itemSlot);
        }

        public void UnregisterInner()
        {
            if (ItemSlot == null)
                return;
            Unregister?.Invoke();
            ItemSlot = null;
        }
    }
}
