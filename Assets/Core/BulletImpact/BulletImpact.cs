using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BulletImpact : NetworkBehaviour
{
    // Reference to the light 2D component.
    public Light2D Light2D;
    // Time in seconds for the impact to vanish.
    public float BulletImpactDuration;

    // Initial light intensity.
    private float _initialIntensity;

    void Awake()
    {
        if (Light2D is null)
        {
            Debug.Log("\"light2D\" wasn't set.");
            throw new Exception();
        }

        _initialIntensity = Light2D.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        Light2D.intensity = Mathf.Clamp(Light2D.intensity - _initialIntensity * (Time.deltaTime / BulletImpactDuration), 0, 1);
        if (Light2D.intensity <= 0f)
        {
            if (base.IsSpawned && base.IsServerInitialized)
            {
                base.Despawn();
            }
        }
    }
}
