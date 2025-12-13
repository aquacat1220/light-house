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
        float _maxFrontRange = 10f;
        [SerializeField]
        [Min(0f)]
        float _maxRearRange = 6f;
        [SerializeField]
        [Min(0f)]
        float _minFrontRange = 3f;
        [SerializeField]
        [Min(0f)]
        float _minRearRange = 3f;

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

        public void AddSizeChangedListener(IFn<Fn.Tuple<float>, Fn.Tuple> listener)
        {
            _sizeChanged._listeners.Add(listener);
        }

        public void RemoveSizeChangedListener(IFn<Fn.Tuple<float>, Fn.Tuple> listener)
        {
            _sizeChanged._listeners.Remove(listener);
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

        [Serializable]
        public class OnRangeChangedFn : IFn<ITuple<float>, Fn.Tuple>
        {
            public PlayerCharacterCamera PlayerCharacterCamera;
            public Fn.Tuple Invoke(ITuple<float> param)
            {
                PlayerCharacterCamera?.OnRangeChanged(param.Item1);
                return Fn.Tuple.Unit;
            }
        }

        public void OnRangeChanged(float newRange)
        {
            float frontRange = Mathf.Clamp(newRange, _minFrontRange, _maxFrontRange);
            float rearRange = Mathf.Clamp(newRange, _minRearRange, _maxRearRange);
            var oldSize = Size;
            _size = (frontRange + rearRange) / 2;
            _cameraTarget.localPosition = ((frontRange - rearRange) / 2) * Vector2.up;

            if (_followCamera != null)
                _followCamera.Camera.orthographicSize = Size;

            if (oldSize != Size)
                _sizeChanged?.Invoke(Size);
        }
    }
}
