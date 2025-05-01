using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputMode
{
    Player,
    UI
}

// This component listens to `PlayerInput.onActionTriggered`,
// and dispatches different events based on which input action was triggered.
public class InputManager : MonoBehaviour
{
    public static InputManager Singleton { get; private set; }
    public event Action<InputAction.CallbackContext> MoveAction;
    public event Action<InputAction.CallbackContext> LookAction;
    public event Action<InputAction.CallbackContext> PrimaryAction;
    public event Action<InputAction.CallbackContext> SecondaryAction;
    public event Action<InputAction.CallbackContext> ToggleUIAction;
    public event Action<InputAction.CallbackContext> DieAction;


    InputMode _inputMode;
    public InputMode InputMode
    {
        get { return _inputMode; }
        set
        {
            if (value == InputMode.Player)
            {
                if (_inputMode == InputMode.Player)
                    return;
                _playerInput.SwitchCurrentActionMap("Player");
                _inputMode = value;
            }
            else if (value == InputMode.UI)
            {
                if (_inputMode == InputMode.UI)
                    return;
                _playerInput.SwitchCurrentActionMap("UI");
                _inputMode = value;
            }
            else
            {
                Debug.Log("Unknown input action map was encountered in `InputManager`.");
                throw new Exception();
            }
        }
    }

    [SerializeField]
    PlayerInput _playerInput;

    bool _isSubscribedToPlayerInput = false;

    void Awake()
    {
        if (_playerInput == null)
        {
            Debug.Log("`_playerInput` wasn't set.");
            throw new Exception();
        }

        if (_playerInput.currentActionMap.name == "Player")
            InputMode = InputMode.Player;
        else if (_playerInput.currentActionMap.name == "UI")
            InputMode = InputMode.UI;
        else
        {
            Debug.Log("Unknown input action map was encountered in `InputManager`.");
            throw new Exception();
        }

        if (Singleton != null)
        {
            Debug.Log("`Singleton` was non-null, implying there are multiple instances of `InputManager`s in this scene.");
            throw new Exception();
        }
        Singleton = this;
    }

    void OnEnable()
    {
        if (!_isSubscribedToPlayerInput)
        {
            _playerInput.onActionTriggered += OnActionTriggered;
            _isSubscribedToPlayerInput = true;
        }
    }

    void OnDisable()
    {
        if (_isSubscribedToPlayerInput)
        {
            _playerInput.onActionTriggered -= OnActionTriggered;
            _isSubscribedToPlayerInput = false;
        }
    }

    void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (context.action.actionMap.name == "Player")
        {
            var actionName = context.action.name;
            if (actionName == "Move")
                MoveAction?.Invoke(context);
            else if (actionName == "Look")
                LookAction?.Invoke(context);
            else if (actionName == "Primary")
                PrimaryAction?.Invoke(context);
            else if (actionName == "Secondary")
                SecondaryAction?.Invoke(context);
            else if (actionName == "ToggleUI")
                ToggleUIAction?.Invoke(context);
            else if (actionName == "Die")
                DieAction?.Invoke(context);
            else
            {
                Debug.Log("Unknown input action encountered in `InputManager`.");
                throw new Exception();
            }
        }
        // UI input actions don't need this, since the UI toolkit connects to the input actions behind the scenes.
    }
}
