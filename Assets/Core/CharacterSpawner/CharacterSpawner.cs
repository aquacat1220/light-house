using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

// Responsible for spawning characters for connected-and-authenticated clients.
// When initialized on the server, spawns characters for all connected-and-authenticated clients,
// as well as future connected-and-authenticated clients.
public class CharacterSpawner : NetworkBehaviour
{
    public static CharacterSpawner Singleton { get; private set; }

    [SerializeField]
    GameObject _characterPrefab;

    [SerializeField]
    Transform[] _spawnPositions;

    bool _isSubscribedToClientConnected = false;
    bool _isSubscribedToConnEvent = false;

    // Dictionary mapping connected clients to a `(wasSpawned, spawnedCharacter)` tuple.
    // `wasSpawned` is `true` if this class has ever spawned a character for the connection.
    // `spawnedCharacter` holds a reference to the latest spawned character, which can be `null` if it was somehow destroyed.
    Dictionary<NetworkConnection, (bool, GameObject)> _connectedClients = new();
    event Action<NetworkConnection> _onClientConnected;

    uint _nextSpawnPositionIndex = 0;

    void Awake()
    {
        if (_characterPrefab == null)
        {
            Debug.Log("`playerPrefab` wasn't set.");
            throw new Exception();
        }

        if (_spawnPositions.Length == 0)
        {
            Debug.Log("Spawn positions are empty. Will default to origin.");
        }

        if (Singleton != null)
        {
            Debug.Log("`Singleton` was non-null, implying there are multiple instances of `CharacterSpawner`s in this scene.");
            throw new Exception();
        }
        Singleton = this;
    }

    public override void OnStartClient()
    {
        // Notify the server that this client has finished loading the scene and connected.
        ConnectClient();
    }

    // Spawn characters on the server side for all connected-and-authenticated clients.
    public override void OnStartServer()
    {
        // First subscribe to `_onClientConnected` to make sure no clients are lost.
        SubscribeToClientConnected();
        // And to the connection state change event to handle disconenctions.
        SubscribeToConnEvent();
        // Then spawn characters for missed clients.
        SpawnMissing();
    }

    // Unsubscribe from events after object deinitializes.
    public override void OnStopServer()
    {
        // Unsubscribe from events, and clear the internal dict.
        UnsubscribeFromClientConnected();
        UnsubscribeFromConnEvent();
        _connectedClients.Clear();
    }

    void OnEnable()
    {
        if (!base.IsServerInitialized)
        {
            // If we are not the server, no need to proceed.
            return;
        }

        // We ARE the server. Proceed to spawn characters for all connected clients that might have arrived while we were disabled.

        // First subscribe to `_onClientConnected` to make sure no clients are lost.
        SubscribeToClientConnected();
        // And to the connection state change event to handle disconnections.
        // We should already be subscribed, since we don't subscribe on disable, but just to be sure.
        SubscribeToConnEvent();
        // Then spawn characters for missed clients.
        SpawnMissing();
    }

    void SpawnMissing()
    {
        // Loop over all connected clients to spawn characters for missed clients.
        foreach (var keyvalue in _connectedClients)
        {
            var clientConnection = keyvalue.Key;
            var (wasSpawned, _) = keyvalue.Value;
            if (!wasSpawned)
            {
                SpawnFirstCharacter(clientConnection);
            }
        }
    }

    void OnDisable()
    {
        // Ignore newly connecting clients, but still keep track of them in the dictionary.
        // They will be assigned a character as soon as the spawner is re-enabled.
        UnsubscribeFromClientConnected();
    }

    void SubscribeToClientConnected()
    {
        if (!_isSubscribedToClientConnected)
        {
            _onClientConnected += SpawnFirstCharacter;
            _isSubscribedToClientConnected = true;
        }
    }

    void UnsubscribeFromClientConnected()
    {
        if (_isSubscribedToClientConnected)
        {
            _onClientConnected -= SpawnFirstCharacter;
            _isSubscribedToClientConnected = false;
        }
    }

    void SubscribeToConnEvent()
    {
        if (!_isSubscribedToConnEvent)
        {
            base.ServerManager.OnRemoteConnectionState += OnServerManagerRemoteConnectionState;
            _isSubscribedToConnEvent = true;
        }
    }

    void UnsubscribeFromConnEvent()
    {
        if (_isSubscribedToConnEvent)
        {
            base.ServerManager.OnRemoteConnectionState += OnServerManagerRemoteConnectionState;
            _isSubscribedToConnEvent = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ConnectClient(NetworkConnection connectedClient = null)
    {
        if (_connectedClients.ContainsKey(connectedClient))
        {
            Debug.Log("`CharacterSpawner` recognizes a client that is supposed to be connecting for the first time.");
            throw new Exception();
        }
        _connectedClients.Add(connectedClient, (false, null));
        _onClientConnected?.Invoke(connectedClient);
    }

    void OnServerManagerRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            // Connection has been stopped.
            // Kill the character of that client, and remove the conenction from `_clients`.
            if (_connectedClients.ContainsKey(connection))
            {
                var (_, character) = _connectedClients[connection];
                character.GetComponent<PlayerCharacterDeath>()?.Die();
                _connectedClients.Remove(connection);
            }
        }
    }

    // Spawn the first ever character for `clientConnection`, and track it.
    // If we ever spawned a character for it, skip.
    // This function calls into `SpawnCharacterUnchecked()` after doing some checks, which in turn actually spawns the character.
    [Server]
    void SpawnFirstCharacter(NetworkConnection clientConnection)
    {
        if (!base.IsServerInitialized)
        {
            // If we are not the server, spawning the character shouldn't be possible.
            // Since we bind this function only on the server, this shouldn't happen. But just to make sure...
            return;
        }

        if (!_connectedClients.ContainsKey(clientConnection))
        {
            Debug.Log("`SpawnFirstCharacter()` was called with a client that doesn't seem to be connected yet.");
            throw new Exception();
        }
        if (_connectedClients[clientConnection].Item1)
        {
            // This is not the first time we are trying to spawn a character for this client.
            return;
        }
        SpawnCharacter(clientConnection);
    }

    // Spawns a character for a connection, and tracks it.
    // This might not be the first spawning attempt; we will still spawn a character and untrack the old one.
    [Server]
    public void SpawnCharacter(NetworkConnection clientConnection)
    {
        if (!base.IsServerInitialized)
            return;

        if (!_connectedClients.ContainsKey(clientConnection))
        {
            Debug.Log("`SpawnCharacter()` was called with a client that doesn't seem to be connected yet.");
            throw new Exception();
        }

        GameObject character = Instantiate(_characterPrefab, SelectSpawnPosition(clientConnection), Quaternion.identity);
        base.Spawn(character, clientConnection, gameObject.scene);
        _connectedClients[clientConnection] = (true, character);
    }

    Vector3 SelectSpawnPosition(NetworkConnection clientConnection)
    {
        var length = _spawnPositions.Length;
        if (length == 0)
        {
            return Vector3.zero;
        }

        return _spawnPositions[_nextSpawnPositionIndex++ % length].transform.position;
    }
}
