using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerCharacterWeapon : NetworkBehaviour
{
    // Reference to the light component of the muzzle.
    [SerializeField]
    Light2D muzzleFlash;
    // Time in seconds for the muzzle flash to vanish.
    [SerializeField]
    float muzzleFlashDuration;
    // Intensity gain of muzzle flash per shot.
    [SerializeField]
    float muzzleFlashPerShot;

    // Light2D component of the character's flashlight.
    [SerializeField]
    Light2D flashlight;

    // Line renderer component for the aim line.
    [SerializeField]
    LineRenderer aimLine;

    // Prefab to spawn on bullet impact.
    [SerializeField]
    GameObject bulletImpactPrefab;
    // Time in seconds for the bullet impact to vanish.
    [SerializeField]
    float bulletImpactDuration;

    // Reference to InputAction for character weapons.
    InputAction fireAction;
    InputAction toggleLightAction;

    // Intensity of the flashlight when it's on. Stored before turning the light off.
    float flashlightIntensity;

    void Awake()
    {
        if (muzzleFlash is null)
        {
            Debug.Log("\"muzzleFlash\" wasn't set.");
            throw new Exception();
        }

        if (aimLine is null)
        {
            Debug.Log("\"aimLine\" wasn't set.");
            throw new Exception();
        }

        if (bulletImpactPrefab is null)
        {
            Debug.Log("\"bulletImpact\" wasn't set.");
            throw new Exception();
        }

        fireAction = InputSystem.actions.FindAction("Fire");
        if (fireAction is null)
        {
            Debug.Log("\"Fire\" action wasn't found.");
            throw new Exception();
        }
        fireAction.performed += Fire;

        toggleLightAction = InputSystem.actions.FindAction("ToggleLight");
        if (toggleLightAction is null)
        {
            Debug.Log("\"ToggleLight\" action wasn't found.");
            throw new Exception();
        }
        toggleLightAction.performed += ToggleLight;
    }

    void Fire(InputAction.CallbackContext context)
    {
        RaycastHit2D hit = Physics2D.Raycast(muzzleFlash.transform.position, muzzleFlash.transform.up);
        if (hit)
        {
            GameObject newBulletImpact = Instantiate(bulletImpactPrefab, hit.point + hit.normal * 0.01f, Quaternion.identity);
        }
        muzzleFlash.intensity = Mathf.Clamp(muzzleFlash.intensity + muzzleFlashPerShot, 0, 1);
    }

    void ToggleLight(InputAction.CallbackContext context)
    {
        if (flashlight.intensity != 0f)
        {
            flashlightIntensity = flashlight.intensity;
            flashlight.intensity = 0f;
        }
        else
        {
            flashlight.intensity = flashlightIntensity;
        }
    }

    void DrawAimLine()
    {
        aimLine.SetPosition(0, muzzleFlash.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(muzzleFlash.transform.position, muzzleFlash.transform.up);
        aimLine.SetPosition(1, hit.point);
    }

    void Update()
    {
        // Lower down muzzle flash intensity.
        muzzleFlash.intensity = Mathf.Clamp(muzzleFlash.intensity - (Time.deltaTime / muzzleFlashDuration), 0, 1);
        // Update the aim line.
        DrawAimLine();
    }
}
