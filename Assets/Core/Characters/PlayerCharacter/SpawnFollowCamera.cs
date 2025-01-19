using System;
using Unity.Netcode;
using UnityEngine;

public class SpawnFollowCamera : NetworkBehaviour
{
    // Reference to the FollowCamera prefab to use.
    [SerializeField]
    GameObject followCameraPrefab;

    void Awake()
    {
        if (followCameraPrefab == null)
        {
            Debug.Log("\"followCamera\" wasn't set.");
            throw new Exception();
        }
    }

    // Check if we are the owner, and intantiate a follow camera if so.
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
        GameObject followCamera = Instantiate(followCameraPrefab);
        followCamera.GetComponent<FollowCamera>().target = transform;
    }
}
