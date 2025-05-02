using System;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    UIDocument _uiDocument;

    [SerializeField]
    bool _startVisible = true;

    bool _isVisible = true;
    public bool IsVisible
    {
        get
        {
            return _isVisible;
        }
        set
        {
            if (value && !_isVisible)
            {
                InputManager.Singleton.InputMode = InputMode.UI;
                _uiDocument.rootVisualElement.RemoveFromClassList("display-none");
                _isVisible = value;
            }
            else if (!value && _isVisible)
            {
                InputManager.Singleton.InputMode = InputMode.Player;
                _uiDocument.rootVisualElement.AddToClassList("display-none");
                _isVisible = value;
            }
        }
    }

    TextField _addressInput;
    TextField _portInput;
    Button _hostButton;
    Button _joinButton;
    Button _soloButton;

    // Is the component subscribed to button clicked?
    bool _isSubscribedToButtons = false;

    // Is the component subscribed to `InputManager.Singleton.ShowUIAction`?
    bool _isSubscribedToShowUI = false;

    // Is the component listening for unhandled `Cancel` events?
    bool _isSubscribedToCancel = false;

    void Awake()
    {
        if (_uiDocument == null)
        {
            Debug.Log("`uIDocument` wasn't set.");
            throw new Exception();
        }
        // Ensure UI visibility matches `_isVisible`'s initial value.
        // This is the LAST PART where we directly touch `_isVisible` without using the property `IsVisible`.
        if (_isVisible)
        {
            InputManager.Singleton.InputMode = InputMode.UI;
            _uiDocument.rootVisualElement.RemoveFromClassList("display-none");
        }
        else
        {
            InputManager.Singleton.InputMode = InputMode.Player;
            _uiDocument.rootVisualElement.AddToClassList("display-none");
        }

        // Then make it follow the `_startVisible` value.
        IsVisible = _startVisible;

        _addressInput = _uiDocument.rootVisualElement.Q<TextField>("AddressInput");
        if (_addressInput == null)
        {
            Debug.Log("TextField with name `AddressInput` wasn't found.");
            throw new Exception();
        }

        _portInput = _uiDocument.rootVisualElement.Q<TextField>("PortInput");
        if (_portInput == null)
        {
            Debug.Log("TextField with name `PortInput` wasn't found.");
            throw new Exception();
        }

        _hostButton = _uiDocument.rootVisualElement.Q<Button>("HostButton");
        if (_hostButton == null)
        {
            Debug.Log("Button with name `HostButton` wasn't found.");
            throw new Exception();
        }

        _joinButton = _uiDocument.rootVisualElement.Q<Button>("JoinButton");
        if (_joinButton == null)
        {
            Debug.Log("Button with name `JoinButton` wasn't found.");
            throw new Exception();
        }

        _soloButton = _uiDocument.rootVisualElement.Q<Button>("SoloButton");
        if (_soloButton == null)
        {
            Debug.Log("Button with name `SoloButton` wasn't found.");
            throw new Exception();
        }
    }

    void OnEnable()
    {
        if (!_isSubscribedToButtons)
        {
            _hostButton.clicked += OnHostButtonClicked;
            _joinButton.clicked += OnJoinButtonClicked;
            _soloButton.clicked += OnSoloButtonClicked;
            _isSubscribedToButtons = true;
        }

        if (!_isSubscribedToShowUI)
        {
            InputManager.Singleton.ShowUIAction += OnShowUI;
            _isSubscribedToShowUI = true;
        }

        if (!_isSubscribedToCancel)
        {
            InputManager.Singleton.UICancelAction += OnCancel;
            _uiDocument.rootVisualElement.RegisterCallback<NavigationCancelEvent>(OnUnhandledCancel);
            _isSubscribedToCancel = true;
        }
    }

    void OnDisable()
    {
        if (_isSubscribedToButtons)
        {
            _hostButton.clicked -= OnHostButtonClicked;
            _joinButton.clicked -= OnJoinButtonClicked;
            _soloButton.clicked -= OnSoloButtonClicked;
            _isSubscribedToButtons = false;
        }

        if (_isSubscribedToShowUI)
        {
            InputManager.Singleton.ShowUIAction -= OnShowUI;
            _isSubscribedToShowUI = false;
        }

        if (_isSubscribedToCancel)
        {
            InputManager.Singleton.UICancelAction -= OnCancel;
            // `?` is there to suppress an exception on game end.
            _uiDocument.rootVisualElement?.UnregisterCallback<NavigationCancelEvent>(OnUnhandledCancel);
            _isSubscribedToCancel = false;
        }
    }

    void OnHostButtonClicked()
    {
        string address = _addressInput.value;
        ushort port = 0;
        if (!ushort.TryParse(_portInput.value, out port))
        {
            // Supplied port input is invalid.
            Debug.Log("Supplied port was invalid.");
            return;
        }

        // Start the instance as a server and a client.
        InstanceFinder.ServerManager.StartConnection(port);
        // TODO: What happens if the address is invalid?
        // TODO: Stop existing connections if any exist, and return early.
        InstanceFinder.ClientManager.StartConnection(address, port);

        // Scene loading is only possible after the server is started.
        // Bind a one-shot lambda to the event.
        Action<ServerConnectionStateArgs> loadSceneOnServerStart = null;
        loadSceneOnServerStart = (args) =>
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                // First remove the lambda from the event to ensure it is called only once.
                InstanceFinder.ServerManager.OnServerConnectionState -= loadSceneOnServerStart;
                // Scene load should be global.
                SceneLoadData sld = new SceneLoadData("Oregon");
                // Replace the currently loaded scene.
                sld.ReplaceScenes = ReplaceOption.All;
                InstanceFinder.SceneManager.LoadGlobalScenes(sld);
            }
        };
        InstanceFinder.ServerManager.OnServerConnectionState += loadSceneOnServerStart;
    }

    void OnJoinButtonClicked()
    {
        string address = _addressInput.value;
        ushort port = 0;
        if (!ushort.TryParse(_portInput.value, out port))
        {
            // Supplied port input is invalid.
            Debug.Log("Supplied port was invalid.");
            return;
        }

        // Start the instance as a client.
        // TODO: What happens if the address is invalid?
        InstanceFinder.ClientManager.StartConnection(address, port);
    }

    void OnSoloButtonClicked()
    {
        string address = _addressInput.value;
        ushort port = 0;
        if (!ushort.TryParse(_portInput.value, out port))
        {
            // Supplied port input is invalid.
            Debug.Log("Supplied port was invalid.");
            return;
        }

        // Start the instance as a server and a client.
        InstanceFinder.ServerManager.StartConnection(port);
        // TODO: Stop existing connections if any exist, and return early.
        // TODO: What happens if the address is invalid?
        InstanceFinder.ClientManager.StartConnection(address, port);

        // Scene loading is only possible after the server is started.
        // Bind a one-shot lambda to the event.
        Action<ServerConnectionStateArgs> loadSceneOnServerStart = null;
        loadSceneOnServerStart = (args) =>
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                // First remove the lambda from the event to ensure it is called only once.
                InstanceFinder.ServerManager.OnServerConnectionState -= loadSceneOnServerStart;
                // Scene load should be global.
                SceneLoadData sld = new SceneLoadData("Solo");
                // Replace the currently loaded scene.
                sld.ReplaceScenes = ReplaceOption.All;
                InstanceFinder.SceneManager.LoadGlobalScenes(sld);
            }
        };
        InstanceFinder.ServerManager.OnServerConnectionState += loadSceneOnServerStart;
    }

    void OnShowUI(InputAction.CallbackContext context)
    {
        // Return early if the action wasn't `performed`.
        if (!context.performed)
            return;
        // `IsVisible` checks if the assigned value is different from the old value, and operates only if so.
        // So assigning without checking old value won't cause any redundant calls.
        IsVisible = true;
    }

    void OnCancel(InputAction.CallbackContext context)
    {
        // Return early if the action wasn't `performed`.
        if (!context.performed)
            return;
        // If we have a focused element, the `OnUnhandledCancel()`
        // should be responsible for checking if any UI elements have consumed the event,
        // and hide the UI if so.
        if (_uiDocument.rootVisualElement.focusController.focusedElement != null)
            return;
        // Else, we should exit the UI.
        IsVisible = false;
    }

    void OnUnhandledCancel(NavigationCancelEvent evt)
    {
        IsVisible = false;
        evt.StopPropagation();
    }
}
