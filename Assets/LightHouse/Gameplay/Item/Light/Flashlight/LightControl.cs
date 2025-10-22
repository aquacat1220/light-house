using System;
using FishNet;
using FishNet.Managing.Timing;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightControl : MonoBehaviour
{

    [Required]
    [ValidateInput("IsValidLight", "The light should be a point light with blend mode \"Default\"")]
    [SerializeField]
    Light2D _light;

    [SerializeField]
    float _rangeChangeRate = 2f;
    [SerializeField]
    float _angleChangeRate = 45f;
    [SerializeField]
    float _intensityChangeRate = 0.5f;

    [SerializeField]
    [MinMaxSlider(0f, 100f)]
    Vector2 _minMaxRange;
    [SerializeField]
    [MinMaxSlider(0f, 360f)]
    Vector2 _minMaxAngle;
    [SerializeField]
    [MinMaxSlider(0f, 1f)]
    Vector2 _minMaxIntensity;

    float _innerToOuterRangeRatio = 0f;
    float _innerToOuterAngleRatio = 0f;

    Vision _vision;
    Vision.RangeHandle? _handle = null;

    Alarm _alarm;

    bool _isIncreasingRange = false;
    bool _isDecreasingRange = false;
    bool _isIncreasingAngle = false;
    bool _isDecreasingAngle = false;
    bool _isIncreasingIntensity = false;
    bool _isDecreasingIntensity = false;

    void Awake()
    {
        _innerToOuterRangeRatio = _light.pointLightInnerRadius / _light.pointLightOuterRadius;
        _innerToOuterAngleRatio = _light.pointLightInnerAngle / _light.pointLightOuterAngle;
        // Call all `SetXX()` functions to ensure the minmax ranges are applied.
        SetEnabled(_light.enabled);
        SetRange(_light.pointLightOuterRadius);
        SetAngle(_light.pointLightOuterAngle);
        SetIntensity(_light.intensity);
    }

    public void OnRegister(ItemSlot itemSlot)
    {
        // Controlling vision is only possible on the server.
        if (itemSlot.IsServerInitialized)
            _vision = itemSlot.User.GetComponent<Vision>();
        RefreshVision();
        _alarm = TimerManager.Singleton.AddAlarm(
            cooldown: (float)InstanceFinder.TimeManager.TickDelta,
            callback: OnAlarm,
            startImmediately: true,
            armImmediately: true,
            autoRestart: true,
            autoRearm: true,
            initialCooldown: 0f,
            destroyAfterTriggered: false
        );
    }

    public void OnUnregister()
    {
        if (_handle is Vision.RangeHandle handle)
        {
            _vision?.RemoveRange(handle);
            _handle = null;
        }
        _vision = null;
        _alarm?.Remove();
    }

    public void Toggle()
    {
        if (_light.enabled)
            SetEnabled(false);
        else
            SetEnabled(true);
    }

    public void On()
    {
        SetEnabled(true);
    }

    public void Off()
    {
        SetEnabled(false);
    }

    public void IncreaseRange(bool start)
    {
        _isIncreasingRange = start;
        _alarm?.Arm();
    }

    public void DecreaseRange(bool start)
    {
        _isDecreasingRange = start;
        _alarm?.Arm();
    }

    public void IncreaseAngle(bool start)
    {
        _isIncreasingAngle = start;
        _alarm?.Arm();
    }

    public void DecreaseAngle(bool start)
    {
        _isDecreasingAngle = start;
        _alarm?.Arm();
    }

    public void IncreaseIntensity(bool start)
    {
        _isIncreasingIntensity = start;
        _alarm?.Arm();
    }

    public void DecreaseIntensity(bool start)
    {
        _isDecreasingIntensity = start;
        _alarm?.Arm();
    }

    void OnAlarm()
    {
        var rangeChange = 0f;
        if (_isIncreasingRange)
            rangeChange += 1f;
        if (_isDecreasingRange)
            rangeChange -= 1f;
        if (rangeChange != 0f)
            SetRange(_light.pointLightOuterRadius + rangeChange * _rangeChangeRate * (float)InstanceFinder.TimeManager.TickDelta);

        var angleChange = 0f;
        if (_isIncreasingAngle)
            angleChange += 1f;
        if (_isDecreasingAngle)
            angleChange -= 1f;
        if (angleChange != 0f)
            SetAngle(_light.pointLightOuterAngle + angleChange * _angleChangeRate * (float)InstanceFinder.TimeManager.TickDelta);

        var intensityChange = 0f;
        if (_isIncreasingIntensity)
            intensityChange += 1f;
        if (_isDecreasingIntensity)
            intensityChange -= 1f;
        if (intensityChange != 0f)
            SetIntensity(_light.intensity + intensityChange * _intensityChangeRate * (float)InstanceFinder.TimeManager.TickDelta);

        // If no more changes are detected, put the alarm to sleep.
        if (!_isIncreasingRange && !_isDecreasingRange && !_isIncreasingAngle && !_isDecreasingAngle && !_isIncreasingIntensity && !_isDecreasingIntensity)
            _alarm?.Disarm();
    }

    void RefreshVision()
    {
        // If `_light` is null, we are not connected to a vision component.
        if (_light == null)
            return;
        // First remove the current handle.
        if (_handle is Vision.RangeHandle handle)
        {
            _vision?.RemoveRange(handle);
            _handle = null;
        }
        // Then install a refreshed handle if we need to (the light is active).
        if (_light.enabled)
        {
            _handle = _vision?.AddRange(_light.pointLightOuterRadius, _light.pointLightOuterRadius);
        }
    }

    void SetEnabled(bool newEnabled)
    {
        _light.enabled = newEnabled;
        RefreshVision();
    }

    void SetRange(float newRange)
    {
        newRange = Math.Clamp(newRange, _minMaxRange.x, _minMaxRange.y);
        _light.pointLightOuterRadius = newRange;
        _light.pointLightInnerRadius = newRange * _innerToOuterRangeRatio;
        RefreshVision();
    }

    void SetAngle(float newAngle)
    {
        newAngle = Math.Clamp(newAngle, _minMaxAngle.x, _minMaxAngle.y);
        _light.pointLightOuterAngle = newAngle;
        _light.pointLightInnerAngle = newAngle * _innerToOuterAngleRatio;
        // RefreshVision();
    }

    void SetIntensity(float newIntensity)
    {
        newIntensity = Math.Clamp(newIntensity, _minMaxIntensity.x, _minMaxIntensity.y);
        _light.intensity = newIntensity;
        // RefreshVision();
    }

    bool IsValidLight(Light2D light)
    {
        if (light == null)
            return true;
        if (light.lightType != Light2D.LightType.Point)
            return false;
        if (light.blendStyleIndex != 0)
            return false;
        return true;
    }
}
