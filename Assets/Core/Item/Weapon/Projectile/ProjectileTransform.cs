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

    public ProjectileSpawner ProjectileSpawner;

    // The tick and position this projectile was spawned on the server. Set in `ReadPayload()` (in clients), `OnStartServer()` (in noPS server), `` (in PS server).
    PreciseTick _spawnedTick = PreciseTick.GetUnsetValue();
    Vector2 _spawnedPosition = Vector2.zero;

    // The time we need to catch up.
    // On non-predicted cases, this value is 0 on server and positive on all clients.
    // On predicted cases, this value is 0 on server, positive on non-spawning clients, and negative on spawning client.
    // This is because the spawning client spawns the projectile before the server does.
    float _timeToCatchUp = 0f;
    Vector2 _distanceToCatchUp = Vector2.zero;

    // Catch up time and distance is calculated once at the first `OnTick()` callback.
    bool _calculatedCatchUp = false;

    // Whether we are subscribed to time manager tick events.
    bool _subscribedToTimeManager = false;

    void Awake()
    {
        if (_rigidbody == null)
        {
            Debug.Log("`_rigidbody` wasn't set.");
            throw new Exception();
        }
    }

    void OnEnable()
    {
        if (!_subscribedToTimeManager && base.IsSpawned)
        {
            TimeManager.OnTick += OnTick;
            _subscribedToTimeManager = true;
        }
    }

    void OnDisable()
    {
        if (_subscribedToTimeManager)
        {
            TimeManager.OnTick -= OnTick;
            _subscribedToTimeManager = false;
        }
    }

    public override void OnStartNetwork()
    {
        if (!_subscribedToTimeManager && base.isActiveAndEnabled)
        {
            TimeManager.OnTick += OnTick;
            _subscribedToTimeManager = true;
        }
    }

    public override void OnStopNetwork()
    {
        if (_subscribedToTimeManager)
        {
            TimeManager.OnTick -= OnTick;
            _subscribedToTimeManager = false;
        }
    }

    public override void OnStartServer()
    {
        if (!_spawnedTick.IsValid())
        {
            _spawnedTick = TimeManager.GetPreciseTick(TickType.Tick);
            _spawnedPosition = transform.position;
        }
    }

    // `SetSpawn()` should only be called on the server, *before we send `_spawnedTick` to clients*.
    public void ResetSpawn(PreciseTick spawnedTick, Vector2 spawnedPosition, float spawnedRotation)
    {
        if (!base.IsServerStarted)
        {
            Debug.Log("`ResetSpawn()` was called on a non-server instance.");
            throw new Exception();
        }
        _spawnedTick = spawnedTick;
        _spawnedPosition = spawnedPosition;
        transform.rotation = Quaternion.Euler(0f, 0f, spawnedRotation);
    }

    // Disables this component, and deactivates all child gameobjects.
    // This is because the `NetworkObject` component shouldn't ever be disabled, but we still need a way to stop the projectile GO from affecting the game.
    // Components that should be disabled during waitlist should be added to a separate child GO.
    public void SetActive(bool active)
    {
        enabled = active;
        foreach (Transform childTransform in transform)
        {
            var child = childTransform.gameObject;
            child.SetActive(active);
        }
    }

    void OnTick()
    {
        if (!_calculatedCatchUp && _spawnedTick.IsValid())
        {
            // We have never calculated catch up time and distance. Try calculating now.
            // If `_spawnedTick` is not valid, we are yet to receive the server side projectile details.
            _timeToCatchUp = (float)TimeManager.TicksToTime(TimeManager.GetPreciseTick(TickType.Tick)) - (float)TimeManager.TicksToTime(_spawnedTick);
            _distanceToCatchUp = _spawnedPosition - (Vector2)transform.position;
            _calculatedCatchUp = true;
        }

        float deltaTime = (float)TimeManager.TickDelta;
        if (_timeToCatchUp != 0f)
        {
            float catchUp = _timeToCatchUp * 0.01f;
            _timeToCatchUp -= catchUp;

            if (Math.Abs(_timeToCatchUp) <= deltaTime / 4f)
            {
                catchUp += _timeToCatchUp;
                _timeToCatchUp = 0f;
            }

            deltaTime += catchUp;
            catchUp = 0f;
        }
        Vector2 delta = transform.up * _speed * deltaTime;
        if (_distanceToCatchUp != Vector2.zero)
        {
            Vector2 catchUp = _distanceToCatchUp * 0.01f;
            _distanceToCatchUp -= catchUp;

            if (_distanceToCatchUp.magnitude <= delta.magnitude / 4f)
            {
                catchUp += _distanceToCatchUp;
                _distanceToCatchUp = Vector2.zero;
            }

            delta += catchUp;
            catchUp = Vector2.zero;
        }
        _rigidbody.MovePosition(_rigidbody.position + delta);
    }

    public override void WritePayload(NetworkConnection connection, Writer writer)
    {
        // This function will be called on the server during spawning.
        // And it will also be called on the spawning client if it is being predicted-spawned.

        // The connection is invalid only if we are the predicted-spawning client.
        // If we are the predicted-spawning client, send the parent projectile spawner.
        if (!connection.IsValid)
        {
            if (!NetworkObject.PredictedSpawner.IsLocalClient)
            {
                Debug.Log("`WritePayload()` was called on client, but we are not the predicted spawner.");
                throw new Exception();
            }
            writer.WriteNetworkBehaviour(ProjectileSpawner);
        }
        // If we are the server, send when and where the projectile was spawned.
        else
        {
            // We are on the server.
            writer.WriteUInt32(_spawnedTick.Tick);
            writer.WriteDouble(_spawnedTick.PercentAsDouble);
            writer.WriteVector2(_spawnedPosition);
            writer.WriteSingle(transform.rotation.eulerAngles.z);
        }
    }

    public override void ReadPayload(NetworkConnection connection, Reader reader)
    {
        // This function will be called on all clients when spawned.
        // And it will also be called on the server if it is being predicted-spawned.
        // We want to read when and where the projectile was spawned on the server.

        // If we are the server, read the parent projectile spawner and add this projectile to waitlist.
        if (connection != null && connection.IsValid)
        {
            if (ProjectileSpawner != null)
            {
                Debug.Log("`ProjectileSpawner` is set to non-null value at server before reading from the PS client.");
                throw new Exception();
            }
            ProjectileSpawner = (ProjectileSpawner)reader.ReadNetworkBehaviour();
            ProjectileSpawner.AddProjectileToWaitlist(this);
        }
        // If we are clients, read the projectile's spawn info.
        else
        {
            // We are on clients. Read the server's `_spawnedTick`.
            var tick = reader.ReadUInt32();
            var percent = reader.ReadDouble();
            _spawnedTick = new PreciseTick(tick, percent);
            _spawnedPosition = reader.ReadVector2();
            var spawnedRotation = reader.ReadSingle();
            // Snap rotation to true value.
            transform.rotation = Quaternion.Euler(0f, 0f, spawnedRotation);
        }
    }
}
