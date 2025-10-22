using System;
using FishNet.Object;
using UnityEngine;

public class PlayerCharacterCamera : NetworkBehaviour
{
    [SerializeField]
    [Min(1f)]
    float _minimumCameraSize = 4f;
    // Check if we are the owner, and instantiate a follow camera if so.
    public override void OnStartClient()
    {
        if (!base.IsOwner)
        {
            // We are not the owning client. Non-local-controlled characters don't need cameras.
            return;
        }
        // We are the owning client. This character (or whatever) is locally controlled.

        // Find the `FollowCamera` singleton, and make it follow `this`.
        var followCamera = FollowCamera.Singleton;
        if (followCamera == null)
        {
            Debug.Log("`FollowCamera.Singleton` was null, implying we do not have a follow camera in this scene.");
            throw new Exception();
        }
        followCamera.Target = transform;
    }

    public void OnRangeChanged(float newRange)
    {
        if (!base.IsOwner)
        {
            // We modify the camera range only if we are the owning client.
            return;
        }
        FollowCamera.Singleton.Camera.orthographicSize = Math.Max(newRange, _minimumCameraSize);
    }

}
