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
    PlayerCharacterInput _input;

    bool _canFire = true;

    public void OnRegister(ItemSlot itemSlot)
    {
        if (base.IsOwner)
        {
            _input = itemSlot.FindComponent<PlayerCharacterInput>();
            _input.Primary.AddListener(OnFire);
        }
        if (base.IsServerInitialized)
        {
            if (_cooldown == null)
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
            else
            {
                // We already have a cooldown alarm.
                // _cooldown.Callback(Fire);
            }
        }
    }

    public void OnUnregister()
    {
        if (_input != null)
        {
            _input.Primary.RemoveListener(OnFire);
            _input = null;
        }
        if (_cooldown != null)
        {
            // Stop firing, in case the client never canceled their fire action.
            StopFire();
            // _cooldown.Callback(null);
        }
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
