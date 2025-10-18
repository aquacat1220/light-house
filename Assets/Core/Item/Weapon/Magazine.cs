using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

// A client-predicted magazine.
// The mag (conceptually) maintains a "remaining ammo" count.
// Each time `OnFire()` is called it will check if we have any ammo, and if so, decrement it and trigger the `_fire` event.
// On the server this is completely authoritative.
// On the client `OnFire()` should be predictively called.
// But `OnFire()` doesn't implements prediction on its own; corrections should be manually made with `CorrectAmmo()`.
// This correction function is designed to rely on downstream components that implement prediction; hook these up so the mag can correct itself when the downstream corrects it.
// And `_fire` will be predictively called, so listeners should implement prediction.
public class Magazine : NetworkBehaviour
{
    public int Capacity
    {
        get
        {
            return (int)_capacity;
        }
    }

    public int LeftAmmo
    {
        get
        {
            int leftAmmo = (int)(_reloadPoint - _shotsFired);
            return Mathf.Clamp(leftAmmo, 0, (int)_capacity);
        }
    }

    public UnityEvent<(int Old, int New)> LeftAmmoChange;

    [SerializeField]
    static float _maxWaitTime = 0.025f;

    [SerializeField]
    UnityEvent _fire;
    [SerializeField]
    uint _capacity = 10;
    [SerializeField]
    float _reloadTime = 2.5f;

    uint _shotsFired = 0;
    uint _reloadPoint;

    bool _isReloading = false;

    Alarm _reloadAlarm;
    Alarm _predictedReloadAlarm;

    int _stepsIntoFuture = 0;
    bool _isLastPredictionStart = false;
    uint? _nextReloadPoint = null;

    void Awake()
    {
        _reloadPoint = _capacity;
        // If we are the server, we need a reload timer.
        // And we also need a timer to cancel reloads when client mispredicts.
    }

    public override void OnStartServer()
    {
        _reloadAlarm = TimerManager.Singleton.AddAlarm(
            cooldown: _reloadTime,
            callback: EndReloadServer,
            startImmediately: false,
            autoRestart: false,
            initialCooldown: _reloadTime
        );
    }

    public override void OnStopServer()
    {
        _reloadAlarm?.Remove();
    }

    public override void OnStartClient()
    {
        _predictedReloadAlarm = TimerManager.Singleton.AddAlarm(
            cooldown: _reloadTime,
            callback: EndReloadClient,
            startImmediately: false,
            autoRestart: false,
            initialCooldown: _reloadTime
        );
    }

    public override void OnStopClient()
    {
        _predictedReloadAlarm?.Remove();
    }

    public void TryFire()
    {
        if (!base.IsServerInitialized && !base.IsOwner)
        {
            Debug.Log("`TryFire()` should only be called on the server or the owner.");
            throw new Exception();
        }

        if (_shotsFired >= _reloadPoint)
            return;
        if (_isReloading)
            return;
        var oldLeftAmmo = LeftAmmo;
        _shotsFired += 1;
        if (oldLeftAmmo != LeftAmmo)
            LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
        _fire?.Invoke();
    }

    public void StartReload()
    {
        if (_isReloading)
            return;
        if (base.IsServerInitialized)
        {
            // Starting reload on server. This is authoritative.
            StartReloadServer();
            return;
        }
        if (base.IsOwner)
        {
            // Starting reload on non-server owner. This is predictive, and may be incorrect.
            StartReloadClient();
            return;
        }
        Debug.Log("`StartReload()` should only ever be called on the server or the owner.");
        throw new Exception();
    }

    void StartReloadServer()
    {
        _isReloading = true;
        _nextReloadPoint = _shotsFired + _capacity;
        StartReloadObserver(_reloadPoint, _nextReloadPoint.Value);
        _reloadAlarm.Start();
    }

    void StartReloadClient()
    {
        _isReloading = true;
        _stepsIntoFuture += 1;
        _isLastPredictionStart = true;
        _predictedReloadAlarm.Start();
    }

    // Observer RPCs always hold the most up to date states on the server.
    // Respect it, unless we are already predicting into the future more than two steps.
    [ObserversRpc(ExcludeServer = true)]
    void StartReloadObserver(uint reloadPoint, uint nextReloadPoint)
    {
        if (_stepsIntoFuture > 0)
        {
            // We were predicting into the future.

            // The present stepped closer to the predicted future.
            _stepsIntoFuture -= 1;
            // If we are still predicting into the future, ignore the current correction.
            if (_stepsIntoFuture != 0)
                return;

            if (_isLastPredictionStart)
            {
                /// We predicted the future to be starting a reload, and we were correct.
                if (_isReloading)
                {
                    // Predicted reloading hasn't finished yet.
                    var oldLeftAmmo = LeftAmmo;
                    _reloadPoint = reloadPoint;
                    if (oldLeftAmmo != LeftAmmo)
                        LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
                    _nextReloadPoint = nextReloadPoint;
                    return;
                }
                else
                {
                    // Predicted reloading has already finished.
                    var oldLeftAmmo = LeftAmmo;
                    _reloadPoint = nextReloadPoint;
                    if (oldLeftAmmo != LeftAmmo)
                        LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
                    return;
                }
            }
            else
            {
                // We predicted the future to be canceling a reload, but the prediction was incorrect.
                // This is possible when the client predicts a cancel but the server didn't, and the server later sends a start reload.
                if (_isReloading)
                {
                    // The last prediction was a cancel, but somehow we are still reloading.
                    Debug.Log("Client magazine is reloading when it can't possibly be.");
                    throw new Exception();
                }
                else
                {
                    // The mag isn't reloading. We just do non-predicted reloading.
                    _isReloading = true;
                    var oldLeftAmmo = LeftAmmo;
                    _reloadPoint = reloadPoint;
                    if (oldLeftAmmo != LeftAmmo)
                        LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
                    return;
                }
            }
        }
        _isReloading = true;
        var oldLeftAmmo1 = LeftAmmo;
        _reloadPoint = reloadPoint;
        if (oldLeftAmmo1 != LeftAmmo)
            LeftAmmoChange?.Invoke((oldLeftAmmo1, LeftAmmo));
    }

    void EndReloadServer()
    {
        if (_nextReloadPoint == null)
        {
            Debug.Log("Reloaded ended on server, but `_nextReloadPoint` was null.");
            throw new Exception();
        }
        _isReloading = false;
        var oldLeftAmmo = LeftAmmo;
        _reloadPoint = _nextReloadPoint.Value;
        if (oldLeftAmmo != LeftAmmo)
            LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
        _nextReloadPoint = null;
        EndReloadObserver(_reloadPoint);
    }

    void EndReloadClient()
    {
        _isReloading = false;
        if (_nextReloadPoint is uint nextReloadPoint)
        {
            // A correction from the server was received while the predictive reload timer was ticking.
            var oldLeftAmmo = LeftAmmo;
            _reloadPoint = nextReloadPoint;
            if (oldLeftAmmo != LeftAmmo)
                LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
            _nextReloadPoint = null;
        }
        else
        {
            // No correction was received for the current predicted reload.
            var oldLeftAmmo = LeftAmmo;
            _reloadPoint = _shotsFired + _capacity;
            if (oldLeftAmmo != LeftAmmo)
                LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
        }
    }

    [ObserversRpc(ExcludeServer = true)]
    void EndReloadObserver(uint reloadPoint)
    {
        // This reload-end might be:
        // - In the future (the client never predicted it to happen).
        // - In the present (the latest prediction made by the client was this one).
        // - In the past (the latest prediction is already past this point).
        // We have to take care not to overwrite `_reloadPoint` in the past case.
        if (_stepsIntoFuture > 0)
        {
            // Case past: since we ack predicted reloads on reload-start, `_unackedPredictedReloadCount > 0` means we have a predicted reload that wasn't acked yet.
            return;
        }
        // If case future, we can overwrite both values.
        // If case present, the values must already be set.
        _isReloading = false;
        var oldLeftAmmo = LeftAmmo;
        _reloadPoint = reloadPoint;
        if (oldLeftAmmo != LeftAmmo)
            LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
    }

    public void CancelReload()
    {
        if (!_isReloading)
            return;
        if (base.IsServerInitialized)
        {
            // Canceling reload on server. This is authoritative.
            CancelReloadServer();
            return;
        }
        if (base.IsOwner)
        {
            // Canceling reload on non-server owner. This is predictive, and may be incorrect.
            CancelReloadClient();
            return;
        }
        Debug.Log("`CancelReload()` should only ever be called on the server or the owner.");
        throw new Exception();
    }

    void CancelReloadServer()
    {
        _isReloading = false;
        _reloadAlarm.Stop();
        _reloadAlarm.Reset(_reloadTime);
        CancelReloadObserver(_reloadPoint);
    }

    void CancelReloadClient()
    {
        _isReloading = false;
        _stepsIntoFuture += 1;
        _isLastPredictionStart = false;
        _predictedReloadAlarm.Stop();
        _predictedReloadAlarm.Reset(_reloadTime);
    }

    [ObserversRpc(ExcludeServer = true)]
    void CancelReloadObserver(uint reloadPoint)
    {
        if (_stepsIntoFuture > 0)
        {
            // We were predicting into the future.

            // The present stepped closer to the predicted future.
            _stepsIntoFuture -= 1;
            // If we are still predicting into the future, ignore the current correction.
            if (_stepsIntoFuture != 0)
                return;

            if (_isLastPredictionStart)
            {
                // We predicted the future to be starting a reload, but the server-authoritative truth tells otherwise.
                if (_isReloading)
                {
                    // The predicted reload is ongoing. Cancel it.
                    _isReloading = false;
                    _predictedReloadAlarm.Stop();
                    _predictedReloadAlarm.Reset(_reloadTime);
                    var oldLeftAmmo = LeftAmmo;
                    _reloadPoint = reloadPoint;
                    if (oldLeftAmmo != LeftAmmo)
                        LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
                    _nextReloadPoint = null;
                    return;
                }
                else
                {
                    // The reloading was already finished. Revert it.
                    var oldLeftAmmo = LeftAmmo;
                    _reloadPoint = reloadPoint;
                    if (oldLeftAmmo != LeftAmmo)
                        LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
                    return;
                }
            }
            else
            {
                // We predicted the future to be canceling a reload, and the prediction was correct.
                if (_isReloading)
                {
                    // The last prediction was a cancel, but somehow we are still reloading.
                    Debug.Log("Client magazine is reloading when it can't possibly be.");
                    throw new Exception();
                }
                else
                {
                    // The mag isn't reloading, which is normal. Just keep the state up to date.
                    _isReloading = false;
                    var oldLeftAmmo = LeftAmmo;
                    _reloadPoint = reloadPoint;
                    if (oldLeftAmmo != LeftAmmo)
                        LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
                    return;
                }
            }
        }
        _isReloading = false;
        var oldLeftAmmo1 = LeftAmmo;
        _reloadPoint = reloadPoint;
        if (oldLeftAmmo1 != LeftAmmo)
            LeftAmmoChange?.Invoke((oldLeftAmmo1, LeftAmmo));
    }

    public void CorrectAmmo(int correction)
    {
        var oldLeftAmmo = LeftAmmo;
        _shotsFired = _shotsFired + (uint)correction;
        if (oldLeftAmmo != LeftAmmo)
            LeftAmmoChange?.Invoke((oldLeftAmmo, LeftAmmo));
    }
}
