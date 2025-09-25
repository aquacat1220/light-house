using System;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class PlayerCharacterInventory : NetworkBehaviour
{
    [SerializeField]
    ItemSlot[] _itemSlots = new ItemSlot[4];

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
            var itemSlotInput = itemSlot.GetComponent<ItemSlotInput>();
            if (itemSlotInput == null)
            {
                Debug.Log("Item slot in `_itemSlots` does not have an item slot input component.");
                throw new Exception();
            }
            _itemSlotInputs[i] = itemSlotInput;
        }
    }

    public void OnSelectItem1(bool isPerformed)
    {
        ChangeMainHand(1);
    }

    public void OnDropItem1(bool isPerformed)
    {
        DropItem(1);
    }

    public void OnSelectItem2(bool isPerformed)
    {
        ChangeMainHand(2);
    }

    public void OnDropItem2(bool isPerformed)
    {
        DropItem(2);
    }

    public void OnSelectItem3(bool isPerformed)
    {
        ChangeMainHand(3);
    }

    public void OnDropItem3(bool isPerformed)
    {
        DropItem(3);
    }

    public void OnSelectItem4(bool isPerformed)
    {
        ChangeMainHand(4);
    }

    public void OnDropItem4(bool isPerformed)
    {
        DropItem(4);
    }

    void ChangeMainHand(int newMainHand)
    {
        if (newMainHand - 1 == _mainHand)
            return;
        _subHand = _mainHand;
        _mainHand = newMainHand - 1;
    }

    void DropItem(int hand)
    {
        _itemSlots[hand - 1].Unequip();
    }
}
