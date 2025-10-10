using System;
using FishNet.Component.Ownership;
using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Serializing;
using UnityEngine;


public class ProjectileTransform : NetworkBehaviour
{
    [SerializeField]
    float _speed = 1f;
    [SerializeField]
    Rigidbody2D _rigidbody;

    // The tick this projectile was instantiated.
    PreciseTick _instantiatedTick = PreciseTick.GetUnsetValue();

    // The time we need to catch up.
    // On non-predicted cases, this value is 0 on server and positive on all clients.
    // On predicted cases, this value is 0 on server, positive on non-spawning clients, and negative on spawning client.
    // This is because the spawning client spawns the projectile before the server does.
    float _timeToCatchUp = 0f;

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
        Debug.Log("ProjectileTransform OnStartNetwork.");
        TimeManager.OnTick += OnTick;
        if (!_instantiatedTick.IsValid())
        {
            _instantiatedTick = TimeManager.GetPreciseTick(TickType.Tick);
        }
    }

    public override void OnStopNetwork()
    {
        TimeManager.OnTick -= OnTick;
    }

    public override void OnStartServer()
    {
        Debug.Log("ProjectileTransform OnStartServer.");
    }

    public override void OnStartClient()
    {
        Debug.Log("ProjectileTransform OnStartClient.");
    }

    void OnTick()
    {
        float deltaTime = (float)TimeManager.TickDelta;
        if (_timeToCatchUp != 0f)
        {
            float catchUp = _timeToCatchUp * 0.01f;
            _timeToCatchUp -= catchUp;

            if (Math.Abs(_timeToCatchUp) <= deltaTime / 2f)
            {
                catchUp += _timeToCatchUp;
                _timeToCatchUp = 0f;
            }

            deltaTime += catchUp;
            Debug.Log($"Caught up {catchUp * 1000} milliseconds.");
            catchUp = 0f;
        }
        Vector2 delta = transform.up * _speed * deltaTime;
        _rigidbody.MovePosition(_rigidbody.position + delta);
    }

    public override void WritePayload(NetworkConnection connection, Writer writer)
    {
        Debug.Log("ProjectileTransform WritePayload.");
        // This function will be called on the server during spawning.
        // And it will also be called on the spawning client if it is being predicted-spawned.
        // We want to send our `_instantiatedTick` only if we are the server.
        writer.WriteUInt32(0);

        if (connection.IsValid)
        {
            Debug.Log("ProjectileTransform WritePayload written.");
            // We are on the server.
            writer.WriteUInt32(_instantiatedTick.Tick);
            writer.WriteDouble(_instantiatedTick.PercentAsDouble);
        }
    }

    public override void ReadPayload(NetworkConnection connection, Reader reader)
    {
        Debug.Log("ProjectileTransform ReadPayload.");
        // This function will be called on all clients when spawned.
        // And it will also be called on the server if it is being predicted-spawned.
        // We want to read the server's `_instantiatedTick` only if we are on clients.
        reader.ReadUInt32();

        if (connection == null || !connection.IsValid)
        {
            Debug.Log("ProjectileTransform ReadPayload read.");
            // We are on clients. Read the server's `_instantiatedTick`.
            var tick = reader.ReadUInt32();
            var percent = reader.ReadDouble();
            var spawnedTick = new PreciseTick(tick, percent);

            // `_instantiatedTick` holds the tick when this projectile was instantiated.
            // If this value is not valid, this means we never encountered `OnStartNetwork()`, which implies we are non-spawning clients.
            // In this case, we are instantiating the projectile NOW. Fetch the current tick.
            if (!_instantiatedTick.IsValid())
            {
                _instantiatedTick = TimeManager.GetPreciseTick(TickType.Tick);
            }
            _timeToCatchUp = (float)TimeManager.TicksToTime(_instantiatedTick) - (float)TimeManager.TicksToTime(spawnedTick);
        }
    }
}
