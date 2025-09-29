using System;
using FishNet.Object;
using UnityEngine;

public class PlayerCharacterInventory : NetworkBehaviour
{
    [SerializeField]
    ItemSlot[] _itemSlots = new ItemSlot[4];
    [SerializeField]
    Transform _mainItemAnchor;
    [SerializeField]
    Transform _subItemAnchor;


    Transform[] _itemSlotAnchors = new Transform[4];
    ItemSlotInput[] _itemSlotInputs = new ItemSlotInput[4];

    int _mainHand = 0;
    int _subHand = 1;

    public void Awake()
    {
        if (_itemSlots == null || _itemSlots.Length != 4)
        {
            Debug.Log("`_itemSlots` must be an array of size 4.");
            throw new Exception();
        }
        if (_mainItemAnchor == null)
        {
            Debug.Log("`_mainItemAnchor` wasn't set.");
            throw new Exception();
        }
        if (_subItemAnchor == null)
        {
            Debug.Log("`_subItemAnchor` wasn't set.");
            throw new Exception();
        }

        for (int i = 0; i < 4; i++)
        {
            var itemSlot = _itemSlots[i];
            if (itemSlot == null)
            {
                Debug.Log("Item slots in `_itemSlots` must be non-null references.");
                throw new Exception();
            }
            for (int j = 0; j < i; j++)
            {
                var otherItemSlot = _itemSlots[j];
                if (otherItemSlot == itemSlot)
                {
                    Debug.Log("Item slots in `_itemSlots` must be distinct references.");
                    throw new Exception();
                }
            }

            var itemSlotAnchor = itemSlot.transform.parent;
            if (itemSlotAnchor == null)
            {
                Debug.Log("Item slot in `_itemSlots` does not have a parent transform. How is that possible?");
                throw new Exception();
            }
            _itemSlotAnchors[i] = itemSlotAnchor;

            var itemSlotInput = itemSlot.GetComponent<ItemSlotInput>();
            if (itemSlotInput == null)
            {
                Debug.Log("Item slot in `_itemSlots` does not have an item slot input component.");
                throw new Exception();
            }
            _itemSlotInputs[i] = itemSlotInput;
        }

        // For correctness, ensure the main/sub hand itemslots start equipped on the main/sub anchors!
        _itemSlots[_mainHand].transform.SetParent(_mainItemAnchor, worldPositionStays: false);
        _itemSlots[_subHand].transform.SetParent(_subItemAnchor, worldPositionStays: false);
    }

    [Server]
    public bool AddItem(Item item)
    {
        foreach (var itemSlot in _itemSlots)
        {
            // `ItemSlot.Equip()` returns true only if both the slot and the item was non-null and unequipped.
            if (itemSlot.Equip(item))
                return true;
        }
        return false;
    }

    [Client(RequireOwnership = true)]
    public void OnPrimary(bool isPerformed)
    {
        _itemSlotInputs[_mainHand].OnPrimary(isPerformed);
    }

    [Client(RequireOwnership = true)]
    public void OnSecondary(bool isPerformed)
    {
        _itemSlotInputs[_mainHand].OnSecondary(isPerformed);
    }

    [Client(RequireOwnership = true)]
    public void OnSelectItem1(bool isPerformed)
    {
        if (!isPerformed)
            return;
        ChangeMainHand(0);
    }

    [Client(RequireOwnership = true)]
    public void OnDropItem1(bool isPerformed)
    {
        if (!isPerformed)
            return;
        DropItem(0);
    }

    [Client(RequireOwnership = true)]
    public void OnSelectItem2(bool isPerformed)
    {
        if (!isPerformed)
            return;
        ChangeMainHand(1);
    }

    [Client(RequireOwnership = true)]
    public void OnDropItem2(bool isPerformed)
    {
        if (!isPerformed)
            return;
        DropItem(1);
    }

    [Client(RequireOwnership = true)]
    public void OnSelectItem3(bool isPerformed)
    {
        if (!isPerformed)
            return;
        ChangeMainHand(2);
    }

    [Client(RequireOwnership = true)]
    public void OnDropItem3(bool isPerformed)
    {
        if (!isPerformed)
            return;
        DropItem(2);
    }

    [Client(RequireOwnership = true)]
    public void OnSelectItem4(bool isPerformed)
    {
        if (!isPerformed)
            return;
        ChangeMainHand(3);
    }

    [Client(RequireOwnership = true)]
    public void OnDropItem4(bool isPerformed)
    {
        if (!isPerformed)
            return;
        DropItem(3);
    }

    [ServerRpc(RequireOwnership = true)]
    void ChangeMainHand(int newMainHand)
    {
        if (newMainHand == _mainHand)
            return;
        ChangeMainHandRpc(newMainHand, _mainHand);
    }

    [ServerRpc(RequireOwnership = true)]
    void DropItem(int hand)
    {
        _itemSlots[hand].Unequip();
        // We don't need bufferlast observerrpcs to sync this, since `ItemSlot` already handles that.
    }

    [ObserversRpc(BufferLast = true, RunLocally = true)]
    void ChangeMainHandRpc(int newMainHand, int newSubHand)
    {
        if (newMainHand == newSubHand)
        {
            Debug.Log("`newMainHand == newSubHand`, which shouldn't be possible with correct server-side checks.");
            throw new Exception();
        }
        // First reposition the old main/subhand item slots back to where they belong.
        _itemSlots[_mainHand].transform.SetParent(_itemSlotAnchors[_mainHand], worldPositionStays: false);
        _itemSlots[_subHand].transform.SetParent(_itemSlotAnchors[_subHand], worldPositionStays: false);

        // And ensure inputs are canceled for the old main hand.
        _itemSlotInputs[_mainHand].OnPrimary(false);
        _itemSlotInputs[_mainHand].OnSecondary(false);

        _mainHand = newMainHand;
        _subHand = newSubHand;

        // Then position the new main/subhand item slots!
        _itemSlots[_mainHand].transform.SetParent(_mainItemAnchor, worldPositionStays: false);
        _itemSlots[_subHand].transform.SetParent(_subItemAnchor, worldPositionStays: false);
    }
}
