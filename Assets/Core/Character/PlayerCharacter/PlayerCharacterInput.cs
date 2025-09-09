using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerCharacterInput : NetworkBehaviour
{
    [SerializeField]
    UnityEvent<Vector2> move;
    [SerializeField]
    UnityEvent<Vector2> look;
    [SerializeField]
    UnityEvent die;
    [SerializeField]
    UnityEvent<bool> primary;
    [SerializeField]
    UnityEvent<bool> secondary;

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
        if (base.IsOwner)
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
        move?.Invoke(context.ReadValue<Vector2>());
    }

    void OnLook(InputAction.CallbackContext context)
    {
        look?.Invoke(context.ReadValue<Vector2>());
    }

    void OnDie(InputAction.CallbackContext context)
    {
        die?.Invoke();
    }

    void OnPrimary(InputAction.CallbackContext context)
    {
        primary?.Invoke(context.performed);
    }

    void OnSecondary(InputAction.CallbackContext context)
    {
        secondary?.Invoke(context.performed);
    }
}
