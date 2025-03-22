using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    // The target transform to follow.
    public Transform Target;
    // Whether or not to follow the rotation of the target.
    public bool _followRotation;

    void LateUpdate()
    {
        if (Target == null)
        {
            // If target transform is null, abort.
            return;
        }
        transform.position = new Vector3(Target.position.x, Target.position.y, -10);
        transform.rotation = Quaternion.identity;
        if (_followRotation)
        {
            transform.rotation = Target.rotation;
        }
    }
}
