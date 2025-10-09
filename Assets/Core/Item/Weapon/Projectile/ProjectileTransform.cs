using System;
using FishNet.Managing.Timing;
using FishNet.Object;
using UnityEngine;


public class ProjectileTransform : NetworkBehaviour
{
    [SerializeField]
    float _initialSpeed = 1f;
    [SerializeField]
    Rigidbody2D _rigidbody;

    void Awake()
    {
        if (_rigidbody == null)
        {
            Debug.Log("`_rigidbody` wasn't set.");
            throw new Exception();
        }
    }

    public override void OnStartNetwork()
    {
        TimeManager.OnTick += OnTick;
    }

    public override void OnStopNetwork()
    {
        TimeManager.OnTick -= OnTick;
    }

    void OnTick()
    {
        Vector2 delta = transform.up * _initialSpeed * (float)TimeManager.TickDelta;
        _rigidbody.MovePosition(_rigidbody.position + delta);
    }
}
