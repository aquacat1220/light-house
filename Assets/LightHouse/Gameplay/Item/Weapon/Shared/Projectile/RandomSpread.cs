namespace LightHouse
{
    using System;
    using FishNet.Object;
    using LightHouse.Fn;
    using NaughtyAttributes;
    using UnityEngine;

    public class RandomSpread : NetworkBehaviour
    {
        [SerializeField]
        [Required]
        [ValidateInput("CheckSpawn", "`_spawnPoint` should have a zeroed local transform.")]
        Transform _spawnPoint;

        // Weapon spread in degrees. This is the maximum degree in which the projectile can spread, with respect to the aimed direction.
        [SerializeField]
        [Min(0f)]
        float _weaponSpread = 1f;
        // A curve that maps heat to weapon spread modifier (multiplied to weapon spread).
        [SerializeField]
        [CurveRange(0f, 0f, 1f, 1f)]
        AnimationCurve _weaponSpreadModifierCurve;

        // Aiming spread (inaccuracy from character's imprecise aiming) in degrees.
        // Weapon spread in degrees. This is the maximum degree in which the aim direction can spread.
        [SerializeField]
        [Min(0f)]
        float _aimSpread = 1f;
        // Modifier to be multiplied to aim spread.
        [SerializeField]
        [Min(0f)]
        float _aimSpreadModifier = 1f;

        // Accumulated heat per fire.
        [SerializeField]
        [Min(0f)]
        float _heatPerFire = 0.25f;
        // Delay (in seconds) before a heated weapon starts cooling down.
        [SerializeField]
        [Min(0f)]
        float _coolDelay = 1f;
        // Amount of heat cooled per second.
        [SerializeField]
        float _coolPerSecond = 0.25f;

        float _heat = 0f;
        float _lastAimError = 0f;

        int _predictedCounter = 0;

        Alarm _delayAlarm;
        Alarm _coolAlarm;

        public float WeaponSpread
        {
            get => _weaponSpread * _weaponSpreadModifierCurve.Evaluate(_heat);
        }

        public float AimSpread
        {
            get => _aimSpread * _aimSpreadModifier;
        }

        void Awake()
        {
            if (_spawnPoint == null)
            {
                Debug.Log("`_spawnPoint` was not set.");
                throw new Exception();
            }
            if (_spawnPoint.localPosition != Vector3.zero || _spawnPoint.localRotation != Quaternion.identity || _spawnPoint.localScale != Vector3.one)
            {
                Debug.Log("`_spawnPoint` should have a zeroed local transform.");
                throw new Exception();
            }
        }

        public override void OnStartNetwork()
        {
            _delayAlarm = TimerManager.Singleton.AddAlarm(
                cooldown: _coolDelay,
                callback: StartCooling,
                startImmediately: false,
                armImmediately: true,
                autoRestart: false,
                autoRearm: true,
                initialCooldown: _coolDelay,
                destroyAfterTriggered: false
            );
            OnPredictedCounterChange(_predictedCounter);
        }

        public override void OnStopNetwork()
        {
            _delayAlarm.Remove();
            _coolAlarm?.Remove();
        }

        void Update()
        {
            // Arm alarm every frame, so the alarm will trigger on every frame as long as it is started.
            _coolAlarm?.Arm();
        }

        [Serializable]
        public class ApplySpreadFn : IFn<ITuple<bool, bool>, Fn.Tuple>, IFn<Fn.Tuple, Fn.Tuple>
        {
            public RandomSpread RandomSpread;
            public bool AddHeat = true;
            public bool ReuseAimError = false;

            public Fn.Tuple Invoke(ITuple<bool, bool> param)
            {
                RandomSpread?.ApplySpread(param.Item1, param.Item2);
                return Fn.Tuple.Unit;
            }

            public Fn.Tuple Invoke(Fn.Tuple param)
            {
                RandomSpread?.ApplySpread(AddHeat, ReuseAimError);
                return Fn.Tuple.Unit;
            }
        }

        public void ApplySpread(bool addHeat = true, bool reuseAimError = false)
        {
            var rng = new SplitMix64((ulong)_predictedCounter);
            var aimBates = 2 * rng.NextBates6() - 1;
            var weaponBates = 2 * rng.NextBates6() - 1;

            float aimError = _lastAimError;
            if (!reuseAimError)
            {
                float aimSpread = _aimSpread * _aimSpreadModifier;
                aimError = (float)aimBates * aimSpread;
                _lastAimError = aimError;
            }

            float weaponSpread = _weaponSpread * _weaponSpreadModifierCurve.Evaluate(_heat);
            float weaponError = (float)weaponBates * weaponSpread;

            float error = aimError + weaponError;
            _spawnPoint.localEulerAngles = new Vector3(0f, 0f, error);

            if (addHeat)
            {
                _heat = Math.Clamp(_heat + _heatPerFire, 0f, 1f);
            }
            _coolAlarm?.Remove();
            _coolAlarm = null;
            _delayAlarm?.Reset(_coolDelay);
            _delayAlarm?.Start();
        }

        [Serializable]
        public class OnPredictedCounterChangeFn : IFn<ITuple<int>, Fn.Tuple>
        {
            public RandomSpread RandomSpread;

            public Fn.Tuple Invoke(ITuple<int> param)
            {
                RandomSpread?.OnPredictedCounterChange(param.Item1);
                return Fn.Tuple.Unit;
            }
        }

        public void OnPredictedCounterChange(int newPredictedCounter)
        {
            _predictedCounter = newPredictedCounter;
        }

        void StartCooling(float _)
        {
            if (_coolAlarm != null)
            {
                Debug.Log("We already have a cool alarm, which shouldn't be possible.");
                throw new Exception();
            }
            _coolAlarm = TimerManager.Singleton.AddAlarm(
                cooldown: 0f,
                callback: Cool,
                startImmediately: true,
                armImmediately: true,
                autoRestart: true,
                autoRearm: false,
                initialCooldown: 0f,
                destroyAfterTriggered: false
            );
        }

        void Cool(float deltaTime)
        {
            _heat = Math.Clamp(_heat - deltaTime * _coolPerSecond, 0f, 1f);
            if (_heat == 0f)
            {
                _coolAlarm.Remove();
                _coolAlarm = null;
            }
        }

        bool CheckSpawn(Transform spawnPoint)
        {
            if (_spawnPoint == null)
                return true;
            if (_spawnPoint.localPosition != Vector3.zero || _spawnPoint.localRotation != Quaternion.identity || _spawnPoint.localScale != Vector3.one)
                return false;
            return true;
        }
    }
}