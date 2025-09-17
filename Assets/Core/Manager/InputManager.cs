using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputMode
{
    None,
    Player,
    UI
}

// This component holds a reference to the active input action asset, and manages which action map is active.
// Since other components might need the `InputManager.Singleton` in as early as `Awake()`,
// this script is set to have an execution order of -1 (smaller the earlier).
public class InputManager : MonoBehaviour
{
    public static InputManager Singleton { get; private set; }
    public InputActions InputActions { get; private set; }

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
                // Disable the entire input action asset to ensure `Player` is the only active map.
                InputActions.Disable();
                InputActions.Player.Enable();
                Cursor.lockState = CursorLockMode.Locked;
                _inputMode = value;
            }
            else if (value == InputMode.UI)
            {
                if (_inputMode == InputMode.UI)
                    return;
                // Disable the entire input action asset to ensure `Player` is the only active map.
                InputActions.Disable();
                InputActions.UI.Enable();
                Cursor.lockState = CursorLockMode.None;
                _inputMode = value;
            }
        }
    }

    void Awake()
    {
        InputActions = new InputActions();
        InputActions.Disable();

        if (Singleton != null)
        {
            Debug.Log("`Singleton` was non-null, implying there are multiple instances of `InputManager`s in this scene.");
            throw new Exception();
        }
        Singleton = this;

        // Then assign `_initialInputMode` to `InputMode`.
        InputMode = _initialInputMode;
    }
}
