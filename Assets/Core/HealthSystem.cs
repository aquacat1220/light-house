using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class HealthSystem : NetworkBehaviour
{
    [SerializeField]
    float MaxHealth = 100.0f;
    [SerializeField]
    float InitialHealth = 100.0f;

    public event Action HealthZero;

    readonly SyncVar<float> _health = new();

    public void Awake()
    {
        _health.SetInitialValues(InitialHealth);
        _health.OnChange += OnHealthChange;
    }

    [Server]
    public void ApplyDamage(float damage)
    {
        if (!base.IsServerInitialized)
            return;
        _health.Value = Mathf.Clamp(_health.Value - damage, 0, MaxHealth);
    }

    [Server]
    public void ApplyRepair(float repair)
    {
        if (!base.IsServerInitialized)
            return;
        _health.Value = Mathf.Clamp(_health.Value + repair, 0, MaxHealth);
    }

    void OnHealthChange(float prev, float next, bool asServer)
    {
        // Checking `base.IsServerInitialized` isn't enough, because 
        // on client host scenarios, the callback will be invoked twice, once on client and once on server.
        // Checking for `asServer` should be enough, but just in case...
        if (next == 0f && asServer && base.IsServerInitialized)
        {
            HealthZero.Invoke();
        }
    }
}
