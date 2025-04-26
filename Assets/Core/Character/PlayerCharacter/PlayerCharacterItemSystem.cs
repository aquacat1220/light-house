using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public enum Hand
{
    Left,
    Right
}

public class PlayerCharacterItemRegisterContext : ItemRegisterContext
{
    public PlayerCharacterItemSystem ItemSystem;
    // The hand to equip the item.
    public Hand Hand;

    public PlayerCharacterItemRegisterContext(PlayerCharacterItemSystem itemSystem, Hand hand)
    {
        ItemSystem = itemSystem;
        Hand = hand;
    }
}

public class PlayerCharacterItemSystem : ItemSystem
{
    // References to item-related actions.
    [SerializeField]
    InputActionReference _itemPrimaryActionRef;
    [SerializeField]
    InputActionReference _itemSecondaryActionRef;

    // Initial items to equip to each hand.
    [SerializeField]
    GameObject _initLeftItem = null;
    [SerializeField]
    GameObject _initRightItem = null;

    // References to anchors to attach the items.
    public Transform LeftItemAnchor;
    public Transform RightItemAnchor;

    // Is the component subscribed to input actions?
    bool _isSubscribedToInputActions = false;

    public event Action LeftItemPrimary;
    public event Action LeftItemSecondary;
    public event Action RightItemPrimary;
    public event Action RightItemSecondary;

    // The current active hand.
    Hand _activeHand = Hand.Right;

    // Items equipped to each hands.
    Item _leftItem = null;
    Item _rightItem = null;

    public void Awake()
    {
        if (_itemPrimaryActionRef == null)
        {
            Debug.Log("`_itemPrimaryActionRef` wasn't set.");
            throw new Exception();
        }
        if (_itemSecondaryActionRef == null)
        {
            Debug.Log("`_itemSecondaryActionRef` wasn't set.");
            throw new Exception();
        }
    }

    public override void OnStartServer()
    {
        // If initial items are indeed `Item`s, spawn them here so they will be initialized.
        if (_initLeftItem != null && _initLeftItem.GetComponent<Item>() != null)
        {
            _initLeftItem = Instantiate(_initLeftItem);
            base.Spawn(_initLeftItem);

        }
        if (_initRightItem != null && _initRightItem.GetComponent<Item>() != null)
        {
            _initRightItem = Instantiate(_initRightItem);
            base.Spawn(_initRightItem);
        }
        // If we are a dedicated server, register to items here.
        if (base.IsServerOnlyStarted)
            RegisterInit();
    }

    public override void OnStartClient()
    {
        // If we are a host, init here.
        if (base.IsServerStarted)
            RegisterInit();
        if (base.IsOwner)
        {
            // We are the owner of this character. Subscribe events to the input actions.
            SubscribeToAction();
        }
    }

    // `RegisterInit()` calls a `RunLocally = true` observer RPC.
    // This can get problematic if we are calling it on a host before client init,
    // since it will immediately run locally.
    [Server]
    void RegisterInit()
    {
        // If initial items are indeed `Item`s, spawn them and register to them.
        if (_initLeftItem != null && _initLeftItem.GetComponent<Item>() != null)
        {
            TryRegisterItem(_initLeftItem.GetComponent<Item>(), Hand.Left);
            _initLeftItem = null;
        }
        if (_initRightItem != null && _initRightItem.GetComponent<Item>() != null)
        {
            TryRegisterItem(_initRightItem.GetComponent<Item>(), Hand.Right);
            _initRightItem = null;
        }
    }

    // public override void OnStopServer()
    // {
    //     // Unregister all items.
    //     UnregisterItem(Hand.Left);
    //     UnregisterItem(Hand.Right);
    // }

    public override void OnStopClient()
    {
        UnsubscribeFromAction();
    }

    void OnEnable()
    {
        if (base.IsOwner)
        {
            // We are the owner of this character. Subscribe events to the input actions.
            // We need this functionality because we unsubscribe on disable.
            SubscribeToAction();
        }
    }

    void OnDisable()
    {
        // We don't check for ownership here, since calling `UnsubscribeFromAction()` when we are not subscribed shouldn't cause any problems.
        UnsubscribeFromAction();
    }

    void SubscribeToAction()
    {
        if (!_isSubscribedToInputActions)
        {
            _itemPrimaryActionRef.action.performed += OnItemPrimary;
            _itemSecondaryActionRef.action.performed += OnItemSecondary;
            _isSubscribedToInputActions = true;
        }
    }

    void UnsubscribeFromAction()
    {
        if (_isSubscribedToInputActions)
        {
            _itemPrimaryActionRef.action.performed -= OnItemPrimary;
            _itemSecondaryActionRef.action.performed -= OnItemSecondary;
            _isSubscribedToInputActions = false;
        }
    }

    // Propagates item-inputs down the correct item, based on the active hand.
    [Client(RequireOwnership = true)]
    void OnItemPrimary(InputAction.CallbackContext context)
    {
        switch (_activeHand)
        {
            case Hand.Left:
                LeftItemPrimary?.Invoke();
                break;
            case Hand.Right:
                RightItemPrimary?.Invoke();
                break;
        }
    }

    // Propagates item-inputs down the correct item, based on the active hand.
    [Client(RequireOwnership = true)]
    void OnItemSecondary(InputAction.CallbackContext context)
    {
        switch (_activeHand)
        {
            case Hand.Left:
                LeftItemSecondary?.Invoke();
                break;
            case Hand.Right:
                RightItemSecondary?.Invoke();
                break;
        }
    }

    // Attempt to register `item` to `hand`, and sync it to all other clients.
    // If hand is already occupied, unregisters the registered item.
    [Server]
    public void TryRegisterItem(Item item, Hand hand)
    {
        // If we are not the server, ignore the call.
        if (!base.IsServerInitialized)
            return;

        // If `item` is `null`, ignore.
        // Unregistering should be done with `UnregisterItem()`.
        if (item == null)
            return;

        if (hand == Hand.Left)
        {
            UnregisterItem(hand);
            Assert.IsNull(_leftItem);
            // Give ownership first, so that the owner will receive ownership before attempting register.
            item.GiveOwnership(base.Owner);
            TryRegisterItemObserver(item, hand);
            if (_leftItem == null)
                item.RemoveOwnership();
        }
        else
        {
            UnregisterItem(hand);
            Assert.IsNull(_rightItem);
            // Give ownership first, so that the owner will receive ownership before attempting register.
            item.GiveOwnership(base.Owner);
            TryRegisterItemObserver(item, hand);
            if (_rightItem == null)
                item.RemoveOwnership();
        }
    }

    // Attempt to register `item` to `hand` on observers.
    // If `item == null`, unregisters any item on `hand` instead.
    // We do this instead of having a separate RPC because
    // only the last register/unregister call (which ever comes last)
    // matters to newly joining observers, and `BufferLast = true`
    // will ensure that.
    // And as a side effect, register->unregister will count as a no-op to
    // new observers, since `TryRegisterItemLocal()` is a no-op for
    // empty hand unregistration.
    [ObserversRpc(RunLocally = true, BufferLast = true)]
    void TryRegisterItemObserver(Item item, Hand hand)
    {
        TryRegisterItemLocal(item, hand);
    }

    // Attempts to register `item` in `hand` locally.
    // If `item == null`, unregisters any item on `hand` instead.
    // Registering to a non-empty hand is forbidden (and shouldn't really happen).
    // Unregistering a empty hand is OK (and might happen on newly joining observers).
    // This function doesn't sync anything: without consideration, `this` will remain desynced until the next valid call.
    void TryRegisterItemLocal(Item item, Hand hand)
    {
        // `item == null` case. Attempt to unregister.
        if (item == null)
        {
            if (hand == Hand.Left)
            {
                if (_leftItem == null)
                    return;
                _leftItem.Unregister();
                _leftItem = null;
            }
            else
            {
                if (_rightItem == null)
                    return;
                _rightItem.Unregister();
                _rightItem = null;
            }
            return;
        }
        // `item != null` case. Attempt to register.
        if (hand == Hand.Left)
        {
            Assert.IsNull(_leftItem);
            if (item.Register(new PlayerCharacterItemRegisterContext(this, Hand.Left)))
                _leftItem = item;
        }
        else
        {
            Assert.IsNull(_rightItem);
            if (item.Register(new PlayerCharacterItemRegisterContext(this, Hand.Right)))
                _rightItem = item;
        }
    }

    // Attempt to unregister any items in `hand`, and sync it to all other clients.
    // If hand is already empty, doesn't send RPCs to the clients. (We assume all clients are already properly synced.)
    [Server]
    public void UnregisterItem(Hand hand)
    {
        // If we are not the server, ignore the call.
        if (!base.IsServerInitialized)
            return;

        if (hand == Hand.Left)
        {
            if (_leftItem != null)
            {
                var oldLeft = _leftItem;
                // Call with `item == null` to unregister `hand`.
                TryRegisterItemObserver(null, hand);
                oldLeft.RemoveOwnership();
            }
            return;
        }

        if (hand == Hand.Right)
        {
            if (_rightItem != null)
            {
                var oldRight = _rightItem;
                // Call with `item == null` to unregister `hand`.
                TryRegisterItemObserver(null, hand);
                oldRight.RemoveOwnership();
            }
            return;
        }
    }
}
