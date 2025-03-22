using System;
using FishNet.Object;
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

    // Is the component subscribed to the action?
    bool isSubscribedToAction = false;

    void Awake()
    {
        if (muzzleFlash == null)
        {
            Debug.Log("\"muzzleFlash\" wasn't set.");
            throw new Exception();
        }

        if (aimLine == null)
        {
            Debug.Log("\"aimLine\" wasn't set.");
            throw new Exception();
        }

        if (bulletImpactPrefab == null)
        {
            Debug.Log("\"bulletImpact\" wasn't set.");
            throw new Exception();
        }

        fireAction = InputSystem.actions.FindAction("Fire");
        if (fireAction == null)
        {
            Debug.Log("\"Fire\" action wasn't found.");
            throw new Exception();
        }

        toggleLightAction = InputSystem.actions.FindAction("ToggleLight");
        if (toggleLightAction == null)
        {
            Debug.Log("\"ToggleLight\" action wasn't found.");
            throw new Exception();
        }
    }

    public override void OnStartClient()
    {
        if (base.IsOwner)
        {
            // We are the owning client of this character. Attach weapon functions to the action.
            SubscribeToAction();
        }
    }

    public override void OnStopClient()
    {
        // We don't check for ownership here, since calling `UnsubscribeFromAction()` when we are not subscribed shouldn't cause any problems.
        UnsubscribeFromAction();
    }

    void OnEnable()
    {
        if (base.IsOwner)
        {
            // We are the owning client of this character. Attach weapon functions to the action.
            // We need this functionality because we unsubscribe on disable.
            SubscribeToAction();
        }
    }

    void OnDisable()
    {
        // We don't check for ownership here, since calling `UnsubscribeFromAction()` when we are not subscribed shouldn't cause any problems.
        UnsubscribeFromAction();
    }

    void SubscribeToAction()
    {
        if (!isSubscribedToAction)
        {
            fireAction.performed += OnFireAction;
            toggleLightAction.performed += OnToggleLightAction;
            isSubscribedToAction = true;
        }
    }
    void UnsubscribeFromAction()
    {
        if (isSubscribedToAction)
        {
            fireAction.performed -= OnFireAction;
            toggleLightAction.performed -= OnToggleLightAction;
            isSubscribedToAction = false;
        }
    }

    void OnFireAction(InputAction.CallbackContext context)
    {
        ServerFireRpc();
    }

    [ServerRpc]
    void ServerFireRpc()
    {
        RaycastHit2D hit = Physics2D.Raycast(muzzleFlash.transform.position, muzzleFlash.transform.up);
        if (hit)
        {
            GameObject newBulletImpact = Instantiate(bulletImpactPrefab, hit.point + hit.normal * 0.01f, Quaternion.identity);
            base.Spawn(newBulletImpact);
        }
        MuzzleFlashRpc();
    }

    [ServerRpc]
    void MuzzleFlashRpc()
    {
        muzzleFlash.intensity = Mathf.Clamp(muzzleFlash.intensity + muzzleFlashPerShot, 0, 1);
    }

    void OnToggleLightAction(InputAction.CallbackContext context)
    {
        ServerToggleLightRpc();
    }

    [ServerRpc]
    void ServerToggleLightRpc()
    {
        ToggleLightRpc();
    }

    [ObserversRpc]
    void ToggleLightRpc()
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
        if (!base.IsOwner)
        {
            // If not owning client, do not draw aim lines.
            return;
        }
        aimLine.SetPosition(0, muzzleFlash.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(muzzleFlash.transform.position, muzzleFlash.transform.up);
        aimLine.SetPosition(1, hit.point);
    }

    void Update()
    {
        // Lower down muzzle flash intensity over time.
        muzzleFlash.intensity = Mathf.Clamp(muzzleFlash.intensity - (Time.deltaTime / muzzleFlashDuration), 0, 1);
        // Draw the aim line.
        DrawAimLine();
    }
}
