using System;
using FishNet.Object;
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
    public override void OnStartClient()
    {
        if (!base.IsOwner)
        {
            // We are not the owning client. Non-local-controlled characters don't need cameras.
            return;
        }
        // We are the owning client. This character (or whatever) is locally controlled.

        // Instantiate a follow camera, and set it to follow this character.
        // This happens only locally, since we don't need cameras to be synced.
        if (followCameraRef != null)
        {
            Debug.Log("\"followCameraRef\" is not null before spawning.");
            throw new Exception();
        }
        followCameraRef = Instantiate(followCameraPrefab);
        followCameraRef.transform.parent = this.transform;
        followCameraRef.GetComponent<FollowCamera>().target = transform;
    }

    // Destroy the followcam.
    public override void OnStopClient()
    {
        // We don't check for ownership here, just in case something went wrong and we got a followcam for a non-owner.
        if (followCameraRef != null)
        {
            Destroy(followCameraRef);
        }
    }
}
