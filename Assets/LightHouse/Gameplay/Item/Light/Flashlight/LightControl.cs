namespace LightHouse
{
    using System;
    using FishNet.Managing.Timing;
    using FishNet.Object;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using Fn;
    using FishNet.Connection;

    public class LightControl : NetworkBehaviour
    {
        [SerializeField]
        Light2D _light;

        [SerializeField]
        float _timeToChange = 1f;

        float _rangeChangeRate = 0f;
        float _angleChangeRate = 0f;
        float _intensityChangeRate = 0f;

        [SerializeField]
        Vector2 _minMaxRange;
        [SerializeField]
        Vector2 _minMaxAngle;
        [SerializeField]
        Vector2 _minMaxIntensity;

        [SerializeField]
        float _initialRange = 0f;
        [SerializeField]
        float _initialAngle = 0f;
        [SerializeField]
        float _initialIntensity = 0f;
        [SerializeField]
        bool _initialEnabled = false;

        [SerializeField]
        float _falloffRange = 0.1f;
        [SerializeField]
        float _falloffAngle = 1f;

        Vision _vision;
        Vision.RangeHandle _handle = null;

        Alarm _alarm;

        bool _rangeUp = false;
        bool _rangeDown = false;
        bool _intensityUp = false;
        bool _intensityDown = false;
        bool _angleUp = false;
        bool _angleDown = false;

        void Awake()
        {
            if (!IsValidLight(_light))
            {
                Debug.Log("`_light` is not valid.");
                throw new Exception();
            }

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

        // Set the light color based on ownership.
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            if (base.IsOwner)
                _light.color = Color.green;
            else
                _light.color = Color.red;
        }

        [Serializable]
        public class OnRegisterFn : IFn<ITuple<ItemSlot>, Fn.Tuple>
        {
            public LightControl LightControl;
            public Fn.Tuple Invoke(ITuple<ItemSlot> param)
            {
                LightControl.OnRegister(param.Item1);
                return Fn.Tuple.Unit;
            }
        }

        [Serializable]
        public class OnUnregisterFn : IFn<Fn.Tuple, Fn.Tuple>
        {
            public LightControl LightControl;
            public Fn.Tuple Invoke(Fn.Tuple _)
            {
                LightControl.OnUnregister();
                return Fn.Tuple.Unit;
            }
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

        [Serializable]
        public class ToggleFn : IFn<Fn.Tuple, Fn.Tuple>
        {
            public LightControl LightControl;

            public Fn.Tuple Invoke(Fn.Tuple _)
            {
                if (LightControl.IsServerInitialized is false)
                    return Fn.Tuple.Unit;
                if (LightControl.IsOn() is true)
                    LightControl.Off();
                else
                    LightControl.On();
                return Fn.Tuple.Unit;
            }
        }

        [Serializable]
        public class SwitchFn : IFn<ITuple<bool>, Fn.Tuple>, IFn<Fn.Tuple, Fn.Tuple>
        {
            public LightControl LightControl;
            public bool DefaultParam = false;

            public Fn.Tuple Invoke(ITuple<bool> param)
            {
                if (LightControl.IsServerInitialized is false)
                    return Fn.Tuple.Unit;
                if (param.Item1)
                    LightControl.On();
                else
                    LightControl.Off();
                return Fn.Tuple.Unit;
            }
            public Fn.Tuple Invoke(Fn.Tuple _)
            {
                if (LightControl.IsServerInitialized is false)
                    return Fn.Tuple.Unit;
                if (DefaultParam)
                    LightControl.On();
                else
                    LightControl.Off();
                return Fn.Tuple.Unit;
            }
        }

        [Server]
        public bool IsOn()
        {
            return _light.enabled;
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
        public void StartRangeUp()
        {
            _rangeUp = true;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StopRangeUp()
        {
            _rangeUp = false;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StartRangeDown()
        {
            _rangeDown = true;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StopRangeDown()
        {
            _rangeDown = false;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StartIntensityUp()
        {
            _intensityUp = true;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StopIntensityUp()
        {
            _intensityUp = false;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StartIntensityDown()
        {
            _intensityDown = true;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StopIntensityDown()
        {
            _intensityDown = false;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StartAngleUp()
        {
            _angleUp = true;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StopAngleUp()
        {
            _angleUp = false;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StartAngleDown()
        {
            _angleDown = true;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        public void StopAngleDown()
        {
            _angleDown = false;
            SyncState();
            _alarm.Arm();
        }

        [Server]
        void SyncState()
        {
            SyncStateRpc(
                _rangeUp, _rangeDown, _light.pointLightOuterRadius,
                _intensityUp, _intensityDown, _light.intensity,
                _angleUp, _angleDown, _light.pointLightOuterAngle,
                _light.enabled
            );
        }

        [ObserversRpc(BufferLast = true, ExcludeServer = true)]
        void SyncStateRpc(
            bool rangeUp, bool rangeDown, float range,
            bool intensityUp, bool intensityDown, float intensity,
            bool angleUp, bool angleDown, float angle,
            bool enabled)
        {
            SetRange(range);
            SetAngle(angle);
            SetIntensity(intensity);
            SetEnabled(enabled);

            // Update flags locally
            _rangeUp = rangeUp;
            _rangeDown = rangeDown;
            _intensityUp = intensityUp;
            _intensityDown = intensityDown;
            _angleUp = angleUp;
            _angleDown = angleDown;

            _alarm.Arm();
        }

        void OnAlarm(float _)
        {
            short rangeChange = (short)((_rangeUp ? 1 : 0) + (_rangeDown ? -1 : 0));
            short angleChange = (short)((_angleUp ? 1 : 0) + (_angleDown ? -1 : 0));
            short intensityChange = (short)((_intensityUp ? 1 : 0) + (_intensityDown ? -1 : 0));

            if (_rangeChangeRate != 0f)
                SetRange(_light.pointLightOuterRadius + rangeChange * _rangeChangeRate * (float)TimeManager.TickDelta);
            else
            {
                if (rangeChange > 0)
                    SetRange(_minMaxRange.y);
                else if (rangeChange < 0)
                    SetRange(_minMaxRange.x);
            }
            if (_angleChangeRate != 0f)
                SetAngle(_light.pointLightOuterAngle + angleChange * _angleChangeRate * (float)TimeManager.TickDelta);
            else
            {
                if (angleChange > 0)
                    SetAngle(_minMaxAngle.y);
                else if (angleChange < 0)
                    SetAngle(_minMaxAngle.x);
            }
            if (_intensityChangeRate != 0f)
                SetIntensity(_light.intensity + intensityChange * _intensityChangeRate * (float)TimeManager.TickDelta);
            else
            {
                if (intensityChange > 0)
                    SetIntensity(_minMaxIntensity.y);
                else if (intensityChange < 0)
                    SetIntensity(_minMaxIntensity.x);
            }

            RefreshVision();

            // If no more changes are detected, put the alarm to sleep.
            if (!_rangeUp && !_rangeDown && !_intensityUp && !_intensityDown && !_angleUp && !_angleDown)
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
            _light.pointLightInnerRadius = Math.Max(0f, newRange - _falloffRange);
            // RefreshVision();
        }

        void SetAngle(float newAngle)
        {
            newAngle = Math.Clamp(newAngle, _minMaxAngle.x, _minMaxAngle.y);
            _light.pointLightOuterAngle = newAngle;
            _light.pointLightInnerAngle = Math.Max(0f, newAngle - _falloffAngle);
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
                return false;
            if (light.lightType != Light2D.LightType.Point)
                return false;
            if (light.blendStyleIndex != 1)
                return false;
            return true;
        }
    }
}
