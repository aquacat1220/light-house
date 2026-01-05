namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;
    using Fn;

    public class PlayerCharacterCamera : NetworkBehaviour
    {
        [SerializeField]
        Vision _vision;

        [SerializeField]
        [Min(0f)]
        float _minFrontSize = 3f;
        [SerializeField]
        [Min(0f)]
        float _maxFrontSize = 10f;
        [SerializeField]
        [Min(0f)]
        float _minRearSize = 3f;
        [SerializeField]
        [Min(0f)]
        float _maxRearSize = 6f;

        [SerializeField]
        Transform _cameraTarget;

        FollowCamera _followCamera = null;

        // Initial values don't matter, because they will be overwritten as soon as the component gets initialized in `Awake()`.
        float _size;
        public float Size
        {
            get
            {
                return _size;
            }
        }

        public event Action<float> SizeChanged;

        float _frontSize;
        public float FrontSize
        {
            get => _frontSize;
        }

        float _rearSize;
        public float RearSize
        {
            get => _rearSize;
        }

        public event Action<float> FrontSizeChanged;
        public event Action<float> RearSizeChanged;

        void Awake()
        {
            if (_cameraTarget == null)
            {
                Debug.Log("`_cameraTarget` was not set.");
                throw new Exception();
            }

            if (_vision == null)
            {
                Debug.Log("`_vision` wasn't set.");
                throw new Exception();
            }
            _vision.RangeChanged += OnRangeChanged;
            OnRangeChanged(_vision.Range);
        }

        void OnDestroy()
        {
            _vision.RangeChanged -= OnRangeChanged;
        }

        void OnEnable()
        {
            if (base.IsClientInitialized && base.IsOwner)
                AttachCamera();
        }

        void OnDisable()
        {
            DetachCamera();
        }

        public override void OnStartClient()
        {
            if (base.isActiveAndEnabled && base.IsOwner)
                AttachCamera();
        }

        public override void OnStopClient()
        {
            DetachCamera();
        }

        void AttachCamera()
        {
            _followCamera = FollowCamera.Singleton;
            if (_followCamera == null)
            {
                Debug.Log("`FollowCamera.Singleton` was null, implying we do not have a follow camera in this scene.");
                throw new Exception();
            }
            if (_followCamera.Target != null)
            {
                Debug.Log($"Attempted to hijack a followcamera that was targeting {_followCamera.Target}.");
                throw new Exception();
            }
            _followCamera.Target = _cameraTarget;
            _followCamera.Camera.orthographicSize = Size;
        }

        void DetachCamera()
        {
            if (_followCamera?.Target != null)
                _followCamera.Target = null;
            _followCamera = null;
        }

        void OnRangeChanged(float newRange)
        {
            float frontSize = Mathf.Clamp(newRange, _minFrontSize, _maxFrontSize);
            float rearSize = Mathf.Clamp(newRange, _minRearSize, _maxRearSize);

            var oldSize = Size;
            var oldFrontSize = FrontSize;
            var oldRearSize = RearSize;

            _size = (frontSize + rearSize) / 2;
            _frontSize = frontSize;
            _rearSize = rearSize;

            _cameraTarget.localPosition = ((FrontSize - RearSize) / 2) * Vector2.up;

            if (_followCamera != null)
                _followCamera.Camera.orthographicSize = Size;

            if (oldSize != Size)
                SizeChanged?.Invoke(Size);
            if (oldFrontSize != FrontSize)
                FrontSizeChanged?.Invoke(FrontSize);
            if (oldRearSize != RearSize)
                RearSizeChanged?.Invoke(RearSize);
        }
    }
}
