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

    public event Action<float, float, bool> HealthChange;

    public float Health
    {
        get
        {
            return _health.Value;
        }
    }

    readonly SyncVar<float> _health = new();

    void Awake()
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
        if (!this.enabled)
            return;
        HealthChange?.Invoke(prev, next, asServer);
    }
}
