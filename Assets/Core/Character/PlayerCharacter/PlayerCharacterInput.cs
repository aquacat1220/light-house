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
        Die?.Invoke();
    }

    void OnPrimary(InputAction.CallbackContext context)
    {
        Primary?.Invoke(context.performed);
    }

    void OnSecondary(InputAction.CallbackContext context)
    {
        Secondary?.Invoke(context.performed);
    }
}
