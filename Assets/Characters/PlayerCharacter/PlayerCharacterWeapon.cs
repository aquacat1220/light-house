using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerCharacterWeapon : MonoBehaviour
{
    // The tip of the muzzle; bullets are spawned here.
    public GameObject muzzleTip;
    // Time in seconds for the muzzle flash to vanish.
    public float muzzleFlashDuration;
    // Intensity gain of muzzle flash per shot.
    public float muzzleFlashPerShot;

    // Line renderer component for the aim line.
    public LineRenderer aimLine;

    // Prefab to spawn on bullet impact.
    public GameObject bulletImpact;
    // Time in seconds for the bullet impact to vanish.
    public float bulletImpactDuration;

    // Reference to the light component of the muzzle.
    private Light2D muzzleFlash;

    // Reference to InputAction for character weapons.
    private InputAction fireAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (muzzleTip is null) {
            Debug.Log("\"muzzleTip\" wasn't set.");
            throw new Exception();
        }
        muzzleFlash = muzzleTip.GetComponent<Light2D>();

        if (aimLine is null) {
            Debug.Log("\"aimLine\" wasn't set.");
            throw new Exception();
        }

        if (bulletImpact is null) {
            Debug.Log("\"bulletImpact\" wasn't set.");
            throw new Exception();
        }

        fireAction = InputSystem.actions.FindAction("Fire");
        if (fireAction is null) {
            Debug.Log("\"Fire\" action wasn't found.");
            throw new Exception();
        }
        fireAction.performed += Fire;
    }

    void Fire(InputAction.CallbackContext context) {
        RaycastHit2D hit = Physics2D.Raycast(muzzleTip.transform.position, muzzleTip.transform.up);
        if (hit) {
            GameObject newBulletImpact = Instantiate(bulletImpact, hit.point + hit.normal * 0.01f, Quaternion.identity);
        }
        muzzleFlash.intensity = Mathf.Clamp(muzzleFlash.intensity + muzzleFlashPerShot, 0, 1);
    }

    void DrawAimLine() {
        aimLine.SetPosition(0, muzzleFlash.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(muzzleTip.transform.position, muzzleTip.transform.up);
        aimLine.SetPosition(1, hit.point);
    }

    void Update() {
        // Lower down muzzle flash intensity.
        muzzleFlash.intensity = Mathf.Clamp(muzzleFlash.intensity - (Time.deltaTime / muzzleFlashDuration), 0, 1);
        // Update the aim line.
        DrawAimLine();
    }
}
