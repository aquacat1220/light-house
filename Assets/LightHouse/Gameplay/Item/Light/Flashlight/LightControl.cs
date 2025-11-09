namespace LightHouse
{
    using System;
    using FishNet;
    using FishNet.Managing.Timing;
    using FishNet.Object;
    using NaughtyAttributes;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

    public class LightControl : NetworkBehaviour
    {

        [Required]
        [ValidateInput("IsValidLight", "The light should be a point light with blend mode \"Default\"")]
        [SerializeField]
        Light2D _light;

        [SerializeField]
        float _timeToChange = 1f;

        float _rangeChangeRate = 0f;
        float _angleChangeRate = 0f;
        float _intensityChangeRate = 0f;

        [SerializeField]
        [MinMaxSlider(0f, 100f)]
        Vector2 _minMaxRange;
        [SerializeField]
        [MinMaxSlider(0f, 360f)]
        Vector2 _minMaxAngle;
        [SerializeField]
        [MinMaxSlider(0f, 1f)]
        Vector2 _minMaxIntensity;

        [SerializeField]
        float _initialRange = 0f;
        [SerializeField]
        float _initialAngle = 0f;
        [SerializeField]
        float _initialIntensity = 0f;
        [SerializeField]
        bool _initialEnabled = false;

        [SerializeField, Range(0f, 1f)]
        float _innerToOuterRangeRatio = 0.8f;
        [SerializeField, Range(0f, 1f)]
        float _innerToOuterAngleRatio = 0.8f;

        Vision _vision;
        Vision.RangeHandle? _handle = null;

        Alarm _alarm;

        short _rangeChange = 0;
        short _intensityChange = 0;
        short _angleChange = 0;

        void Awake()
        {
            if (_timeToChange != 0f)
            {
                _rangeChangeRate = (_minMaxRange.y - _minMaxRange.x) / _timeToChange;
                _angleChangeRate = (_minMaxAngle.y - _minMaxAngle.x) / _timeToChange;
                _intensityChangeRate = (_minMaxIntensity.y - _minMaxIntensity.x) / _timeToChange;
            }
            // Call all `SetXX()` functions to ensure the minmax intensitys are applied.
            SetEnabled(_initialEnabled);
            SetRange(_initialRange);
            SetAngle(_initialAngle);
            SetIntensity(_initialIntensity);
        }

        public void OnRegister(ItemSlot itemSlot)
        {
            // Controlling vision is only possible on the server.
            if (base.IsServerInitialized)
                _vision = itemSlot.User.GetComponent<Vision>();
            RefreshVision();
            _alarm = TimerManager.Singleton.AddAlarm(
                cooldown: (float)TimeManager.TickDelta,
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
            if (_vision != null && _handle is Vision.RangeHandle handle)
            {
                _vision.RemoveRange(handle);
                _handle = null;
            }
            _vision = null;
            _alarm.Remove();
        }

        [Server]
        public void Toggle()
        {
            if (_light.enabled)
                SetEnabled(false);
            else
                SetEnabled(true);
            SyncState();
            RefreshVision();
        }

        [Server]
        public void On()
        {
            SetEnabled(true);
            SyncState();
            RefreshVision();
        }

        [Server]
        public void Off()
        {
            SetEnabled(false);
            SyncState();
            RefreshVision();
        }

        [Server]
        public void StartRangeChange(bool up)
        {
            _rangeChange = up ? (short)1 : (short)-1;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StopRangeChange()
        {
            _rangeChange = 0;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StartIntensityChange(bool up)
        {
            _intensityChange = up ? (short)1 : (short)-1;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StopIntensityChange()
        {
            _intensityChange = 0;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StartAngleChange(bool up)
        {
            _angleChange = up ? (short)1 : (short)-1;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StopAngleChange()
        {
            _angleChange = 0;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        void SyncState()
        {
            SyncStateRpc(_rangeChange, _light.pointLightOuterRadius, _intensityChange, _light.intensity, _angleChange, _light.pointLightOuterAngle, _light.enabled);
        }

        [ObserversRpc(BufferLast = true, ExcludeServer = true)]
        void SyncStateRpc(short rangeChange, float range, short intensityChange, float intensity, short angleChange, float angle, bool enabled)
        {
            SetRange(range);
            SetAngle(angle);
            SetIntensity(intensity);
            SetEnabled(enabled);
            _rangeChange = rangeChange;
            _angleChange = angleChange;
            _intensityChange = intensityChange;
            _alarm.Arm();
        }

        void OnAlarm(float _)
        {
            if (_rangeChangeRate != 0f)
                SetRange(_light.pointLightOuterRadius + _rangeChange * _rangeChangeRate * (float)TimeManager.TickDelta);
            else
            {
                if (_rangeChange > 0)
                    SetRange(_minMaxRange.y);
                else if (_rangeChange < 0)
                    SetRange(_minMaxRange.x);
            }
            if (_angleChangeRate != 0f)
                SetAngle(_light.pointLightOuterAngle + _angleChange * _angleChangeRate * (float)TimeManager.TickDelta);
            else
            {
                if (_angleChange > 0)
                    SetAngle(_minMaxAngle.y);
                else if (_angleChange < 0)
                    SetAngle(_minMaxAngle.x);
            }
            if (_intensityChangeRate != 0f)
                SetIntensity(_light.intensity + _intensityChange * _intensityChangeRate * (float)TimeManager.TickDelta);
            else
            {
                if (_intensityChange > 0)
                    SetIntensity(_minMaxIntensity.y);
                else if (_intensityChange < 0)
                    SetIntensity(_minMaxIntensity.x);
            }

            RefreshVision();

            // If no more changes are detected, put the alarm to sleep.
            if (_rangeChange == 0 && _intensityChange == 0 && _angleChange == 0)
                _alarm.Disarm();
        }

        void RefreshVision()
        {
            // If `_vision` is null, we are not connected to a vision component.
            if (_vision == null)
                return;
            // First remove the current handle.
            if (_handle is Vision.RangeHandle handle)
            {
                _vision.RemoveRange(handle);
                _handle = null;
            }
            // Then install a refreshed handle if we need to (the light is active).
            if (_light.enabled)
            {
                _handle = _vision.AddRange(_light.pointLightOuterRadius, _light.pointLightOuterRadius);
            }
        }

        void SetEnabled(bool newEnabled)
        {
            _light.enabled = newEnabled;
            // RefreshVision();
        }

        void SetRange(float newRange)
        {
            newRange = Math.Clamp(newRange, _minMaxRange.x, _minMaxRange.y);
            _light.pointLightOuterRadius = newRange;
            _light.pointLightInnerRadius = newRange * _innerToOuterRangeRatio;
            // RefreshVision();
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
            if (light.blendStyleIndex != 1)
                return false;
            return true;
        }
    }
}
