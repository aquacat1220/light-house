using FishNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrudeNetworkStarter : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.sKey.wasPressedThisFrame)
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
    }
}
