using System;
using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using LightHouse;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCharacterDeath : NetworkBehaviour
{
    [SerializeField]
    UnityEvent _death;
    [SerializeField, Min(0f)]
    float _respawnDelay = 0f;
    [SerializeField, Min(0f)]
    float _despawnDelay = 0f;

    public override void OnStartServer()
    {
        base.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    public override void OnStopServer()
    {
        base.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    // [Server]
    public void Die()
    {
        if (!base.IsServerInitialized)
            return;
        DieLocal();
        DieRpc();

        if (TimerManager.Singleton != null)
        {
            TimerManager.Singleton.AddAlarm(
                cooldown: _respawnDelay,
                callback: RespawnAlarm,
                startImmediately: true,
                armImmediately: true,
                autoRestart: false,
                autoRearm: false,
                initialCooldown: _respawnDelay,
                destroyAfterTriggered: true
            );
            TimerManager.Singleton.AddAlarm(
                cooldown: _despawnDelay,
                callback: DespawnAlarm,
                startImmediately: true,
                armImmediately: true,
                autoRestart: false,
                autoRearm: false,
                initialCooldown: _despawnDelay,
                destroyAfterTriggered: true
            );
        }
        else
        {
            Debug.Log("`TimerManager` wasn't present in scene.");
            throw new Exception();
        }
    }

    [ObserversRpc(ExcludeServer = true, BufferLast = true)]
    void DieRpc()
    {
        DieLocal();
    }

    void DieLocal()
    {
        _death?.Invoke();
    }

    void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if (connection == base.Owner && args.ConnectionState == RemoteConnectionState.Stopped)
        {
            // Connection has been stopped.
            Die();
        }
    }

    void RespawnAlarm()
    {
        if (base.Owner.IsValid)
        {
            // The owner is still valid. Respawn a character.
            if (CharacterSpawner.Singleton != null)
                CharacterSpawner.Singleton.SpawnCharacter(base.Owner);
            else
                Debug.Log("`CharacterSpawner` wasn't found in scene. Is this normal?");
        }
    }

    void DespawnAlarm()
    {
        base.Despawn();
    }
}
