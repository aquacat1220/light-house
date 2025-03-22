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
    [SerializeField]
    GameObject _characterPrefab;

    [SerializeField]
    Transform[] _spawnPositions;

    bool _isSubscribedToAuthEvent = false;
    bool _isSubscribedToConnEvent = false;

    HashSet<NetworkConnection> _spawned = new HashSet<NetworkConnection>();

    uint _nextSpawnPositionIndex = 0;

    void Awake()
    {
        if (_characterPrefab == null)
        {
            Debug.Log("\"playerPrefab\" wasn't set.");
            throw new Exception();
        }

        if (_spawnPositions.Length == 0)
        {
            Debug.Log("Spawn positions are empty. Will default to origin.");
        }
    }

    // Spawn characters on the server side for all connected-and-authenticated clients.
    public override void OnStartServer()
    {
        // First subscribe to the auth event to make sure no clients are lost.
        SubscribeToAuthEvent();
        // And to the connection state change event to handle disconenctions.
        SubscribeToConnEvent();

        // Then loop over all connected-and-authenticated clients to spawn characters for them.
        foreach (var keyvalue in base.ServerManager.Clients)
        {
            var clientConnection = keyvalue.Value;
            if (clientConnection.IsAuthenticated)
            {
                SpawnCharacter(clientConnection);
            }
        }
    }

    // Unsubscribe from events after object deinitializes.
    public override void OnStopServer()
    {
        // Unsubscribe from events, and clear the internal hashset.
        UnsubscribeFromAuthEvent();
        UnsubscribeFromConnEvent();
        _spawned.Clear();
    }

    void OnEnable()
    {
        if (!base.IsServerInitialized)
        {
            // If we are not the server, no need to proceed.
            return;
        }

        // We ARE the server. Proceed to spawn characters for all connected-and-authenticated clients that might have changed while we were disabled.

        // First subscribe to the event, to make sure no clients are lost.
        SubscribeToAuthEvent();
        // And to the connection state change event to handle disconenctions.
        // We should already be subscribed, since we don't subscribe on disable, but just to be sure.
        SubscribeToConnEvent();

        // Then loop over all connected-and-authenticated clients to spawn characters for them.
        foreach (var keyvalue in base.ServerManager.Clients)
        {
            var clientConnection = keyvalue.Value;
            if (clientConnection.IsAuthenticated)
            {
                SpawnCharacter(clientConnection);
            }
        }
    }

    void OnDisable()
    {
        UnsubscribeFromAuthEvent();
    }

    void SubscribeToAuthEvent()
    {
        if (!_isSubscribedToAuthEvent)
        {
            base.ServerManager.OnAuthenticationResult += ServerManager_OnAuthenticationResult;
            _isSubscribedToAuthEvent = true;
        }
    }

    void UnsubscribeFromAuthEvent()
    {
        if (_isSubscribedToAuthEvent)
        {
            base.ServerManager.OnAuthenticationResult -= ServerManager_OnAuthenticationResult;
            _isSubscribedToAuthEvent = false;
        }
    }

    void SubscribeToConnEvent()
    {
        if (!_isSubscribedToConnEvent)
        {
            base.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
            _isSubscribedToConnEvent = true;
        }
    }

    void UnsubscribeFromConnEvent()
    {
        if (_isSubscribedToConnEvent)
        {
            base.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
            _isSubscribedToConnEvent = false;
        }
    }

    void ServerManager_OnAuthenticationResult(NetworkConnection clientConnection, bool authenticationPassed)
    {
        if (!authenticationPassed)
        {
            // Authentication failed.
            // According to `ServerManager` implementation, this event wouldn't even be called if authentication fails.
            // But just in case implementations change...!
            return;
        }

        SpawnCharacter(clientConnection);
    }

    void ServerManager_OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            // Connection has been stopped.
            // Remove the conenction from the spawned set.
            _spawned.Remove(connection);
            // We don't care if the connection wasn't in the set, because `Remove()` will just return false if so.
        }
    }

    void SpawnCharacter(NetworkConnection clientConnection)
    {
        if (!base.IsServerInitialized)
        {
            // If we are not the server, spawning the character shouldn't be possible.
            // Since we bind this function only on the server, this shouldn't happen. But just to make sure...
            Debug.Log("\"SpawnCharacter\" was called on non-server.");
            return;
        }

        if (_spawned.Contains(clientConnection))
        {
            // This client already has a character spawned by this instance.
            return;
        }
        _spawned.Add(clientConnection);
        GameObject character = Instantiate(_characterPrefab, SelectSpawnPosition(clientConnection), Quaternion.identity);
        base.Spawn(character, clientConnection, gameObject.scene);
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
