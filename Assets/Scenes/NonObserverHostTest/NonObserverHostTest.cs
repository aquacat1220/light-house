using System;
using System.Linq;
using FishNet;
using FishNet.Component.Observing;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class NonObserverHostTest : NetworkBehaviour
{
    void Awake()
    {
        if (InstanceFinder.ServerManager.StartConnection(7902))
            Debug.Log("Success! Started as server!");
        else
            Debug.Log("Failure...");

        if (InstanceFinder.ClientManager.StartConnection("127.0.0.1", 7902))
            Debug.Log("Success! Started as client!");
        else
            Debug.Log("Failure...");
    }

    void Update()
    {
        if (!base.IsServerInitialized)
            return;
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            MatchCondition.AddToMatch(1, InstanceFinder.ServerManager.Clients.Values.ToArray());
        }
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            MatchCondition.RemoveFromMatch(1, InstanceFinder.ServerManager.Clients.Values.ToArray(), null);
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ObserverRpc();
        }
    }

    public override void OnStartServer()
    {
        MatchCondition.AddToMatch(1, NetworkObject);
        Debug.Log("SERVER");
    }

    public override void OnStartClient()
    {
        Debug.Log("CLIENT START");
    }

    public override void OnStopClient()
    {
        Debug.Log("CLIENT STOP");
    }

    [ObserversRpc()]
    void ObserverRpc()
    {
        Debug.Log("RPC");
    }
}
