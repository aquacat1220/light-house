using UnityEngine;

public class RotationalMovement : MonoBehaviour
{
    [SerializeField]
    Transform _center;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(_center.position, _center.forward, 0.02f);
    }
}
