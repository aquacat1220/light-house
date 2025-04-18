using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerCharacterWeapon : NetworkBehaviour
{
    // Reference to the light component of the muzzle.
    [SerializeField]
    Light2D _muzzleFlash;
    // Time in seconds for the muzzle flash to vanish.
    [SerializeField]
    float _muzzleFlashDuration;
    // Intensity gain of muzzle flash per shot.
    [SerializeField]
    float _muzzleFlashPerShot;

    // Light2D component of the character's flashlight.
    [SerializeField]
    Light2D _flashlight;

    // Line renderer component for the aim line.
    [SerializeField]
    LineRenderer _aimLine;

    // Prefab to spawn on bullet impact.
    [SerializeField]
    GameObject _bulletImpactPrefab;

    // Reference to InputAction for character weapons.
    InputAction _fireAction;
    InputAction _toggleLightAction;

    // Intensity of the flashlight when it's on. Stored before turning the light off.
    float _flashlightIntensity;

    // Is the component subscribed to the action?
    bool _isSubscribedToAction = false;

    void Awake()
    {
        if (_muzzleFlash == null)
        {
            Debug.Log("\"muzzleFlash\" wasn't set.");
            throw new Exception();
        }

        if (_aimLine == null)
        {
            Debug.Log("\"aimLine\" wasn't set.");
            throw new Exception();
        }

        if (_bulletImpactPrefab == null)
        {
            Debug.Log("\"bulletImpact\" wasn't set.");
            throw new Exception();
        }

        _fireAction = InputSystem.actions.FindAction("Fire");
        if (_fireAction == null)
        {
            Debug.Log("\"Fire\" action wasn't found.");
            throw new Exception();
        }

        _toggleLightAction = InputSystem.actions.FindAction("ToggleLight");
        if (_toggleLightAction == null)
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
        if (!_isSubscribedToAction)
        {
            _fireAction.performed += OnFireAction;
            _toggleLightAction.performed += OnToggleLightAction;
            _isSubscribedToAction = true;
        }
    }
    void UnsubscribeFromAction()
    {
        if (_isSubscribedToAction)
        {
            _fireAction.performed -= OnFireAction;
            _toggleLightAction.performed -= OnToggleLightAction;
            _isSubscribedToAction = false;
        }
    }

    void OnFireAction(InputAction.CallbackContext context)
    {
        ServerFireRpc();
    }

    [ServerRpc]
    void ServerFireRpc()
    {
        RaycastHit2D hit = Physics2D.Raycast(_muzzleFlash.transform.position, _muzzleFlash.transform.up);
        if (hit)
        {
            GameObject newBulletImpact = Instantiate(_bulletImpactPrefab, hit.point + hit.normal * 0.01f, Quaternion.identity);
            base.Spawn(newBulletImpact);
        }
        MuzzleFlashRpc();
    }

    [ObserversRpc]
    void MuzzleFlashRpc()
    {
        _muzzleFlash.intensity = Mathf.Clamp(_muzzleFlash.intensity + _muzzleFlashPerShot, 0, 1);
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
        if (_flashlight.intensity != 0f)
        {
            _flashlightIntensity = _flashlight.intensity;
            _flashlight.intensity = 0f;
        }
        else
        {
            _flashlight.intensity = _flashlightIntensity;
        }
    }

    void DrawAimLine()
    {
        if (!base.IsOwner)
        {
            // If not owning client, do not draw aim lines.
            return;
        }
        _aimLine.SetPosition(0, _muzzleFlash.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(_muzzleFlash.transform.position, _muzzleFlash.transform.up);
        _aimLine.SetPosition(1, hit.point);
    }

    void Update()
    {
        // Lower down muzzle flash intensity over time.
        _muzzleFlash.intensity = Mathf.Clamp(_muzzleFlash.intensity - (Time.deltaTime / _muzzleFlashDuration), 0, 1);
        // Draw the aim line.
        DrawAimLine();
    }
}
