using System;
using NaughtyAttributes;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public static FollowCamera Singleton { get; private set; }

    // The target transform to follow.
    public Transform Target;
    // Whether or not to follow the rotation of the target.
    public bool FollowRotation;
    [Required]
    public Camera Camera;

    void Awake()
    {
        if (Singleton != null)
        {
            Debug.Log("`Singleton` was non-null, implying there are multiple instances of `FollowCamera`s in this scene.");
            throw new Exception();
        }
        Singleton = this;
    }

    void LateUpdate()
    {
        if (Target == null)
        {
            // If target transform is null, abort.
            return;
        }
        transform.position = new Vector3(Target.position.x, Target.position.y, -10);
        transform.rotation = Quaternion.identity;
        if (FollowRotation)
        {
            transform.rotation = Target.rotation;
        }
    }
}
