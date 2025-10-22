using FishNet.Connection;
using FishNet.Object;
using FishNet.Serializing;
using UnityEngine;

public class SpawnTestDummy : NetworkBehaviour
{
    public override void OnStartNetwork()
    {
        Debug.Log("SpawnTestDummy OnStartNetwork");
    }

    public override void OnStartServer()
    {
        Debug.Log("SpawnTestDummy OnStartServer");
    }

    public override void OnStartClient()
    {
        Debug.Log("SpawnTestDummy OnStartClient");
    }

    public override void OnStopNetwork()
    {
        Debug.Log("SpawnTestDummy OnStopNetwork");
    }

    public override void OnStopServer()
    {
        Debug.Log("SpawnTestDummy OnStopServer");
    }

    public override void OnStopClient()
    {
        Debug.Log("SpawnTestDummy OnStopClient");
    }

    public override void WritePayload(NetworkConnection connection, Writer writer)
    {
        Debug.Log("SpawnTestDummy WritePayload");
        writer.WriteUInt16(1);
    }

    public override void ReadPayload(NetworkConnection connection, Reader reader)
    {
        Debug.Log("SpawnTestDummy ReadPayload");
        reader.ReadUInt16();
    }
}
