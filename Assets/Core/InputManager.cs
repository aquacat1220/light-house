using System;
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
// Since other components might need the `InputManager.Singleton` in as early as `Awake()`,
// this script is set to have an execution order of -1 (smaller the earlier).
public class InputManager : MonoBehaviour
{
    public static InputManager Singleton { get; set; }
    public InputActions InputActions { get; set; }

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
                InputActions.Disable();
                Cursor.lockState = CursorLockMode.None;
                _inputMode = value;
            }
            else if (value == InputMode.Player)
            {
                if (_inputMode == InputMode.Player)
                    return;
                InputActions.Player.Enable();
                Cursor.lockState = CursorLockMode.Locked;
                _inputMode = value;
            }
            else if (value == InputMode.UI)
            {
                if (_inputMode == InputMode.UI)
                    return;
                InputActions.UI.Enable();
                Cursor.lockState = CursorLockMode.None;
                _inputMode = value;
            }
        }
    }

    void Awake()
    {
        InputActions = new InputActions();

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
        // Debug.Log($"Player: {InputActions.Player.enabled}, UI: {InputActions.UI.enabled}");
    }
}
