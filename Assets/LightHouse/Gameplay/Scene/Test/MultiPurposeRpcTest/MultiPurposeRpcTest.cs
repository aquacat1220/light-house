namespace LightHouse
{
    using FishNet.Connection;
    using FishNet.Object;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class MultiPurposeRpcTest : NetworkBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                Rpc(null, 10, ServerManager.Clients[1]);
            }
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                Rpc(ServerManager.Clients[1], 10, base.LocalConnection);
            }
        }

        [ObserversRpc]
        [TargetRpc]
        void Rpc(NetworkConnection conn, int val, NetworkConnection test)
        {
            Debug.Log($"Rpc triggered on {base.LocalConnection}, {test}");
        }
    }
}