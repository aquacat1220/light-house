using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BulletImpact : MonoBehaviour
{
    // Reference to the light 2D component.
    public Light2D light2D;
    // Time in seconds for the impact to vanish.
    public float bulletImpactDuration;

    // Initial light intensity.
    private float initialIntensity;

    void Start() {
        if (light2D is null) {
            Debug.Log("\"light2D\" wasn't set.");
            throw new Exception();
        }

        initialIntensity = light2D.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        light2D.intensity = Mathf.Clamp(light2D.intensity - initialIntensity * (Time.deltaTime / bulletImpactDuration), 0, 1);
        if (light2D.intensity <= 0f) {
            Destroy(gameObject);
        }
    }
}
