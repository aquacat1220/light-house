using System;
using Unity.Netcode;
using UnityEngine;

public class SpawnFollowCamera : NetworkBehaviour
{
    // Reference to the FollowCamera prefab to use.
    [SerializeField]
    GameObject followCameraPrefab;

    // Reference to created FollowCamera.
    GameObject followCameraRef;

    void Awake()
    {
        if (followCameraPrefab == null)
        {
            Debug.Log("\"followCamera\" wasn't set.");
            throw new Exception();
        }
    }

    // Check if we are the owner, and instantiate a follow camera if so.
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            // We are not the owner. Non-local-controlled characters don't need cameras.
            return;
        }
        // We are the owner. This character (or whatever) is locally controlled.

        // Instantiate a follow camera, and set it to follow this character.
        // This happens only locally, since we don't need cameras to be synced.
        if (followCameraRef != null)
        {
            Debug.Log("\"followCameraRef\" is not null before spawning.");
            throw new Exception();
        }
        followCameraRef = Instantiate(followCameraPrefab);
        followCameraRef.GetComponent<FollowCamera>().target = transform;
    }

    // Check if we are the owner (so we must've spawned a camera), and destroy the camera if so.
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner)
        {
            // We are not the owner. No need to cleanup the camera.
            return;
        }
        // We are the owner. Find and destroy the previously created camera.
        if (followCameraRef == null)
        {
            Debug.Log("\"followCameraRef\" is null despite being the owner.");
            throw new Exception();
        }

        Destroy(followCameraRef);
    }
}
