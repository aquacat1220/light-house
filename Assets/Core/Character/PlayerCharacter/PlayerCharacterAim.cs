using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterAim : MonoBehaviour
{
    [SerializeField]
    Transform _aimPointTransform;

    [SerializeField]
    float _initialMinAimDistance = 0f;
    [SerializeField]
    float _initialMaxAimDistance = 30f;
    [SerializeField]
    float _initialAimDistance = 0f;

    float _minAimDistance = 0f;
    public float MinAimDistance
    {
        get
        {
            return _minAimDistance;
        }
        set
        {
            _minAimDistance = value;
            _maxAimDistance = Mathf.Max(value, _maxAimDistance);
            AimDistance = _aimDistance;
        }
    }

    float _maxAimDistance = 30f;
    public float MaxAimDistance
    {
        get
        {
            return _maxAimDistance;
        }
        set
        {
            _maxAimDistance = value;
            _minAimDistance = Mathf.Min(value, _minAimDistance);
            AimDistance = _aimDistance;
        }
    }

    float _aimDistance = 0f;
    public float AimDistance
    {
        get
        {
            return _aimDistance;
        }
        set
        {
            _aimDistance = Mathf.Clamp(value, _minAimDistance, _maxAimDistance);
            _aimPointTransform.localPosition = new Vector3(0f, _aimDistance, 0f);
        }
    }

    bool _isSubscribedToLook = false;

    void Awake()
    {
        if (_aimPointTransform == null)
        {
            Debug.Log("`_aimPointTransform` wasn't set.");
            throw new Exception();
        }
        // The guarantees are:
        // 1. `_minAimDistance <= _aimDistance && _aimDistance <= _maxAimDistance` is always true.
        // 2. `_aimPointTransform.localPosition == new Vector3(0f, _aimDistance, 0f)` is always true.
        // The property getter/setters guarantee that if the initial state is consistent, it will remain so.

        // Ensure we start consistently.
        _maxAimDistance = Mathf.Max(_minAimDistance, _maxAimDistance);
        _aimDistance = Mathf.Clamp(_aimDistance, _minAimDistance, _maxAimDistance);
        _aimPointTransform.SetLocalPositionAndRotation(new Vector3(0f, _aimDistance, 0f), Quaternion.identity);
        _aimPointTransform.localScale = Vector3.one;

        // And apply the initial values.
        MinAimDistance = _initialMinAimDistance;
        MaxAimDistance = _initialMaxAimDistance;
        AimDistance = _initialAimDistance;
    }

    void OnEnable()
    {
        if (!_isSubscribedToLook)
        {
            InputManager.Singleton.LookAction += OnLook;
            _isSubscribedToLook = true;
        }
    }

    void OnDisable()
    {
        if (_isSubscribedToLook)
        {
            InputManager.Singleton.LookAction -= OnLook;
            _isSubscribedToLook = false;
        }
    }

    void OnLook(InputAction.CallbackContext context)
    {
        float deltaY = context.ReadValue<Vector2>().y;
        AimDistance += deltaY * 0.01f;
    }
}
