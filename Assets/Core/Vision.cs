using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Vision : NetworkBehaviour
{
    [SerializeField]
    float _baseVision = 2f;

    [SerializeField]
    SpriteMask _visibleMask;

    float _currVision = 0f;

    HashSet<Light2D> _lights = new();

    void Awake()
    {
        if (_visibleMask == null)
        {
            Debug.Log("`_visibleMask` wasn't set.");
            throw new Exception();
        }
        RecalculateVision();
    }

    void OnEnable()
    {
        RecalculateVision();
    }

    void OnDisable()
    {
        RecalculateVision();
    }

    public override void OnStartClient()
    {
        RecalculateVision();
    }

    public override void OnStopClient()
    {
        RecalculateVision();
    }

    [Client(RequireOwnership = true)]
    public void RegisterLight(Light2D light)
    {
        if (light.lightType != Light2D.LightType.Point)
        {
            Debug.Log("`Vision` currently allows only point lights.");
            throw new Exception();
        }
        if (_lights.Add(light))
            RecalculateVision();
    }

    [Client(RequireOwnership = true)]
    public void UnregisterLight(Light2D light)
    {
        if (_lights.Remove(light))
            RecalculateVision();
    }

    void RecalculateVision()
    {
        float maxLightRange = 0f;
        foreach (var light in _lights)
        {
            if (light.pointLightOuterRadius > maxLightRange)
                maxLightRange = light.pointLightOuterRadius;
        }
        _currVision = _baseVision + maxLightRange;
        // Vision can't be negative.
        _currVision = Mathf.Max(0f, _currVision);

        // Vision should only be activated when the component is enabled, client-inited, and is locally-owned.
        if (!base.enabled || !base.IsClientInitialized || !base.IsOwner)
            _visibleMask.transform.localScale = Vector3.zero;
        else
            _visibleMask.transform.localScale = _currVision * Vector3.one;
    }
}
