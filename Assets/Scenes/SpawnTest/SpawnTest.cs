using System;
using System.Linq;
using FishNet;
using FishNet.Component.Observing;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnTest : NetworkBehaviour
{
    [SerializeField]
    GameObject _spawnTestDummy;

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
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            var dummy = Instantiate(_spawnTestDummy, new Vector3(3f, 2f, 0f), Quaternion.identity);
            Spawn(dummy, base.Owner, gameObject.scene);
        }
        if (!base.IsServerInitialized)
            return;
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ObserverRpc();
        }
    }

    public override void OnStartServer()
    {
        Debug.Log("SERVER START");
    }

    public override void OnStopServer()
    {
        Debug.Log("SERVER STOP");
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
