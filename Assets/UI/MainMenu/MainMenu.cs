using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    UIDocument uIDocument;

    [SerializeField]
    bool startVisible = false;

    TextField addressInput;
    TextField portInput;
    Button hostButton;
    Button joinButton;
    Button soloButton;

    // Is the component subscribed to button clicked?
    bool isSubscribedToButtons = false;

    // Reference to InputAction for UI toggle.
    InputAction cancelAction;

    // Is the component subscribed to the action?
    bool isSubscribedToCancel = false;

    // Is the menu currently visible? Synced with the actual USS selector.
    bool isMenuVisible = true;

    void Awake()
    {
        if (uIDocument == null)
        {
            Debug.Log("\"uIDocument\" wasn't set.");
            throw new Exception();
        }

        if (startVisible)
        {
            ShowMainMenu();
        }
        else
        {
            HideMainMenu();
        }

        addressInput = uIDocument.rootVisualElement.Q<TextField>("AddressInput");
        if (addressInput == null)
        {
            Debug.Log("TextField with name \"AddressInput\" wasn't found.");
            throw new Exception();
        }

        portInput = uIDocument.rootVisualElement.Q<TextField>("PortInput");
        if (portInput == null)
        {
            Debug.Log("TextField with name \"PortInput\" wasn't found.");
            throw new Exception();
        }

        hostButton = uIDocument.rootVisualElement.Q<Button>("HostButton");
        if (hostButton == null)
        {
            Debug.Log("Button with name \"HostButton\" wasn't found.");
            throw new Exception();
        }

        joinButton = uIDocument.rootVisualElement.Q<Button>("JoinButton");
        if (joinButton == null)
        {
            Debug.Log("Button with name \"JoinButton\" wasn't found.");
            throw new Exception();
        }

        soloButton = uIDocument.rootVisualElement.Q<Button>("SoloButton");
        if (soloButton == null)
        {
            Debug.Log("Button with name \"SoloButton\" wasn't found.");
            throw new Exception();
        }

        cancelAction = InputSystem.actions.FindAction("Cancel");
        if (cancelAction == null)
        {
            Debug.Log("\"Cancel\" action wasn't found.");
            throw new Exception();
        }
    }

    void OnEnable()
    {
        if (!isSubscribedToButtons)
        {
            hostButton.clicked += OnHostButtonClicked;
            joinButton.clicked += OnJoinButtonClicked;
            soloButton.clicked += OnSoloButtonClicked;
            isSubscribedToButtons = true;
        }

        if (!isSubscribedToCancel)
        {
            cancelAction.performed += OnCancel;
            isSubscribedToCancel = true;
        }
    }

    void OnDisable()
    {
        if (isSubscribedToButtons)
        {
            hostButton.clicked -= OnHostButtonClicked;
            joinButton.clicked -= OnJoinButtonClicked;
            soloButton.clicked -= OnSoloButtonClicked;
            isSubscribedToButtons = false;
        }

        if (isSubscribedToCancel)
        {
            cancelAction.performed -= OnCancel;
            isSubscribedToCancel = false;
        }
    }

    void OnHostButtonClicked()
    {
        string address = addressInput.value;
        ushort port = 0;
        if (!ushort.TryParse(portInput.value, out port))
        {
            // Supplied port input is invalid.
            Debug.Log("Supplied port was invalid.");
            return;
        }

        // Start the instance as a server and a client.
        InstanceFinder.ServerManager.StartConnection(port);
        // TODO: What happens if the address is invalid?
        // InstanceFinder.ClientManager.StartConnection("0.0.0.0", port);

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
        string address = addressInput.value;
        ushort port = 0;
        if (!ushort.TryParse(portInput.value, out port))
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
        string address = addressInput.value;
        ushort port = 0;
        if (!ushort.TryParse(portInput.value, out port))
        {
            // Supplied port input is invalid.
            Debug.Log("Supplied port was invalid.");
            return;
        }

        // Start the instance as a server and a client.
        InstanceFinder.ServerManager.StartConnection(port);
        // TODO: What happens if the address is invalid?
        InstanceFinder.ClientManager.StartConnection("0.0.0.0", port);

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
        if (isMenuVisible)
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
        if (isMenuVisible)
        {
            uIDocument.rootVisualElement.AddToClassList("display-none");
            isMenuVisible = false;
        }
    }

    void ShowMainMenu()
    {
        if (!isMenuVisible)
        {
            uIDocument.rootVisualElement.RemoveFromClassList("display-none");
            isMenuVisible = true;
        }
    }
}
