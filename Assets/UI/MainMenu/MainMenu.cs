using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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

    NetworkManager networkManager;
    UnityTransport unityTransport;

    // Is the component subscribed to button clicked?
    bool isSubscribedToClicked = false;

    // Reference to InputAction for UI toggle.
    InputAction cancelAction;

    // Is the component subscribed to the action?
    bool isSubscribedToAction = false;

    // Is the menu currently visible? Synced with the actual USS selector.
    bool isMenuVisible = true;


    // Reference to a coroutine created by `StartCoroutine(StartSessionCoroutine())`.
    // `null` if no coroutine is running.
    Coroutine runningCoroutine;

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

    void Start()
    {
        if (networkManager == null)
        {
            networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.Log("NetworkManager singleton was null.");
                throw new Exception();
            }
            unityTransport = networkManager.GetComponent<UnityTransport>();
            if (unityTransport == null)
            {
                Debug.Log("NetworkManager didn't have a UnityTransport component.");
                throw new Exception();
            }
        }
    }

    void OnEnable()
    {
        if (!isSubscribedToClicked)
        {
            hostButton.clicked += OnHostButtonClicked;
            joinButton.clicked += OnJoinButtonClicked;
            soloButton.clicked += OnSoloButtonClicked;
            isSubscribedToClicked = true;
        }

        if (!isSubscribedToAction)
        {
            cancelAction.performed += OnCancel;
            isSubscribedToAction = true;
        }
    }

    void OnDisable()
    {
        if (isSubscribedToClicked)
        {
            hostButton.clicked -= OnHostButtonClicked;
            joinButton.clicked -= OnJoinButtonClicked;
            soloButton.clicked -= OnSoloButtonClicked;
            isSubscribedToClicked = false;
        }

        if (isSubscribedToAction)
        {
            cancelAction.performed -= OnCancel;
            isSubscribedToAction = false;
        }
    }

    void OnHostButtonClicked()
    {
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }
        runningCoroutine = StartCoroutine(StartSessionCoroutine(StartSessionMode.Host));
    }

    void OnJoinButtonClicked()
    {
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }
        runningCoroutine = StartCoroutine(StartSessionCoroutine(StartSessionMode.Join));
    }

    void OnSoloButtonClicked()
    {
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }
        runningCoroutine = StartCoroutine(StartSessionCoroutine(StartSessionMode.Solo));
    }

    enum StartSessionMode
    {
        Host,
        Join,
        Solo
    }

    IEnumerator StartSessionCoroutine(StartSessionMode sessionMode)
    {
        if (networkManager.IsListening && !networkManager.ShutdownInProgress)
        {
            // We are connected to a session, and shutdown isn't in progress.
            // Shutdown the current session to join a new one.
            networkManager.Shutdown();
        }
        while (networkManager.IsListening)
        {
            // Shutdown might take multiple frames. Wait for it to end.
            yield return null;
        }

        SetConnectionData();

        switch (sessionMode)
        {
            case StartSessionMode.Host:
                if (networkManager.StartHost())
                {
                    networkManager.SceneManager.LoadScene("Oregon", LoadSceneMode.Single);
                    HideMainMenu();
                };
                break;
            case StartSessionMode.Join:
                if (networkManager.StartClient())
                {
                    HideMainMenu();
                };
                break;
            case StartSessionMode.Solo:
                if (networkManager.StartHost())
                {
                    networkManager.SceneManager.LoadScene("Solo", LoadSceneMode.Single);
                    HideMainMenu();
                };
                break;
        }
    }

    void SetConnectionData()
    {
        string addressValue = addressInput.value;
        ushort portValue = 0;
        if (!ushort.TryParse(portInput.value, out portValue))
        {
            // Supplied port input is invalid.
            return;
        }
        unityTransport.SetConnectionData(addressInput.value, portValue, "0.0.0.0");
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
