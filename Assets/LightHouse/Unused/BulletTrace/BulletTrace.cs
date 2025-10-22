using System;
using System.Collections;
using UnityEngine;

public class BulletTrace : MonoBehaviour
{
    [SerializeField]
    float _destroyAfter = 1f;
    [SerializeField]
    LineRenderer _lineRenderer;

    void Awake()
    {
        if (_lineRenderer == null)
        {
            Debug.Log("`_lineRenderer` wasn't set.");
            throw new Exception();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, _destroyAfter);
    }

    // Sets the end point of this bullet trace in world position.
    // The line renderer still operates in local space, so the world space position will first be translated to local space.
    // Thus, moving the bullet trace gameobject will move the end position with it.
    public void SetEndPosition(Vector3 worldEndPosition)
    {
        Vector3 localEndPositon = transform.InverseTransformPoint(worldEndPosition);
        _lineRenderer.SetPosition(1, localEndPositon);
    }
}
