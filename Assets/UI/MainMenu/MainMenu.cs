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
    UIDocument _uIDocument;

    [SerializeField]
    bool _startVisible = false;

    TextField _addressInput;
    TextField _portInput;
    Button _hostButton;
    Button _joinButton;
    Button _soloButton;

    // Is the component subscribed to button clicked?
    bool _isSubscribedToButtons = false;

    // Reference to InputAction for UI toggle.
    InputAction _cancelAction;

    // Is the component subscribed to the action?
    bool _isSubscribedToCancel = false;

    // Is the menu currently visible? Synced with the actual USS selector.
    bool _isMenuVisible = true;

    void Awake()
    {
        if (_uIDocument == null)
        {
            Debug.Log("`uIDocument` wasn't set.");
            throw new Exception();
        }

        if (_startVisible)
        {
            ShowMainMenu();
        }
        else
        {
            HideMainMenu();
        }

        _addressInput = _uIDocument.rootVisualElement.Q<TextField>("AddressInput");
        if (_addressInput == null)
        {
            Debug.Log("TextField with name `AddressInput` wasn't found.");
            throw new Exception();
        }

        _portInput = _uIDocument.rootVisualElement.Q<TextField>("PortInput");
        if (_portInput == null)
        {
            Debug.Log("TextField with name `PortInput` wasn't found.");
            throw new Exception();
        }

        _hostButton = _uIDocument.rootVisualElement.Q<Button>("HostButton");
        if (_hostButton == null)
        {
            Debug.Log("Button with name `HostButton` wasn't found.");
            throw new Exception();
        }

        _joinButton = _uIDocument.rootVisualElement.Q<Button>("JoinButton");
        if (_joinButton == null)
        {
            Debug.Log("Button with name `JoinButton` wasn't found.");
            throw new Exception();
        }

        _soloButton = _uIDocument.rootVisualElement.Q<Button>("SoloButton");
        if (_soloButton == null)
        {
            Debug.Log("Button with name `SoloButton` wasn't found.");
            throw new Exception();
        }

        _cancelAction = InputSystem.actions.FindAction("Cancel");
        if (_cancelAction == null)
        {
            Debug.Log("`Cancel` action wasn't found.");
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

        if (!_isSubscribedToCancel)
        {
            _cancelAction.performed += OnCancel;
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

        if (_isSubscribedToCancel)
        {
            _cancelAction.performed -= OnCancel;
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

    void OnCancel(InputAction.CallbackContext context)
    {
        if (_isMenuVisible)
        {
            HideMainMenu();
        }
        else
        {
            ShowMainMenu();
        }
    }

    void HideMainMenu()
    {
        if (_isMenuVisible)
        {
            _uIDocument.rootVisualElement.AddToClassList("display-none");
            _isMenuVisible = false;
        }
    }

    void ShowMainMenu()
    {
        if (!_isMenuVisible)
        {
            _uIDocument.rootVisualElement.RemoveFromClassList("display-none");
            _isMenuVisible = true;
        }
    }
}
