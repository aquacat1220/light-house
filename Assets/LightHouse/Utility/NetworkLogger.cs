using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class NetworkLogger : NetworkBehaviour
{
    void OnEnable()
    {
        Debug.Log($"[Time: {Time.time} | Owner: {base.Owner} | ServerInit: {base.IsServerInitialized} | ClientInit: {base.IsClientInitialized}] Enabled.");
    }

    void OnDisable()
    {
        Debug.Log($"[Time: {Time.time} | Owner: {base.Owner} | ServerInit: {base.IsServerInitialized} | ClientInit: {base.IsClientInitialized}] Disabled.");
    }

    public override void OnStartServer()
    {
        Debug.Log($"[Time: {Time.time} | Owner: {base.Owner} | ServerInit: {base.IsServerInitialized} | ClientInit: {base.IsClientInitialized}] Started as server.");
    }

    public override void OnStopServer()
    {
        Debug.Log($"[Time: {Time.time} | Owner: {base.Owner} | ServerInit: {base.IsServerInitialized} | ClientInit: {base.IsClientInitialized}] Stopped as server.");
    }

    public override void OnStartClient()
    {
        Debug.Log($"[Time: {Time.time} | Owner: {base.Owner} | ServerInit: {base.IsServerInitialized} | ClientInit: {base.IsClientInitialized}] Started as client.");
    }

    public override void OnStopClient()
    {
        Debug.Log($"[Time: {Time.time} | Owner: {base.Owner} | ServerInit: {base.IsServerInitialized} | ClientInit: {base.IsClientInitialized}] Stopped as client.");
    }

    public override void OnOwnershipServer(NetworkConnection prevOwner)
    {
        Debug.Log($"[Time: {Time.time} | Owner: {base.Owner} | ServerInit: {base.IsServerInitialized} | ClientInit: {base.IsClientInitialized}] Ownership change on server. Previous owner: {prevOwner}.");
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        Debug.Log($"[Time: {Time.time} | Owner: {base.Owner} | ServerInit: {base.IsServerInitialized} | ClientInit: {base.IsClientInitialized}] Ownership change on client. Previous owner: {prevOwner}.");
    }

    public void Log(string str)
    {
        Debug.Log($"[Time: {Time.time} | Owner: {base.Owner} | ServerInit: {base.IsServerInitialized} | ClientInit: {base.IsClientInitialized}] {str}");
    }
}
