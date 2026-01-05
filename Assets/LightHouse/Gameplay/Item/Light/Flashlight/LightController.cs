namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using FishNet.Connection;
    using UnityEngine.Assertions;

    public class LightController : NetworkBehaviour
    {
        [SerializeField]
        Item _item;
        [SerializeField]
        ItemInput _itemInput;

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
            if (_item == null)
            {
                Debug.Log("`_item` was not set.");
                throw new Exception();
            }
            _item.Register += OnRegister;
            _item.Unregister += OnUnregister;

            Assert.IsNotNull(_itemInput);

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

        void OnDestroy()
        {
            _item.Register -= OnRegister;
            _item.Unregister -= OnUnregister;
        }

        public override void OnStartServer()
        {
            _itemInput.Primary += OnPrimary;
            _itemInput.Action1 += OnAction1;
            _itemInput.Action2 += OnAction2;
        }

        public override void OnStopServer()
        {
            _itemInput.Primary -= OnPrimary;
            _itemInput.Action1 -= OnAction1;
            _itemInput.Action2 -= OnAction2;
        }

        // Set the light color based on ownership.
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            if (base.IsOwner)
                _light.color = Color.green;
            else
                _light.color = Color.red;
        }

        void OnRegister(ItemSlot itemSlot)
        {
            // Controlling vision is only possible on the server.
            if (base.IsServerInitialized)
                _vision = itemSlot.User.GetComponent<Vision>();
            RefreshVision();
        }

        void OnUnregister()
        {
            if (_vision != null && _handle is Vision.RangeHandle handle)
            {
                _vision.RemoveRange(handle);
                _handle = null;
            }
            _vision = null;
            _rangeUp = false;
            _rangeDown = false;
            _intensityUp = false;
            _intensityDown = false;
            _angleUp = false;
            _angleDown = false;
            // We don't want the alarm callback to be triggered after the light is unregistered.
            _alarm?.Remove();
            _alarm = null;
        }

        void Update()
        {
            _alarm?.Start();
        }

        void OnPrimary(bool newState)
        {
            if (!newState)
                return;
            if (IsOn())
                Off();
            else
                On();
        }

        void OnAction1(bool newState)
        {
            if (newState)
            {
                StartRangeDown();
                StartAngleUp();
            }
            else
            {
                StopRangeDown();
                StopAngleUp();
            }
        }

        void OnAction2(bool newState)
        {
            if (newState)
            {
                StartRangeUp();
                StartAngleDown();
            }
            else
            {
                StopRangeUp();
                StopAngleDown();
            }
        }

        [Server]
        bool IsOn()
        {
            return _light.enabled;
        }

        [Server]
        void On()
        {
            SetEnabled(true);
            SyncState();
            RefreshVision();
        }

        [Server]
        void Off()
        {
            SetEnabled(false);
            SyncState();
            RefreshVision();
        }

        [Server]
        void StartRangeUp()
        {
            _rangeUp = true;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StopRangeUp()
        {
            _rangeUp = false;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StartRangeDown()
        {
            _rangeDown = true;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StopRangeDown()
        {
            _rangeDown = false;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StartIntensityUp()
        {
            _intensityUp = true;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StopIntensityUp()
        {
            _intensityUp = false;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StartIntensityDown()
        {
            _intensityDown = true;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StopIntensityDown()
        {
            _intensityDown = false;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StartAngleUp()
        {
            _angleUp = true;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StopAngleUp()
        {
            _angleUp = false;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StartAngleDown()
        {
            _angleDown = true;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        [Server]
        void StopAngleDown()
        {
            _angleDown = false;
            SyncState();
            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
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

            if (_alarm == null)
                _alarm = TimerManager.Singleton.AddAlarm(
                    cooldown: 0f,
                    callback: OnAlarm,
                    startImmediately: true,
                    armImmediately: true,
                    autoRestart: false,
                    autoRearm: true,
                    initialCooldown: 0f,
                    destroyAfterTriggered: false
                );
        }

        void OnAlarm(float deltaTime)
        {
            // This callback will be triggered every frame, as long as the alarm exists.
            short rangeChange = (short)((_rangeUp ? 1 : 0) + (_rangeDown ? -1 : 0));
            short angleChange = (short)((_angleUp ? 1 : 0) + (_angleDown ? -1 : 0));
            short intensityChange = (short)((_intensityUp ? 1 : 0) + (_intensityDown ? -1 : 0));

            if (_rangeChangeRate != 0f)
                SetRange(_light.pointLightOuterRadius + rangeChange * _rangeChangeRate * deltaTime);
            else
            {
                if (rangeChange > 0)
                    SetRange(_minMaxRange.y);
                else if (rangeChange < 0)
                    SetRange(_minMaxRange.x);
            }
            if (_angleChangeRate != 0f)
                SetAngle(_light.pointLightOuterAngle + angleChange * _angleChangeRate * deltaTime);
            else
            {
                if (angleChange > 0)
                    SetAngle(_minMaxAngle.y);
                else if (angleChange < 0)
                    SetAngle(_minMaxAngle.x);
            }
            if (_intensityChangeRate != 0f)
                SetIntensity(_light.intensity + intensityChange * _intensityChangeRate * deltaTime);
            else
            {
                if (intensityChange > 0)
                    SetIntensity(_minMaxIntensity.y);
                else if (intensityChange < 0)
                    SetIntensity(_minMaxIntensity.x);
            }

            RefreshVision();

            // If no more changes are detected, remove the alarm.
            if (!_rangeUp && !_rangeDown && !_intensityUp && !_intensityDown && !_angleUp && !_angleDown)
            {
                _alarm?.Remove();
                _alarm = null;
            }
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
