namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;
    using Fn;

    public class PlayerCharacterCamera : NetworkBehaviour
    {
        [SerializeField]
        [Min(0f)]
        float _maxFrontSize = 10f;
        [SerializeField]
        [Min(0f)]
        float _maxRearSize = 6f;
        [SerializeField]
        [Min(0f)]
        float _minFrontSize = 3f;
        [SerializeField]
        [Min(0f)]
        float _minRearSize = 3f;

        [SerializeField]
        Transform _cameraTarget;

        FollowCamera _followCamera = null;

        float _size = 6f;
        public float Size
        {
            get
            {
                return _size;
            }
        }

        [SerializeField]
        Event<float> _sizeChanged;

        float _frontSize = 3f;
        public float FrontSize
        {
            get => _frontSize;
        }

        float _rearSize = 3f;
        public float RearSize
        {
            get => _rearSize;
        }

        [SerializeField]
        Event<float> _frontSizeChanged;
        [SerializeField]
        Event<float> _rearSizeChanged;


        public void AddSizeChangedListener(IFn<Fn.Tuple<float>, Fn.Tuple> listener)
        {
            _sizeChanged._listeners.Add(listener);
        }

        public void RemoveSizeChangedListener(IFn<Fn.Tuple<float>, Fn.Tuple> listener)
        {
            _sizeChanged._listeners.Remove(listener);
        }

        public void AddFrontSizeChangedListener(IFn<Fn.Tuple<float>, Fn.Tuple> listener)
        {
            _frontSizeChanged._listeners.Add(listener);
        }

        public void RemoveFrontSizeChangedListener(IFn<Fn.Tuple<float>, Fn.Tuple> listener)
        {
            _frontSizeChanged._listeners.Remove(listener);
        }

        public void AddRearSizeChangedListener(IFn<Fn.Tuple<float>, Fn.Tuple> listener)
        {
            _rearSizeChanged._listeners.Add(listener);
        }

        public void RemoveRearSizeChangedListener(IFn<Fn.Tuple<float>, Fn.Tuple> listener)
        {
            _rearSizeChanged._listeners.Remove(listener);
        }

        void Awake()
        {
            if (_cameraTarget == null)
            {
                Debug.Log("`_cameraTarget` was not set.");
                throw new Exception();
            }
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

        public void OnRangeChanged(float newRange)
        {
            Debug.Log($"RANGECHANGE {newRange}");
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
                _sizeChanged?.Invoke(Size);
            if (oldFrontSize != FrontSize)
                _frontSizeChanged?.Invoke(FrontSize);
            if (oldRearSize != RearSize)
                _rearSizeChanged?.Invoke(RearSize);
            Debug.Log($"{Size}, {FrontSize}, {RearSize}");
        }
    }
}
