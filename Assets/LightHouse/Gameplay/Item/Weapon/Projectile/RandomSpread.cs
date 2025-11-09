using System;
using FishNet.Object;
using LightHouse;
using NaughtyAttributes;
using UnityEngine;

public class RandomSpread : NetworkBehaviour
{
    [SerializeField]
    [Required]
    [ValidateInput("CheckSpawn", "`_spawnPoint` should have a zeroed local transform.")]
    Transform _spawnPoint;

    // A curve that maps heat to normalized variance, representing the proportion of maximum variance to be applied.
    [SerializeField]
    [CurveRange(0f, 0f, 1f, 1f)]
    AnimationCurve _varianceCurve;
    // Max variance in degrees.
    [SerializeField]
    [Min(0f)]
    float _maxVariance = 1f;
    // A curve that maps heat to normalized drift, representing the proportion of maximum drift to be applied.
    [SerializeField]
    [CurveRange(0f, 0f, 1f, 1f)]
    AnimationCurve _driftCurve;
    // Max drift in degrees.
    [SerializeField]
    [Min(0f)]
    float _maxDrift = 1f;

    [SerializeField]
    [Min(0f)]
    float _heatPerFire = 0.25f;
    [SerializeField]
    [Min(0f)]
    float _coolDelay = 1f;
    [SerializeField]
    float _coolPerSecond = 0.25f;

    float _heat = 0f;

    int _predictedCounter = 0;
    float _lastGaussian = 0f;

    Alarm _delayAlarm;
    Alarm _coolAlarm;

    void Awake()
    {
        if (_spawnPoint == null)
        {
            Debug.Log("`_spawnPoint` was not set.");
            throw new Exception();
        }
        if (_spawnPoint.localPosition != Vector3.zero || _spawnPoint.localRotation != Quaternion.identity || _spawnPoint.localScale != Vector3.one)
        {
            Debug.Log("`_spawnPoint` should have a zeroed local transform.");
            throw new Exception();
        }
    }

    public override void OnStartNetwork()
    {
        _delayAlarm = TimerManager.Singleton.AddAlarm(
            cooldown: _coolDelay,
            callback: StartCooling,
            startImmediately: false,
            armImmediately: true,
            autoRestart: false,
            autoRearm: true,
            initialCooldown: _coolDelay,
            destroyAfterTriggered: false
        );
        OnPredictedCounterChange(_predictedCounter);
    }

    public override void OnStopNetwork()
    {
        _delayAlarm.Remove();
        _coolAlarm?.Remove();
    }

    void Update()
    {
        // Arm alarm every frame, so the alarm will trigger on every frame as long as it is started.
        _coolAlarm?.Arm();
    }

    public void OnPredictedCounterChange(int newPredictedCounter)
    {
        var delta = newPredictedCounter - _predictedCounter;
        if (delta <= 0)
        {
            // If delta is negative, this means our latest prediction was rejected.
            // But we don't do any correction here:
            // - Downstream components like projectile spawner and projectile transform will do the correction for us.
            // - And correction at this phase is too complicated.
        }
        else
        {
            _heat = Math.Clamp(_heat + delta * _heatPerFire, 0f, 1f);
            // Debug.Log($"{Time.time}: Heat at {_heat}.");
            // Debug.Log($"{Time.time}: Starting cooling delay.");
            // Stop ongoing cooling alarm if it exists.
            _coolAlarm?.Remove();
            _coolAlarm = null;
            _delayAlarm?.Reset(_coolDelay);
            _delayAlarm?.Start();
        }
        _predictedCounter = newPredictedCounter;
        (var gaussian, var _) = new SplitMix64((ulong)_predictedCounter).NextGaussian();
        _lastGaussian = (float)gaussian;
        RefreshSpawnPoint();
    }

    void StartCooling(float _)
    {
        // Debug.Log($"{Time.time}: Starting cooling.");
        if (_coolAlarm != null)
        {
            Debug.Log("We already have a cool alarm, which shouldn't be possible.");
            throw new Exception();
        }
        _coolAlarm = TimerManager.Singleton.AddAlarm(
            cooldown: 0f,
            callback: Cool,
            startImmediately: true,
            armImmediately: true,
            autoRestart: true,
            autoRearm: false,
            initialCooldown: 0f,
            destroyAfterTriggered: false
        );
    }

    void Cool(float deltaTime)
    {
        _heat = Math.Clamp(_heat - deltaTime * _coolPerSecond, 0f, 1f);
        // Debug.Log($"{Time.time}: Cooled down to {_heat}.");
        RefreshSpawnPoint();
        if (_heat == 0f)
        {
            _coolAlarm.Remove();
            _coolAlarm = null;
        }
    }

    void RefreshSpawnPoint()
    {
        float variance = _maxVariance * _varianceCurve.Evaluate(_heat);
        float mean = _maxDrift * _driftCurve.Evaluate(_heat);
        float error = mean + variance * _lastGaussian;
        // Debug.Log($"{Time.time}: Refreshed spawn point with variance {variance}, mean {mean}, error {error}.");
        _spawnPoint.localEulerAngles = new Vector3(0f, 0f, error);
    }

    bool CheckSpawn(Transform spawnPoint)
    {
        if (_spawnPoint == null)
            return true;
        if (_spawnPoint.localPosition != Vector3.zero || _spawnPoint.localRotation != Quaternion.identity || _spawnPoint.localScale != Vector3.one)
            return false;
        return true;
    }
}
