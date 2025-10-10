using FishNet.Connection;
using FishNet.Object;
using FishNet.Serializing;
using UnityEngine;

public class SpawnTestDummy : NetworkBehaviour
{
    public override void OnStartNetwork()
    {
        Debug.Log("OnStartNetwork");
    }

    public override void OnStartServer()
    {
        Debug.Log("OnStartServer");
    }

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient");
    }

    public override void WritePayload(NetworkConnection connection, Writer writer)
    {
        Debug.Log("WritePayload");
        writer.WriteUInt16(1);
    }

    public override void ReadPayload(NetworkConnection connection, Reader reader)
    {
        Debug.Log("ReadPayload");
        reader.ReadUInt16();
    }
}
