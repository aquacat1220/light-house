using System;
using FishNet.Object;
using Unity.VisualScripting.Antlr3.Runtime;
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
        // And unregister all items.
        UnregisterItem(Hand.Left);
        UnregisterItem(Hand.Right);
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
            _itemPrimaryActionRef.action.performed += OnItemPrimary;
            _itemSecondaryActionRef.action.performed += OnItemSecondary;
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
    // If hand is already occupied, unregisteres the registered item.
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
    // `TryRregisterItem()` does hand-empty-checking, but it's still OK to call it with non-empty hands.
    [ObserversRpc(RunLocally = true, BufferLast = true)]
    void TryRegisterItemObserver(Item item, Hand hand)
    {
        TryRegisterItemLocal(item, hand);
    }

    // Attempts to register `item` in `hand` locally.
    // This function doesn't sync anything: without consideration, `this` will remain desynced until the next valid call.
    // `TryRregisterItem()` does hand-empty-checking, but it's still OK to call it with non-empty hands.
    void TryRegisterItemLocal(Item item, Hand hand)
    {
        if (item == null)
        {
            Debug.Log("`TryRegisterItemLocal()` was called with a `item == null`, which is really strange.");
            return;
        }
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
                UnregisterItemObserver(hand);
                oldLeft.RemoveOwnership();
            }
            return;
        }

        if (hand == Hand.Right)
        {
            if (_rightItem != null)
            {
                var oldRight = _rightItem;
                UnregisterItemObserver(hand);
                oldRight.RemoveOwnership();
            }
            return;
        }
    }

    // Attempt to unregister any items in `hand` on observers.
    // `UnregisterItem()` does hand-not-empty-checking, but it's still OK to call it with empty hands.
    // Since `BufferLast = true`, on newly joining observers, this might be called without a registered item,
    // when the previous `TryRegisterItemObserver()` call was overwritten by a more recent one.
    // Thus, it is *essential* that this function is OK to call with empty hands.
    [ObserversRpc(RunLocally = true, BufferLast = true)]
    void UnregisterItemObserver(Hand hand)
    {
        UnregisterItemLocal(hand);
    }

    // Attempt to unregister any items in `hand` locally.
    // This function doesn't sync anything: without consideration, `this` will remain desynced until the next valid call.
    // `UnregisterItem()` does hand-not-empty-checking, but it's still OK to call it with empty hands.
    void UnregisterItemLocal(Hand hand)
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
    }
}
