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

    TimerHandle _cooldown;
    PlayerCharacterInput _input;

    public void OnRegister(ItemSlot itemSlot)
    {
        Debug.Log($"SingleFire register. Owner: {base.Owner.ClientId}, Local Client: {base.LocalConnection.ClientId}, Is Owner: {base.IsOwner}, Client Init: {base.IsClientInitialized}, Server Init: {base.IsServerInitialized}");
        if (base.IsOwner)
        {
            _input = itemSlot.FindComponent<PlayerCharacterInput>();
            _input.Primary.AddListener(OnPrimary);
        }
        if (base.IsServerInitialized)
            _cooldown = TimerManager.Singleton.AddAlarm(_fireCooldown, null, startActive: false, isRecurrent: false, destroyAfterTriggered: false);
    }

    public void OnUnregister()
    {
        _input.Primary.RemoveListener(OnPrimary);
        _input = null;
    }

    // Responds to the primary action.
    [Client(RequireOwnership = true)]
    void OnPrimary(bool isPerformed)
    {
        if (!isPerformed)
            return;
        TryFire();
    }

    [ServerRpc(RequireOwnership = true)]
    void TryFire()
    {
        if (!_cooldown.IsActive())
        {
            // If cooldown is not active, we can fire and activate the cooldown.
            _cooldown.Activate();

            if (_fireOnlyOnServer)
                _fire?.Invoke();
            else
                FireObserver();
        }
    }

    [ObserversRpc(RunLocally = true)]
    void FireObserver()
    {
        _fire?.Invoke();
    }
}
