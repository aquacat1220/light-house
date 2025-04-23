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
    [SerializeField]
    Item _leftItem = null;
    [SerializeField]
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
        // `RegisterInit()` should be called only after `Item`s have been fully initialized.
        // With initialization ordering, we can guarantee that they are initializing first.
        // So calling `RegisterInit()` only after we have initialized is enough.
        if (!base.IsClientStarted || base.IsClientInitialized)
        {
            RegisterInit();
        }
    }

    public override void OnStartClient()
    {
        // `RegisterInit()` should be called only after `Item`s have been fully initialized.
        // With initialization ordering, we can guarantee that they are initializing first.
        // So calling `RegisterInit()` only after we have initialized is enough.
        if (!base.IsServerStarted || base.IsServerInitialized)
        {
            RegisterInit();
        }

        if (base.IsOwner)
        {
            // We are the owner of this character. Subscribe events to the input actions.
            SubscribeToAction();
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

    // Makes sure initial values of `_leftItem` and `_rightItem` are registered.
    // `Item.Register()` requires all network-related stuff to be initialized.
    // Calls to this function should check carefully.
    void RegisterInit()
    {
        if (_leftItem != null)
        {
            if (!_leftItem.Register(new PlayerCharacterItemRegisterContext(this, Hand.Left)))
            {
                _leftItem = null;
            }
        }
        if (_rightItem != null)
        {
            if (!_rightItem.Register(new PlayerCharacterItemRegisterContext(this, Hand.Right)))
            {
                _rightItem = null;
            }
        }
    }

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
            if (_leftItem != null)
            {
                UnregisterItem(hand);
            }
            Assert.IsNull(_leftItem);
            TryRegisterItemObserver(item, hand);
        }
        else
        {
            if (_rightItem != null)
            {
                UnregisterItem(hand);
            }
            Assert.IsNull(_rightItem);
            TryRegisterItemObserver(item, hand);
        }
    }

    public void UnregisterItem(Hand hand)
    {
        // If we are not the server, ignore the call.
        if (!base.IsServerInitialized)
            return;

        if (hand == Hand.Left && _leftItem == null)
            return;
        if (hand == Hand.Right && _rightItem == null)
            return;

        UnregisterItemObserver(hand);
    }

    [ObserversRpc(RunLocally = true)]
    void TryRegisterItemObserver(Item item, Hand hand)
    {
        Assert.IsNotNull(item);
        if (hand == Hand.Left)
        {
            Assert.IsNull(_leftItem);
            if (item.Register(new PlayerCharacterItemRegisterContext(this, Hand.Left)))
            {
                _leftItem = item;
            }
        }
        else
        {
            Assert.IsNull(_rightItem);
            if (item.Register(new PlayerCharacterItemRegisterContext(this, Hand.Right)))
            {
                _rightItem = item;
            }
        }
    }


    [ObserversRpc(RunLocally = true)]
    void UnregisterItemObserver(Hand hand)
    {
        if (hand == Hand.Left)
        {
            Assert.IsNotNull(_leftItem);
            _leftItem.Unregister();
            _leftItem = null;
        }
        else
        {
            Assert.IsNotNull(_rightItem);
            _rightItem.Unregister();
            _rightItem = null;
        }
    }
}
