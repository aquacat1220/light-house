using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using NaughtyAttributes;
using UnityEngine;
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
    float _falloffDistance = 1f;

    public override void OnStartServer()
    {
        AddRangeModifier(_modifier);
    }

    public float Range
    {
        get
        {
            float range = 0f;
            if (_ranges.Peek() is (var handle, _))
            {
                range = handle.Range;
            }
            range += _modifiers.Sum((handle) => handle.Modifier);
            range = Math.Max(range, 0f);
            _visionLight.pointLightInnerRadius = Math.Max(range - _falloffDistance, 0f);
            _visionLight.pointLightOuterRadius = range;
            return range;
        }
    }

    [Server]
    public RangeHandle AddRange(float range, float priority)
    {
        var handle = AddRangeLocal(range, priority);
        AddRangeLocalRpc(range, priority);
        return handle;
    }

    [Server]
    public bool RemoveRange(RangeHandle handle)
    {
        var success = RemoveRangeLocal(handle);
        RemoveRangeRpc(handle);
        return success;
    }

    [Server]
    public RangeModifierHandle AddRangeModifier(float modifier)
    {
        var handle = AddRangeModifierLocal(modifier);
        AddRangeModifierRpc(modifier);
        return handle;
    }

    [Server]
    public bool RemoveRangeModifier(RangeModifierHandle handle)
    {
        var success = RemoveRangeModifierLocal(handle);
        RemoveRangeModifierRpc(handle);
        return success;
    }

    [ObserversRpc(ExcludeServer = true)]
    void AddRangeLocalRpc(float range, float priority)
    {
        AddRangeLocal(range, priority);
    }

    [ObserversRpc(ExcludeServer = true)]
    void RemoveRangeRpc(RangeHandle handle)
    {
        RemoveRangeLocal(handle);
    }

    [ObserversRpc(ExcludeServer = true)]
    void AddRangeModifierRpc(float modifier)
    {
        AddRangeModifierLocal(modifier);
    }

    [ObserversRpc(ExcludeServer = true)]
    void RemoveRangeModifierRpc(RangeModifierHandle handle)
    {
        RemoveRangeModifierLocal(handle);
    }

    RangeHandle AddRangeLocal(float range, float priority)
    {
        var handle = new RangeHandle(range, priority);
        _ranges.Push(handle, priority);
        _ = Range;
        return handle;
    }

    bool RemoveRangeLocal(RangeHandle handle)
    {
        var success = _ranges.Remove(handle) != null;
        _ = Range;
        return success;
    }

    RangeModifierHandle AddRangeModifierLocal(float modifier)
    {
        var handle = new RangeModifierHandle(modifier);
        _modifiers.Add(handle);
        _ = Range;
        return handle;
    }

    bool RemoveRangeModifierLocal(RangeModifierHandle handle)
    {
        var success = _modifiers.Remove(handle);
        _ = Range;
        return success;
    }

    bool IsValidLight(Light2D light)
    {
        if (light == null)
            return true;
        if (light.lightType != Light2D.LightType.Point)
            return false;
        if (light.pointLightInnerAngle != 360f)
            return false;
        if (light.pointLightOuterAngle != 360f)
            return false;
        if (light.blendStyleIndex != 1)
            return false;
        return true;
    }
}
