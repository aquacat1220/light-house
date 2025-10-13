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
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            SwitchCondition.Active = true;
            Debug.Log("All nobs are now observable to all clients!");
        }
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            SwitchCondition.Active = false;
            Debug.Log("All nobs are now unobservable to all clients!");
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ObserverRpc();
        }
    }
    public override void OnStartNetwork()
    {
        Debug.Log("SpawnTest OnStartNetwork");
    }

    public override void OnStartServer()
    {
        Debug.Log("SpawnTest OnStartServer");
    }

    public override void OnStartClient()
    {
        Debug.Log("SpawnTest OnStartClient");
    }

    public override void OnStopNetwork()
    {
        Debug.Log("SpawnTest OnStopNetwork");
    }

    public override void OnStopServer()
    {
        Debug.Log("SpawnTest OnStopServer");
    }

    public override void OnStopClient()
    {
        Debug.Log("SpawnTest OnStopClient");
    }


    [ObserversRpc()]
    void ObserverRpc()
    {
        Debug.Log("RPC");
    }
}
