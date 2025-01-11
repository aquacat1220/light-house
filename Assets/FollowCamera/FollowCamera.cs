using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    // The target transform to follow.
    public Transform target;
    // Whether or not to follow the rotation of the target.
    public bool followRotation;

    void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, target.position.y, -10);
        if (followRotation)
        {
            transform.rotation = target.rotation;
        }
    }
}
