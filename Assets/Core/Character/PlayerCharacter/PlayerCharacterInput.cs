using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerCharacterInput : NetworkBehaviour
{
    // Triggered when the move input changes. Argument holds the new input value.
    public UnityEvent<Vector2> Move;
    // Triggered when the look input changes. Argument holds the new input value.
    public UnityEvent<Vector2> Look;
    // Triggered when the die action is performed.
    public UnityEvent Die;
    // Triggered when the primary action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> Primary;
    // Triggered when the secondary action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> Secondary;
    // Triggered when the select item 1 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> SelectItem1;
    // Triggered when the drop item 1 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> DropItem1;
    // Triggered when the select item 2 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> SelectItem2;
    // Triggered when the drop item 2 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> DropItem2;
    // Triggered when the select item 3 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> SelectItem3;
    // Triggered when the drop item 3 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> DropItem3;
    // Triggered when the select item 4 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> SelectItem4;
    // Triggered when the drop item 4 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
    public UnityEvent<bool> DropItem4;

    bool _isSubscribedToInputManager = false;

    public override void OnStartClient()
    {
        if (base.IsOwner)
        {
            // We are the owning client of this character. Inputs should be passed down.
            SubscribeToInputManager();
        }
    }

    public override void OnStopClient()
    {
        // We don't check for ownership here, since calling `UnsubscribeFromInputManager()` when we are not subscribed shouldn't cause any problems.
        UnsubscribeFromInputManager();
    }

    void OnEnable()
    {
        if (base.IsClientInitialized && base.IsOwner)
        {
            // We are the owning client of this character. Inputs should be passed down.
            SubscribeToInputManager();
        }
    }

    void OnDisable()
    {
        // We don't check for ownership here, since calling `UnsubscribeFromInputManager()` when we are not subscribed shouldn't cause any problems.
        UnsubscribeFromInputManager();
    }

    void SubscribeToInputManager()
    {
        if (!_isSubscribedToInputManager)
        {
            InputManager.Singleton.InputActions.Player.Move.performed += OnMove;
            InputManager.Singleton.InputActions.Player.Move.canceled += OnMove;
            InputManager.Singleton.InputActions.Player.Look.performed += OnLook;
            InputManager.Singleton.InputActions.Player.Look.canceled += OnLook;
            InputManager.Singleton.InputActions.Player.Die.performed += OnDie;
            InputManager.Singleton.InputActions.Player.Primary.performed += OnPrimary;
            InputManager.Singleton.InputActions.Player.Primary.canceled += OnPrimary;
            InputManager.Singleton.InputActions.Player.Secondary.performed += OnSecondary;
            InputManager.Singleton.InputActions.Player.Secondary.canceled += OnSecondary;
            InputManager.Singleton.InputActions.Player.SelectItem1.performed += OnSelectItem1;
            InputManager.Singleton.InputActions.Player.SelectItem1.canceled += OnSelectItem1;
            InputManager.Singleton.InputActions.Player.DropItem1.performed += OnDropItem1;
            InputManager.Singleton.InputActions.Player.DropItem1.canceled += OnDropItem1;
            InputManager.Singleton.InputActions.Player.SelectItem2.performed += OnSelectItem2;
            InputManager.Singleton.InputActions.Player.SelectItem2.canceled += OnSelectItem2;
            InputManager.Singleton.InputActions.Player.DropItem2.performed += OnDropItem2;
            InputManager.Singleton.InputActions.Player.DropItem2.canceled += OnDropItem2;
            InputManager.Singleton.InputActions.Player.SelectItem3.performed += OnSelectItem3;
            InputManager.Singleton.InputActions.Player.SelectItem3.canceled += OnSelectItem3;
            InputManager.Singleton.InputActions.Player.DropItem3.performed += OnDropItem3;
            InputManager.Singleton.InputActions.Player.DropItem3.canceled += OnDropItem3;
            InputManager.Singleton.InputActions.Player.SelectItem4.performed += OnSelectItem4;
            InputManager.Singleton.InputActions.Player.SelectItem4.canceled += OnSelectItem4;
            InputManager.Singleton.InputActions.Player.DropItem4.performed += OnDropItem4;
            InputManager.Singleton.InputActions.Player.DropItem4.canceled += OnDropItem4;
            _isSubscribedToInputManager = true;
        }
    }

    void UnsubscribeFromInputManager()
    {
        if (_isSubscribedToInputManager)
        {
            InputManager.Singleton.InputActions.Player.Move.performed -= OnMove;
            InputManager.Singleton.InputActions.Player.Move.canceled -= OnMove;
            InputManager.Singleton.InputActions.Player.Look.performed -= OnLook;
            InputManager.Singleton.InputActions.Player.Look.canceled -= OnLook;
            InputManager.Singleton.InputActions.Player.Die.performed -= OnDie;
            InputManager.Singleton.InputActions.Player.Primary.performed -= OnPrimary;
            InputManager.Singleton.InputActions.Player.Primary.canceled -= OnPrimary;
            InputManager.Singleton.InputActions.Player.Secondary.performed -= OnSecondary;
            InputManager.Singleton.InputActions.Player.Secondary.canceled -= OnSecondary;
            InputManager.Singleton.InputActions.Player.SelectItem1.performed -= OnSelectItem1;
            InputManager.Singleton.InputActions.Player.SelectItem1.canceled -= OnSelectItem1;
            InputManager.Singleton.InputActions.Player.DropItem1.performed -= OnDropItem1;
            InputManager.Singleton.InputActions.Player.DropItem1.canceled -= OnDropItem1;
            InputManager.Singleton.InputActions.Player.SelectItem2.performed -= OnSelectItem2;
            InputManager.Singleton.InputActions.Player.SelectItem2.canceled -= OnSelectItem2;
            InputManager.Singleton.InputActions.Player.DropItem2.performed -= OnDropItem2;
            InputManager.Singleton.InputActions.Player.DropItem2.canceled -= OnDropItem2;
            InputManager.Singleton.InputActions.Player.SelectItem3.performed -= OnSelectItem3;
            InputManager.Singleton.InputActions.Player.SelectItem3.canceled -= OnSelectItem3;
            InputManager.Singleton.InputActions.Player.DropItem3.performed -= OnDropItem3;
            InputManager.Singleton.InputActions.Player.DropItem3.canceled -= OnDropItem3;
            InputManager.Singleton.InputActions.Player.SelectItem4.performed -= OnSelectItem4;
            InputManager.Singleton.InputActions.Player.SelectItem4.canceled -= OnSelectItem4;
            InputManager.Singleton.InputActions.Player.DropItem4.performed -= OnDropItem4;
            InputManager.Singleton.InputActions.Player.DropItem4.canceled -= OnDropItem4;
            _isSubscribedToInputManager = false;
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        Move?.Invoke(context.ReadValue<Vector2>());
    }

    void OnLook(InputAction.CallbackContext context)
    {
        Look?.Invoke(context.ReadValue<Vector2>());
    }

    void OnDie(InputAction.CallbackContext context)
    {
        if (context.performed)
            Die?.Invoke();
    }

    void OnPrimary(InputAction.CallbackContext context)
    {
        if (context.performed)
            Primary?.Invoke(true);
        else if (context.canceled)
            Primary?.Invoke(false);
    }

    void OnSecondary(InputAction.CallbackContext context)
    {
        if (context.performed)
            Secondary?.Invoke(true);
        else if (context.canceled)
            Secondary?.Invoke(false);
    }

    void OnSelectItem1(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectItem1?.Invoke(true);
        else if (context.canceled)
            SelectItem1?.Invoke(false);
    }

    void OnDropItem1(InputAction.CallbackContext context)
    {
        if (context.performed)
            DropItem1?.Invoke(true);
        else if (context.canceled)
            DropItem1?.Invoke(false);
    }

    void OnSelectItem2(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectItem2?.Invoke(true);
        else if (context.canceled)
            SelectItem2?.Invoke(false);
    }

    void OnDropItem2(InputAction.CallbackContext context)
    {
        if (context.performed)
            DropItem2?.Invoke(true);
        else if (context.canceled)
            DropItem2?.Invoke(false);
    }

    void OnSelectItem3(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectItem3?.Invoke(true);
        else if (context.canceled)
            SelectItem3?.Invoke(false);
    }

    void OnDropItem3(InputAction.CallbackContext context)
    {
        if (context.performed)
            DropItem3?.Invoke(true);
        else if (context.canceled)
            DropItem3?.Invoke(false);
    }

    void OnSelectItem4(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectItem4?.Invoke(true);
        else if (context.canceled)
            SelectItem4?.Invoke(false);
    }

    void OnDropItem4(InputAction.CallbackContext context)
    {
        if (context.performed)
            DropItem4?.Invoke(true);
        else if (context.canceled)
            DropItem4?.Invoke(false);
    }
}
