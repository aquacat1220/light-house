using System;
using FishNet.Object;
using UnityEngine;

public class SpawnFollowCamera : NetworkBehaviour
{
    // Reference to the FollowCamera prefab to use.
    [SerializeField]
    GameObject _followCameraPrefab;

    // Reference to created FollowCamera.
    GameObject _followCameraRef;

    void Awake()
    {
        if (_followCameraPrefab == null)
        {
            Debug.Log("`followCamera` wasn't set.");
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
        if (_followCameraRef != null)
        {
            Debug.Log("`followCameraRef` is not null before spawning.");
            throw new Exception();
        }
        _followCameraRef = Instantiate(_followCameraPrefab);
        _followCameraRef.transform.parent = this.transform;
        _followCameraRef.GetComponent<FollowCamera>().Target = transform;
    }

    // Destroy the followcam.
    public override void OnStopClient()
    {
        // We don't check for ownership here, just in case something went wrong and we got a followcam for a non-owner.
        if (_followCameraRef != null)
        {
            Destroy(_followCameraRef);
        }
    }
}
