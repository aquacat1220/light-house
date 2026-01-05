namespace LightHouse
{
    using System;
    using FishNet.Connection;
    using FishNet.Object;
    using FishNet.Transporting;
    using UnityEngine;
    using Fn;

    public class CharacterDeath : NetworkBehaviour
    {
        [SerializeField]
        Vitality _vitality;

        [SerializeField]
        MonoBehaviour[] _disableOnDeath = new MonoBehaviour[0];

        [SerializeField, Min(0f)]
        float _respawnDelay = 0f;
        [SerializeField, Min(0f)]
        float _despawnDelay = 0f;

        // Make sure the character only dies once.
        bool _isDead = false;

        void Awake()
        {
            if (_vitality == null)
            {
                Debug.Log("`_vitality` was not set.");
                throw new Exception();
            }
            _vitality.VitBelowZero += Die;
        }

        public override void OnStartServer()
        {
            base.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        }

        public override void OnStopServer()
        {
            base.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        }

        void OnDestroy()
        {
            _vitality.VitBelowZero -= Die;
        }

        // [Server]
        public void Die()
        {
            if (!base.IsServerInitialized)
                return;
            if (_isDead)
                return;
            _isDead = true;
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
            foreach (var component in _disableOnDeath)
                component.enabled = false;
        }

        void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            if (connection == base.Owner && args.ConnectionState == RemoteConnectionState.Stopped)
            {
                // Connection has been stopped.
                Die();
            }
        }

        void RespawnAlarm(float _)
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

        void DespawnAlarm(float _)
        {
            base.Despawn();
        }
    }
}