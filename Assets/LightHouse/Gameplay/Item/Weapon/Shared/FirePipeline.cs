namespace LightHouse
{
    using System;
    using FishNet.Example.ColliderRollbacks;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class FirePipeline : MonoBehaviour
    {
        [SerializeField]
        ItemInput _itemInput;
        [SerializeField]
        Magazine _magazine;
        [SerializeField]
        RandomSpread _randomSpread;
        [SerializeField]
        ProjectileSpawner _projectileSpawner;

        public enum FireModeEnum
        {
            Single,
            Auto
        }

        [SerializeField]
        FireModeEnum _fireMode;
        public FireModeEnum FireMode
        {
            get => _fireMode;
            set
            {
                _fireMode = value;
                if (_fireMode == FireModeEnum.Single)
                {
                    _singleState.Enable();
                    _autoState.Disable();
                }
                else if (_fireMode == FireModeEnum.Auto)
                {
                    _singleState.Disable();
                    _autoState.Enable();
                }
                else
                {
                    Debug.Log("Unknown variant of `FireModeEnum` encountered.");
                    throw new Exception();
                }
            }
        }

        [SerializeField]
        float _singleFireRpm = 450f;
        [SerializeField]
        float _autoFireRpm = 700f;

        InputState<bool> _inputState = new();

        InputState<bool> _singleState = new();
        InputState<bool> _autoState = new();

        void Awake()
        {
            if (_itemInput == null)
            {
                Debug.Log("`_itemInput` was not set.");
                throw new Exception();
            }
            _itemInput.Primary += OnPrimary;
            _itemInput.Action3 += OnAction3;

            if (_magazine == null)
            {
                Debug.Log("`_magazine` was not set.");
                throw new Exception();
            }
            _magazine.Fire += OnFire;

            if (_randomSpread == null)
            {
                Debug.Log("`_randomSpread` was not set.");
                throw new Exception();
            }

            if (_projectileSpawner == null)
            {
                Debug.Log("`_projectileSpawner` was not set.");
                throw new Exception();
            }

            if (_singleFireRpm != 0f)
                _singleState.Change += new LimitPulse(TryFire, delay: 60f / _singleFireRpm, bufferPulse: true, timer: null).Invoke;
            _singleState.Parent = _inputState;

            if (_autoFireRpm != 0f)
                _autoState.Change += new RepeatPulse(TryFire, repeatDelay: 60f / _autoFireRpm, timer: null).Invoke;
            _autoState.Parent = _inputState;

            FireMode = _fireMode;
            _inputState.Enable();
        }

        void OnDestroy()
        {
            _itemInput.Primary -= OnPrimary;
            _itemInput.Action3 -= OnAction3;
            _inputState.Disable();
            // No need to cleanup `_singleState.Change` and `_autoState.Change`.
        }

        void OnPrimary(bool newState)
        {
            var result = _inputState.RootChangeState(newState);
            Assert.IsTrue(result);
        }

        void OnAction3(bool newState)
        {
            _magazine.StartReload();
        }

        void TryFire(bool isUp)
        {
            if (!isUp)
                return;
            _magazine.TryFire();
        }

        void OnFire()
        {
            _randomSpread.ApplySpread(addHeat: true, reuseAimError: false);
            _projectileSpawner.SpawnProjectile();
        }
    }
}