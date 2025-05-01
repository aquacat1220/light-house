using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputMode
{
    None,
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
    public event Action<InputAction.CallbackContext> ShowUIAction;
    public event Action<InputAction.CallbackContext> DieAction;

    public event Action<InputAction.CallbackContext> UICancelAction;


    // Reference to the input action asset to use.
    [SerializeField]
    InputActionAsset _inputActionAsset;

    // Input action maps retrieved from `_inputActionAsset`.
    InputActionMap _playerMap;
    InputActionMap _uiMap;

    bool _isSubscribedToMaps = false;

    [SerializeField]
    InputMode _initialInputMode = InputMode.None;

    InputMode _inputMode = InputMode.None;
    public InputMode InputMode
    {
        get { return _inputMode; }
        set
        {
            if (value == InputMode.None)
            {
                if (_inputMode == InputMode.None)
                    return;
                _inputActionAsset.Disable();
                _inputMode = value;
            }
            else if (value == InputMode.Player)
            {
                if (_inputMode == InputMode.Player)
                    return;
                _inputActionAsset.Disable();
                _playerMap.Enable();
                _inputMode = value;
            }
            else if (value == InputMode.UI)
            {
                if (_inputMode == InputMode.UI)
                    return;
                _inputActionAsset.Disable();
                _uiMap.Enable();
                _inputMode = value;
            }
            else
            {
                Debug.Log("Unknown input action map was encountered in `InputManager`.");
                throw new Exception();
            }
        }
    }

    void Awake()
    {
        if (_inputActionAsset == null)
        {
            Debug.Log("`_inputActionAsset` wasn't set.");
            throw new Exception();
        }
        _playerMap = _inputActionAsset.FindActionMap("Player", true);
        _uiMap = _inputActionAsset.FindActionMap("UI", true);
        if (_inputActionAsset.actionMaps.Count != 2)
        {
            Debug.Log("`_inputActionAsset` contains action maps other than \"Player\" and \"UI\"");
            throw new Exception();
        }
        // Disable asset, since `_inputMode` is initially `InputMode.None`.
        _inputActionAsset.Disable();

        if (Singleton != null)
        {
            Debug.Log("`Singleton` was non-null, implying there are multiple instances of `InputManager`s in this scene.");
            throw new Exception();
        }
        Singleton = this;

        // Then assign `_initialInputMode` to `InputMode`.
        InputMode = _initialInputMode;
    }

    void Update()
    {
        Debug.Log($"{_playerMap.enabled}, {_uiMap.enabled}");
        // _inputActionAsset.Disable();
    }

    void OnEnable()
    {
        if (!_isSubscribedToMaps)
        {
            _playerMap.actionTriggered += OnActionTriggered;
            _uiMap.actionTriggered += OnActionTriggered;
            _isSubscribedToMaps = true;
        }
    }

    void OnDisable()
    {
        if (_isSubscribedToMaps)
        {
            _playerMap.actionTriggered -= OnActionTriggered;
            _playerMap.actionTriggered -= OnActionTriggered;
            _isSubscribedToMaps = false;
        }
    }

    void OnActionTriggered(InputAction.CallbackContext context)
    {
        var actionMapName = context.action.actionMap.name;
        var actionName = context.action.name;
        if (actionMapName == "Player")
        {
            if (actionName == "Move")
                MoveAction?.Invoke(context);
            else if (actionName == "Look")
                LookAction?.Invoke(context);
            else if (actionName == "Primary")
                PrimaryAction?.Invoke(context);
            else if (actionName == "Secondary")
                SecondaryAction?.Invoke(context);
            else if (actionName == "ShowUI")
                ShowUIAction?.Invoke(context);
            else if (actionName == "Die")
                DieAction?.Invoke(context);
            else
            {
                Debug.Log("Unknown input action encountered in `InputManager`.");
                throw new Exception();
            }
        }
        else if (actionMapName == "UI")
        {
            if (actionName == "Cancel")
                UICancelAction?.Invoke(context);
        }
        else
        {
            Debug.Log("Unknown input action map encountered in `InputManager`.");
            throw new Exception();
        }

    }
}
