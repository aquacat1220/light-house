using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : NetworkBehaviour
{
    [SerializeField]
    float MaxHealth = 100.0f;
    [SerializeField]
    float InitialHealth = 100.0f;

    public UnityEvent OnHealthZero;

    readonly SyncVar<float> _health = new();

    public void Awake()
    {
        _health.SetInitialValues(InitialHealth);
        _health.OnChange += OnHealthChange;

        if (OnHealthZero.GetPersistentEventCount() == 0)
        {
            Debug.Log("No one is listening to `OnHealthZero`.");
            throw new Exception();
        }
    }

    public void ApplyDamage(float damage)
    {
        if (!base.IsServerStarted)
            return;
        _health.Value = Mathf.Clamp(_health.Value - damage, 0, MaxHealth);
    }

    public void ApplyRepair(float repair)
    {
        if (!base.IsServerStarted)
            return;
        _health.Value = Mathf.Clamp(_health.Value + repair, 0, MaxHealth);
    }

    void OnHealthChange(float prev, float next, bool asServer)
    {
        if (next == 0f)
        {
            OnHealthZero.Invoke();
        }
    }
}
