namespace LightHouse
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FishNet.Object;
    using UnityEngine;
    using Fn;
    using UnityEngine.Rendering.Universal;

    public class Vision : NetworkBehaviour
    {
        public class RangeHandle
        {
            public float Range;

            public RangeHandle(float range)
            {
                Range = range;
            }
        }

        public class RangeModifierHandle
        {
            public float Modifier;

            public RangeModifierHandle(float modifier)
            {
                Modifier = modifier;
            }
        }

        Heap<RangeHandle, float> _ranges = Heap.MaxHeap<RangeHandle, float>();
        List<RangeModifierHandle> _modifiers = new();

        [SerializeField]
        Light2D _visionLight;

        [SerializeField]
        float _modifier = 2f;

        [SerializeField]
        [Range(0f, 1f)]
        float _falloffDistance = 1f;

        RangeModifierHandle _handle;

        float _range = 0f;
        public float Range
        {
            get
            {
                return _range;
            }
        }

        [SerializeField]
        Event<float> _rangeChanged;

        void Awake()
        {
            if (!IsValidLight(_visionLight))
            {
                Debug.Log("`_visionLight` is not valid.");
                throw new Exception();
            }
        }

        public override void OnStartServer()
        {
            _handle = AddRangeModifier(_modifier);
        }

        public override void OnStopServer()
        {
            if (_handle != null)
                RemoveRangeModifier(_handle);
        }

        public override void OnStartClient()
        {
            if (base.IsOwner)
                _visionLight.enabled = true;
        }

        public override void OnStopClient()
        {
            _visionLight.enabled = false;
        }

        void UpdateRange()
        {
            float newRange = 0f;
            if (_ranges.Peek() is (var handle, _))
            {
                newRange = handle.Range;
            }
            newRange += _modifiers.Sum((handle) => handle.Modifier);
            newRange = Math.Max(newRange, 0f);
            if (_range != newRange)
            {
                UpdateRangeLocal(newRange);
                UpdateRangeRpc(newRange);
            }
        }

        void UpdateRangeLocal(float newRange)
        {
            _range = newRange;
            _visionLight.pointLightInnerRadius = Math.Max(_range - _falloffDistance, 0f);
            _visionLight.pointLightOuterRadius = _range;
            _rangeChanged?.Invoke(_range);
        }

        [ObserversRpc(BufferLast = true, ExcludeServer = true)]
        void UpdateRangeRpc(float newRange)
        {
            UpdateRangeLocal(newRange);
        }

        [Server]
        public RangeHandle AddRange(float range, float priority)
        {
            var handle = new RangeHandle(range);
            _ranges.Push(handle, priority);
            UpdateRange();
            return handle;
        }

        [Server]
        public bool RemoveRange(RangeHandle handle)
        {
            var success = _ranges.Remove(handle) != null;
            UpdateRange();
            return success;
        }

        [Server]
        public RangeModifierHandle AddRangeModifier(float modifier)
        {
            var handle = new RangeModifierHandle(modifier);
            _modifiers.Add(handle);
            UpdateRange();
            return handle;
        }

        [Server]
        public bool RemoveRangeModifier(RangeModifierHandle handle)
        {
            var success = _modifiers.Remove(handle);
            UpdateRange();
            return success;
        }

        bool IsValidLight(Light2D light)
        {
            if (light == null)
                return false;
            if (light.lightType != Light2D.LightType.Point)
                return false;
            if (light.pointLightInnerAngle != 360f)
                return false;
            if (light.pointLightOuterAngle != 360f)
                return false;
            if (light.pointLightInnerRadius != 0f)
                return false;
            if (light.pointLightOuterRadius != 0f)
                return false;
            if (light.enabled)
                return false;
            if (light.blendStyleIndex != 2)
                return false;
            return true;
        }
    }
}
