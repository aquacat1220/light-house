namespace LightHouse
{
    using System;
    using FishNet.Object;
    using NaughtyAttributes;
    using UnityEngine;
    using Fn;

    public class PlayerCharacterCamera : NetworkBehaviour
    {
        [SerializeField]
        [Min(1f)]
        float _minimumCameraSize = 6f;

        FollowCamera _followCamera = null;
        float _range = 1f;

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
            _followCamera.Target = transform;
            _followCamera.Camera.orthographicSize = Math.Max(_range, _minimumCameraSize);
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
            _range = newRange;
            if (_followCamera != null)
            {
                _followCamera.Camera.orthographicSize = Math.Max(_range, _minimumCameraSize);
            }
        }
    }
}
