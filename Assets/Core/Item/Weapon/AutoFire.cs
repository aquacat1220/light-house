using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class AutoFire : NetworkBehaviour
{
    [SerializeField]
    UnityEvent _fire;

    [SerializeField]
    float _fireCooldown = 0.5f;
    [SerializeField]
    bool _fireOnlyOnServer = false;

    TimerHandle _cooldown;
    PlayerCharacterInput _input;

    public void OnRegister(ItemSlot itemSlot)
    {
        if (base.IsOwner)
        {
            _input = itemSlot.FindComponent<PlayerCharacterInput>();
            _input.Primary.AddListener(OnPrimary);
        }
        if (base.IsServerInitialized)
        {
            // Add a recurrent alarm that is always started, but needs manual arming everytime, starting now.
            _cooldown = TimerManager.Singleton.AddAlarm(
                cooldown: _fireCooldown,
                callback: Fire,
                startImmediately: true,
                armImmediately: false,
                autoRestart: true,
                autoRearm: true,
                initialCooldown: 0f,
                destroyAfterTriggered: false
            );
        }
    }

    public void OnUnregister()
    {
        if (_input != null)
        {
            _input.Primary.RemoveListener(OnPrimary);
            _input = null;
        }
        if (_cooldown != null)
        {
            _cooldown.Remove();
            _cooldown = null;
        }
    }

    // Responds to the primary action.
    [Client(RequireOwnership = true)]
    void OnPrimary(bool isPerformed)
    {
        if (isPerformed)
            StartFire();
        else
            StopFire();
    }

    [ServerRpc(RequireOwnership = true)]
    void StartFire()
    {
        _cooldown.Arm();
    }

    [ServerRpc(RequireOwnership = true)]
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
