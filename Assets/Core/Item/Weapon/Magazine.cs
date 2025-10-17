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
    [SerializeField]
    static float _maxWaitTime = 0.025f;

    [SerializeField]
    UnityEvent _fire;
    [SerializeField]
    uint _magazineSize = 10;
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
        _reloadPoint = _magazineSize;
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
        _shotsFired += 1;
        _fire?.Invoke();
        Debug.Log($"Ammo left: {_reloadPoint - _shotsFired}.");
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
        Debug.Log("StartReloadServer.");
        _isReloading = true;
        _nextReloadPoint = _shotsFired + _magazineSize;
        StartReloadObserver(_reloadPoint, _nextReloadPoint.Value);
        _reloadAlarm.Start();
    }

    void StartReloadClient()
    {
        Debug.Log("StartReloadClient.");
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
        Debug.Log("StartReloadObserver.");
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
                    _reloadPoint = reloadPoint;
                    _nextReloadPoint = nextReloadPoint;
                    return;
                }
                else
                {
                    // Predicted reloading has already finished.
                    _reloadPoint = nextReloadPoint;
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
                    _reloadPoint = reloadPoint;
                    return;
                }
            }
        }
        _isReloading = true;
        _reloadPoint = reloadPoint;
    }

    void EndReloadServer()
    {
        Debug.Log("EndReloadServer.");
        if (_nextReloadPoint == null)
        {
            Debug.Log("Reloaded ended on server, but `_nextReloadPoint` was null.");
            throw new Exception();
        }
        _isReloading = false;
        _reloadPoint = _nextReloadPoint.Value;
        _nextReloadPoint = null;
        EndReloadObserver(_reloadPoint);
    }

    void EndReloadClient()
    {
        Debug.Log("EndReloadClient.");
        _isReloading = false;
        if (_nextReloadPoint is uint nextReloadPoint)
        {
            // A correction from the server was received while the predictive reload timer was ticking.
            _reloadPoint = nextReloadPoint;
            _nextReloadPoint = null;
        }
        else
        {
            // No correction was received for the current predicted reload.
            _reloadPoint = _shotsFired + _magazineSize;
        }
    }

    [ObserversRpc(ExcludeServer = true)]
    void EndReloadObserver(uint reloadPoint)
    {
        Debug.Log("EndReloadObserver.");
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
        _reloadPoint = reloadPoint;
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
        Debug.Log("CancelReloadServer.");
        _isReloading = false;
        _reloadAlarm.Stop();
        _reloadAlarm.Reset(_reloadTime);
        CancelReloadObserver(_reloadPoint);
    }

    void CancelReloadClient()
    {
        Debug.Log("CancelReloadClient.");
        _isReloading = false;
        _stepsIntoFuture += 1;
        _isLastPredictionStart = false;
        _predictedReloadAlarm.Stop();
        _predictedReloadAlarm.Reset(_reloadTime);
    }

    [ObserversRpc(ExcludeServer = true)]
    void CancelReloadObserver(uint reloadPoint)
    {
        Debug.Log("CancelReloadObserver.");
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
                    _reloadPoint = reloadPoint;
                    _nextReloadPoint = null;
                    return;
                }
                else
                {
                    // The reloading was already finished. Revert it.
                    _reloadPoint = reloadPoint;
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
                    _reloadPoint = reloadPoint;
                    return;
                }
            }
        }
        _isReloading = false;
        _reloadPoint = reloadPoint;
    }

    public void CorrectAmmo(int correction)
    {
        _shotsFired = _shotsFired + (uint)correction;
        Debug.Log($"Corrected ammo. Ammo left: {_reloadPoint - _shotsFired}.");
    }
}
