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

    // Dictionaty mapping connected clients to a bool representing if they have a character spawned for them.
    Dictionary<NetworkConnection, bool> _clients = new();
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

        // Then loop over all connected clients to spawn them characters.
        foreach (var keyvalue in _clients)
        {
            var clientConnection = keyvalue.Key;
            var hasCharacter = keyvalue.Value;
            if (!hasCharacter)
            {
                TrySpawnCharacter(clientConnection);
            }
        }
    }

    // Unsubscribe from events after object deinitializes.
    public override void OnStopServer()
    {
        // Unsubscribe from events, and clear the internal dict.
        UnsubscribeFromClientConnected();
        UnsubscribeFromConnEvent();
        _clients.Clear();
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

        // Then loop over all connected clients to spawn them characters.
        foreach (var keyvalue in _clients)
        {
            var clientConnection = keyvalue.Key;
            var hasCharacter = keyvalue.Value;
            if (!hasCharacter)
            {
                TrySpawnCharacter(clientConnection);
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
            _onClientConnected += TrySpawnCharacter;
            _isSubscribedToClientConnected = true;
        }
    }

    void UnsubscribeFromClientConnected()
    {
        if (_isSubscribedToClientConnected)
        {
            _onClientConnected -= TrySpawnCharacter;
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
        if (_clients.ContainsKey(connectedClient))
        {
            Debug.Log("`CharacterSpawner` recognizes a client that is supposed to be connecting for the first time.");
            throw new Exception();
        }
        _clients.Add(connectedClient, false);
        _onClientConnected?.Invoke(connectedClient);
    }

    void OnServerManagerRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            // Connection has been stopped.
            // Remove the conenction from `_clients`.
            _clients.Remove(connection);
            // We don't care if the connection wasn't in the dict, because `Remove()` will just return false if so.
        }
    }

    // Try spawning a character for `clientConnection` if we don't have one yet.
    // This function calls into `SpawnCharacterUnchecked()` after doing some checks, which in turn actually spawns the character.
    void TrySpawnCharacter(NetworkConnection clientConnection)
    {
        if (!base.IsServerInitialized)
        {
            // If we are not the server, spawning the character shouldn't be possible.
            // Since we bind this function only on the server, this shouldn't happen. But just to make sure...
            Debug.Log("`TrySpawnCharacter` was called on non-server.");
            return;
        }

        if (!_clients.ContainsKey(clientConnection))
        {
            Debug.Log("`TrySpawnCharacter` was called with a client that doesn't seem to be connected yet.");
            throw new Exception();
        }
        if (_clients[clientConnection])
        {
            // This client already has a character spawned by this instance.
            return;
        }
        SpawnCharacterUnchecked(clientConnection);
    }

    // Spawns a character for `clientConnection`, unchecked, untracked.
    // This function doesn't check if we have a character for the client, nor keep track that we indeed spawned one for them.
    // External calls to this function may be made to respawn characters: it's the caller's responsibility to make sure we don't have two characters for the same client.
    // Should be only called on the server.
    public void SpawnCharacterUnchecked(NetworkConnection clientConnection)
    {
        if (!base.IsServerInitialized)
        {
            return;
        }
        GameObject character = Instantiate(_characterPrefab, SelectSpawnPosition(clientConnection), Quaternion.identity);
        base.Spawn(character, clientConnection, gameObject.scene);
        _clients[clientConnection] = true;
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
