using System;
using System.Linq;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Observing;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemRemakeTest : MonoBehaviour
{
    [SerializeField]
    ItemSlot _a;
    [SerializeField]
    ItemSlot _b;

    [SerializeField]
    Item _x;
    [SerializeField]
    Item _y;
    [SerializeField]
    Item _z;

    ItemSlot _selected;

    bool _started = false;
    bool _observing = true;

    void Awake()
    {
        if (_a == null || _b == null || _x == null || _y == null || _z == null)
            throw new Exception();
    }

    public void OnXRegister(ItemSlot itemSlot)
    {
        Debug.Log($"Item X registered to slot {itemSlot.gameObject.name}.");
        Debug.Log($"A: {_a.Item?.gameObject.name}, B: {_b.Item?.gameObject.name}, X: {_x.ItemSlot?.gameObject.name}, Y: {_y.ItemSlot?.gameObject.name}, Z: {_z.ItemSlot?.gameObject.name}");
    }

    public void OnYRegister(ItemSlot itemSlot)
    {
        Debug.Log($"Item Y registered to slot {itemSlot.gameObject.name}.");
        Debug.Log($"A: {_a.Item?.gameObject.name}, B: {_b.Item?.gameObject.name}, X: {_x.ItemSlot?.gameObject.name}, Y: {_y.ItemSlot?.gameObject.name}, Z: {_z.ItemSlot?.gameObject.name}");
    }

    public void OnZRegister(ItemSlot itemSlot)
    {
        Debug.Log($"Item Z registered to slot {itemSlot.gameObject.name}.");
        Debug.Log($"A: {_a.Item?.gameObject.name}, B: {_b.Item?.gameObject.name}, X: {_x.ItemSlot?.gameObject.name}, Y: {_y.ItemSlot?.gameObject.name}, Z: {_z.ItemSlot?.gameObject.name}");
    }

    public void OnXUnregister()
    {
        Debug.Log($"Item X unregistered.");
        Debug.Log($"A: {_a.Item?.gameObject.name}, B: {_b.Item?.gameObject.name}, X: {_x.ItemSlot?.gameObject.name}, Y: {_y.ItemSlot?.gameObject.name}, Z: {_z.ItemSlot?.gameObject.name}");
    }

    public void OnYUnregister()
    {
        Debug.Log($"Item Y unregistered.");
        Debug.Log($"A: {_a.Item?.gameObject.name}, B: {_b.Item?.gameObject.name}, X: {_x.ItemSlot?.gameObject.name}, Y: {_y.ItemSlot?.gameObject.name}, Z: {_z.ItemSlot?.gameObject.name}");
    }

    public void OnZUnregister()
    {
        Debug.Log($"Item Z unregistered.");
        Debug.Log($"A: {_a.Item?.gameObject.name}, B: {_b.Item?.gameObject.name}, X: {_x.ItemSlot?.gameObject.name}, Y: {_y.ItemSlot?.gameObject.name}, Z: {_z.ItemSlot?.gameObject.name}");
    }

    void Update()
    {
        if (!_started)
        {
            if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                Debug.Log("Attempting to start as server...");
                if (InstanceFinder.ServerManager.StartConnection(7902))
                {
                    Debug.Log("Success! Started as server!");
                    MatchCondition.AddToMatch(1, _a.NetworkObject);
                    MatchCondition.AddToMatch(1, _b.NetworkObject);
                    MatchCondition.AddToMatch(1, _x.NetworkObject);
                    MatchCondition.AddToMatch(1, _y.NetworkObject);
                    MatchCondition.AddToMatch(1, _z.NetworkObject);
                    _started = true;
                }
                else
                    Debug.Log("Failure...");
            }
            else if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                Debug.Log("Attempting to start as client...");
                if (InstanceFinder.ClientManager.StartConnection("127.0.0.1", 7902))
                {
                    Debug.Log("Success! Started as client!");
                    _started = true;
                }
                else
                    Debug.Log("Failure...");
            }
        }

        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            if (_observing)
            {
                MatchCondition.RemoveFromMatch(1, InstanceFinder.ServerManager.Clients.Values.ToArray(), null);
                Debug.Log("Turning observation off.");
                _observing = false;
            }
            else
            {
                MatchCondition.AddToMatch(1, InstanceFinder.ServerManager.Clients.Values.ToArray());
                Debug.Log("Turning observation on.");
                _observing = true;
            }
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log($"A: {_a.Item?.gameObject.name}, B: {_b.Item?.gameObject.name}, X: {_x.ItemSlot?.gameObject.name}, Y: {_y.ItemSlot?.gameObject.name}, Z: {_z.ItemSlot?.gameObject.name}");
            Debug.Log($"{MatchCondition.GetMatchConnections().GetValueOrDefault(1)}, {MatchCondition.GetMatchObjects().GetValueOrDefault(1)}");
        }

        if (_selected == null)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                _selected = _a;
                Debug.Log($"Selected item slot A, currently equipping {_a.Item?.gameObject.name}");
            }
            else if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                _selected = _b;
                Debug.Log($"Selected item slot B, currently equipping {_b.Item?.gameObject.name}");
            }
        }

        else
        {
            if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                Debug.Log($"Equipping item X to selected slot, succeeded: {_selected.Equip(_x)}");
                _selected = null;
            }
            else if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                Debug.Log($"Equipping item Y to selected slot, succeeded: {_selected.Equip(_y)}");
                _selected = null;
            }
            else if (Keyboard.current.zKey.wasPressedThisFrame)
            {
                Debug.Log($"Equipping item Z to selected slot, succeeded: {_selected.Equip(_z)}");
                _selected = null;
            }
            else if (Keyboard.current.uKey.wasPressedThisFrame)
            {
                Debug.Log($"Unequipping from selected slot, succeeded: {_selected.Unequip()}");
                _selected = null;
            }
        }
    }
}
