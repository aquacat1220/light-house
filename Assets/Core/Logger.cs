using FishNet.Object;
using UnityEngine;

public class Logger : NetworkBehaviour
{
    public void Log(string str)
    {
        Debug.Log($"{Time.time}: {str}");
    }

    public override void OnStartNetwork()
    {
        Debug.Log("Network");
    }

    public override void OnStartServer()
    {
        Debug.Log("Server");
    }

    public override void OnStartClient()
    {
        Debug.Log("Client");
    }
}
