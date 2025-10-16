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
    uint _nextReloadAt;

    bool _isReloading = false;

    Alarm _reloadAlarm;

    void Awake()
    {
        _nextReloadAt = _magazineSize;
        // If we are the server, we need a reload timer.
        // And we also need a timer to cancel reloads when client mispredicts.
    }

    public override void OnStartServer()
    {
        _reloadAlarm = TimerManager.Singleton.AddAlarm(
            cooldown: _reloadTime,
            callback: () => EndReloadRpc(_shotsFired + _magazineSize),
            startImmediately: false,
            autoRestart: false,
            initialCooldown: _reloadTime
        );
    }

    public override void OnStopServer()
    {
        _reloadAlarm?.Remove();
    }

    public void TryFire()
    {
        if (!base.IsServerInitialized && !base.IsOwner)
        {
            Debug.Log("`TryFire()` should only be called on the server or the owner.");
            throw new Exception();
        }

        if (_shotsFired >= _nextReloadAt)
            return;
        if (_isReloading)
            return;
        _shotsFired += 1;
        _fire?.Invoke();
        Debug.Log($"Ammo left: {_nextReloadAt - _shotsFired}.");
    }

    public void Reload()
    {
        if (base.IsServerInitialized)
        {
            if (_isReloading)
                return;
            StartReloadRpc();
            _reloadAlarm.Start();
        }
    }

    public void CancelReload()
    {
        if (base.IsServerInitialized)
        {
            if (!_isReloading)
                return;
            CancelReloadRpc();
            _reloadAlarm.Stop();
            _reloadAlarm.Reset(_reloadTime);
        }
    }

    [ObserversRpc(RunLocally = true)]
    void StartReloadRpc()
    {
        _isReloading = true;
        Debug.Log($"Starting reload. Ammo left: {_nextReloadAt - _shotsFired}.");
    }

    [ObserversRpc(RunLocally = true)]
    void EndReloadRpc(uint nextReloadAt)
    {
        _isReloading = false;
        _nextReloadAt = nextReloadAt;
        Debug.Log($"Ended reload. Ammo left: {_nextReloadAt - _shotsFired}.");
    }

    [ObserversRpc(RunLocally = true)]
    void CancelReloadRpc()
    {
        _isReloading = false;
        Debug.Log($"Ended reload. Ammo left: {_nextReloadAt - _shotsFired}.");
    }

    public void CorrectAmmo(int correction)
    {
        _shotsFired = _shotsFired + (uint)correction;
        Debug.Log($"Corrected ammo. Ammo left: {_nextReloadAt - _shotsFired}.");
    }
}
