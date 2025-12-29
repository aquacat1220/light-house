namespace LightHouse
{
    using FishNet.Object;
    using UnityEngine;
    using FishNet.Connection;

    public class NetworkEvent : NetworkBehaviour
    {
        [SerializeField]
        Fn.Event _onStartNetwork;
        [SerializeField]
        Fn.Event _onStartServer;
        [SerializeField]
        Fn.Event _onStartClient;
        [SerializeField]
        Fn.Event _onStopNetwork;
        [SerializeField]
        Fn.Event _onStopServer;
        [SerializeField]
        Fn.Event _onStopClient;
        [SerializeField]
        Fn.Event<NetworkConnection> _onOwnershipServer;
        [SerializeField]
        Fn.Event<NetworkConnection> _onOwnershipClient;

        public override void OnStartNetwork()
        {
            _onStartNetwork.Invoke();
        }

        public override void OnStartServer()
        {
            _onStartServer.Invoke();
        }

        public override void OnStartClient()
        {
            _onStartClient.Invoke();
        }
        public override void OnStopNetwork()
        {
            _onStopNetwork.Invoke();
        }

        public override void OnStopServer()
        {
            _onStopServer.Invoke();
        }

        public override void OnStopClient()
        {
            _onStopClient.Invoke();
        }

        public override void OnOwnershipServer(NetworkConnection prevOwner)
        {
            _onOwnershipServer.Invoke(prevOwner);
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            _onOwnershipClient.Invoke(prevOwner);
        }
    }
}