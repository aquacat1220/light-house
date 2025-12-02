namespace LightHouse
{
    using System;
    using FishNet;
    using FishNet.Managing.Scened;
    using FishNet.Transporting;
    using NaughtyAttributes;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class SessionMenu : MonoBehaviour
    {
        [SerializeField]
        [Required]
        VisualTreeAsset _sessionMenu;

        MenuManager.MenuHandle _handle;

        TextField _addressInput;
        TextField _portInput;
        Button _hostButton;
        Button _joinButton;
        Button _soloButton;

        void Awake()
        {
            if (_sessionMenu == null)
            {
                Debug.Log("`_sessionMenu` wasn't set.");
                throw new Exception();
            }
        }

        void OnEnable()
        {
            _handle = MenuManager.Singleton.AddMenu(_sessionMenu, "Session");
            _addressInput = _handle.Menu.Q<TextField>("AddressInput");
            _portInput = _handle.Menu.Q<TextField>("PortInput");
            _hostButton = _handle.Menu.Q<Button>("HostButton");
            _joinButton = _handle.Menu.Q<Button>("JoinButton");
            _soloButton = _handle.Menu.Q<Button>("SoloButton");
            _hostButton.clicked += OnHostButtonClicked;
            _joinButton.clicked += OnJoinButtonClicked;
            _soloButton.clicked += OnSoloButtonClicked;
        }

        void OnDisable()
        {
            _hostButton.clicked -= OnHostButtonClicked;
            _joinButton.clicked -= OnJoinButtonClicked;
            _soloButton.clicked -= OnSoloButtonClicked;
            _addressInput = null;
            _portInput = null;
            _hostButton = null;
            _joinButton = null;
            _soloButton = null;
            MenuManager.Singleton.RemoveMenu(_handle);
            _handle = null;
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
    }
}
