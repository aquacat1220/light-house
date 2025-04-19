using System;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
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
    public Hand hand;

    public PlayerCharacterItemRegisterContext(PlayerCharacterItemSystem itemSystem)
    {
        ItemSystem = itemSystem;
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

    public override void OnStartClient()
    {
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

    // Depending on the `item`'s behavior, registering can fail.
    public bool RegisterItem(Item item, Hand hand)
    {
        if (hand == Hand.Left)
        {
            if (_leftItem != null)
            {
                UnregisterItem(hand);
            }
            if (!item.Register(new PlayerCharacterItemRegisterContext(this)))
                return false;
            _leftItem = item;
            return true;
        }
        else
        {
            if (_rightItem != null)
            {
                UnregisterItem(hand);
            }
            if (!item.Register(new PlayerCharacterItemRegisterContext(this)))
                return false;
            _rightItem = item;
            return true;
        }
    }

    // Unlike `RegisterItem()`, unregistering cannot, and should not fail.
    public void UnregisterItem(Hand hand)
    {
        if (hand == Hand.Left)
        {
            if (_leftItem != null)
            {
                UnregisterItem(hand);
            }
        }
        else
        {
            if (_rightItem != null)
            {
                UnregisterItem(hand);
            }
        }
    }
}
