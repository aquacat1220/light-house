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

        // Weapon variance (inaccuracy due to weapon design) in degrees.
        [SerializeField]
        [Min(0f)]
        float _weaponVariance = 1f;
        // A curve that maps heat to weapon variance modifier (multiplied to weapon variance).
        [SerializeField]
        [CurveRange(0f, 0f, 1f, 1f)]
        AnimationCurve _weaponVarianceModifierCurve;

        // Aiming variance (inaccuracy from character's imprecise aiming) in degrees.
        [SerializeField]
        [Min(0f)]
        float _aimVariance = 1f;
        // Modifier to be multiplied to aim variance.
        [SerializeField]
        [Min(0f)]
        float _aimVarianceModifier = 1f;

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
            (var aimGaussian, var weaponGaussian) = new SplitMix64((ulong)_predictedCounter).NextGaussian();

            float aimError = _lastAimError;
            if (!reuseAimError)
            {
                float aimVariance = _aimVariance * _aimVarianceModifier;
                aimError = (float)aimGaussian * aimVariance;
                _lastAimError = aimError;
                Debug.Log($"{Time.time}: Recalculated aim error to {aimError}.");
            }

            float weaponVariance = _weaponVariance * _weaponVarianceModifierCurve.Evaluate(_heat);
            float weaponError = (float)weaponGaussian * weaponVariance;

            float error = aimError + weaponError;
            Debug.Log($"{Time.time}: Set spawn point with aim error {aimError}, weapon error {weaponError}.");
            _spawnPoint.localEulerAngles = new Vector3(0f, 0f, error);

            if (addHeat)
            {
                _heat = Math.Clamp(_heat + _heatPerFire, 0f, 1f);
                Debug.Log($"{Time.time}: Heated up to {_heat}.");
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
            Debug.Log($"{Time.time}: Starting cooling.");
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
            Debug.Log($"{Time.time}: Cooled down to {_heat}.");
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