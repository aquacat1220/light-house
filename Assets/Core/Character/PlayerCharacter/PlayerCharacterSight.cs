using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerCharacterSight : NetworkBehaviour
{
    [SerializeField]
    float _baseSight = 2f;

    [SerializeField]
    SpriteMask _sightMask;

    HashSet<Light2D> _lights = new();

    float _currMaxRange = 0f;
    float CurrMaxRange
    {
        get
        {
            return _currMaxRange;
        }
        set
        {
            _currMaxRange = value;
            _sightMask.transform.localScale = (_baseSight + _currMaxRange) * Vector3.one;
        }
    }

    void Awake()
    {
        if (_sightMask == null)
        {
            Debug.Log("`_sightMask` wasn't set.");
            throw new Exception();
        }
        // Start with zeroed `_sightMask`, and set the value in `OnStartClient()`.
        _sightMask.transform.localScale = Vector3.zero;
    }

    public override void OnStartClient()
    {
        // If we are not the owner, sight masks can stay zeroed.
        if (!base.IsOwner)
            return;
        _sightMask.transform.localScale = (_baseSight + CurrMaxRange) * Vector3.one;
    }

    public override void OnStopClient()
    {
        _sightMask.transform.localScale = Vector3.zero;
    }

    void OnEnable()
    {
        // If we are not the owner, or we are not client-inited yet, sight masks can stay zeroed.
        if (!base.IsClientInitialized || !base.IsOwner)
            return;
        _sightMask.transform.localScale = (_baseSight + CurrMaxRange) * Vector3.one;
    }

    void OnDisable()
    {
        _sightMask.transform.localScale = Vector3.zero;
    }

    [Client(RequireOwnership = true)]
    public void RegisterLight(Light2D light)
    {
        if (light.lightType != Light2D.LightType.Point)
        {
            Debug.Log("`PlayerCharacterSight` currently allows only point lights.");
            throw new Exception();
        }
        if (_lights.Add(light))
            CurrMaxRange = Mathf.Max(CurrMaxRange, light.pointLightOuterRadius);
    }

    [Client(RequireOwnership = true)]
    public void UnregisterLight(Light2D light)
    {
        if (_lights.Remove(light))
            CurrMaxRange = _lights.Count() > 0 ? _lights.Max(light => light.pointLightOuterRadius) : 0f;
    }
}
