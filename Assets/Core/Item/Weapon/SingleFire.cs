using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class SingleFire : NetworkBehaviour
{
    [SerializeField]
    UnityEvent _fire;

    [SerializeField]
    float _fireCooldown = 1.0f;
    [SerializeField]
    bool _fireOnlyOnServer = false;

    Alarm _cooldown;

    bool _canFire = true;

    public override void OnStartServer()
    {
        _cooldown = TimerManager.Singleton.AddAlarm(
            cooldown: _fireCooldown,
            callback: Fire,
            startImmediately: true,
            armImmediately: false,
            autoRestart: true,
            autoRearm: false,
            initialCooldown: 0f,
            destroyAfterTriggered: false
        );
    }

    public override void OnStopServer()
    {
        _cooldown.Remove();
    }

    // Responds to the primary action.
    [Client(RequireOwnership = true)]
    public void OnFire(bool isPerformed)
    {
        TryFire(isPerformed);
    }

    public void PreventFire()
    {
        // Stop the current fire action.
        StopFire();
        // And ignore any future fire actions.
        _canFire = false;
    }

    public void AllowFire()
    {
        // Allow future fire actions to trigger `Fire()`.
        _canFire = true;
        // But we don't bring back the old input state.
    }

    [ServerRpc(RequireOwnership = true)]
    void TryFire(bool isPerformed)
    {
        if (!_canFire)
            return;
        if (isPerformed)
            StartFire();
        else
            StopFire();
    }

    [Server]
    void StartFire()
    {
        _cooldown.Arm();
    }

    [Server]
    void StopFire()
    {
        _cooldown.Disarm();
    }

    [Server]
    void Fire()
    {
        if (_fireOnlyOnServer)
            _fire?.Invoke();
        else
            FireObserver();
    }

    [ObserversRpc(RunLocally = true)]
    void FireObserver()
    {
        _fire?.Invoke();
    }
}
