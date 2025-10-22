using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class Vision : NetworkBehaviour
{
    public struct RangeHandle
    {
        public float Range;
        public float Priority;

        public RangeHandle(float range, float priority)
        {
            Range = range;
            Priority = priority;
        }
    }

    public struct RangeModifierHandle
    {
        public float Modifier;

        public RangeModifierHandle(float modifier)
        {
            Modifier = modifier;
        }
    }

    Heap<RangeHandle, float> _ranges = Heap.MaxHeap<RangeHandle, float>();
    List<RangeModifierHandle> _modifiers = new();

    [Required]
    [ValidateInput("IsValidLight", "The vision light should be a 360 deg spot light with blend mode \"Vision\".")]
    [SerializeField]
    Light2D _visionLight;

    [SerializeField]
    float _modifier = 2f;

    [SerializeField]
    [Range(0f, 1f)]
    float _falloffDistance = 1f;

    RangeModifierHandle? _handle;

    float _range = 0f;
    public float Range
    {
        get
        {
            return _range;
        }
    }

    public UnityEvent<float> RangeChanged;

    public override void OnStartServer()
    {
        _handle = AddRangeModifier(_modifier);
    }

    public override void OnStopServer()
    {
        RemoveRangeModifier(_handle.Value);
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
        RangeChanged?.Invoke(_range);
    }

    [ObserversRpc(BufferLast = true, ExcludeServer = true)]
    void UpdateRangeRpc(float newRange)
    {
        UpdateRangeLocal(newRange);
    }

    [Server]
    public RangeHandle AddRange(float range, float priority)
    {
        var handle = new RangeHandle(range, priority);
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
            return true;
        // light.lightType = Light2D.LightType.Point;
        // light.pointLightInnerAngle = 360f;
        // light.pointLightOuterAngle = 360f;
        // light.pointLightInnerRadius = 0f;
        // light.pointLightOuterRadius = 0f;
        // light.enabled = false;
        // light.blendStyleIndex = 1;
        if (light.lightType != Light2D.LightType.Point)
            return false;
        if (light.pointLightInnerAngle != 360f)
            return false;
        if (light.pointLightOuterAngle != 360f)
            return false;
        // if (light.pointLightInnerRadius != 0f)
        //     return false;
        // if (light.pointLightOuterRadius != 0f)
        //     return false;
        if (light.enabled)
            return false;
        if (light.blendStyleIndex != 2)
            return false;
        return true;
    }
}
