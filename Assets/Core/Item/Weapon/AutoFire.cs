using System;
using FishNet.Object;
using UnityEngine;

public class AutoFire : NetworkBehaviour
{
    // If `_fire` is non-null, this means we are the server, and someone registered to this component.
    Action _fire;

    [SerializeField]
    float _firingDelay = 0.5f;

    float _remainingDelay = 0f;

    bool _isFiring = false;

    void Update()
    {
        // If `_fire` is null, this means either we are not the server, or no one registered.
        // Since timer countdown is only necessary on the server for registered components, we can return early.
        if (_fire == null)
            return;

        _remainingDelay = Mathf.Clamp(_remainingDelay - Time.deltaTime, 0, _firingDelay);
        if (_isFiring && _remainingDelay <= 0f)
        {
            _fire?.Invoke();
            _remainingDelay = _firingDelay;
        }
    }

    // Registers the `fire` delegate to be triggered on firing.
    [Server]
    public void Register(Action fire)
    {
        if (!base.IsServerInitialized)
        {
            // Registering should happen on the server, which will then replicate the effects to the clients.
            return;
        }
        // Unregister first to be sure.
        Unregister();

        _fire = fire;
    }

    [Server]
    public void Unregister()
    {
        _fire = null;
        _remainingDelay = 0f;
    }

    [Client(RequireOwnership = true)]
    public void StartFireClient()
    {
        if (!base.IsOwner)
            return;
        ToggleFire(true);
    }

    [Client(RequireOwnership = true)]
    public void StopFireClient()
    {
        if (!base.IsOwner)
            return;
        ToggleFire(false);
    }

    [ServerRpc]
    void ToggleFire(bool fire)
    {
        _isFiring = fire;
    }
}
