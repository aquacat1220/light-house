using System;
using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    Transform[] spawnPositions;

    NetworkManager networkManager;

    bool isSubscribedToEvent = false;

    uint nextSpawnPositionIndex = 0;

    void Awake()
    {
        if (playerPrefab == null)
        {
            Debug.Log("\"playerPrefab\" wasn't set.");
            throw new Exception();
        }

        if (spawnPositions.Length == 0)
        {
            Debug.Log("Spawn positions are empty. Will default to origin.");
        }
    }

    void Start()
    {
        Initialize();
    }

    // Check if we are the authority, and spawn in player characters it we are.
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Set `networkManager`.
        Initialize();

        if (!HasAuthority)
        {
            // No need to go further if we are not the authority. Spawning should happen only on the server side.
            return;
        }

        // First subscribe to the event, to make sure no clients are lost.
        SubscribeToEvent();

        // Then loop over all "already-joined" clients to spawn characters for them.
        foreach (var keyvalue in networkManager.ConnectedClients)
        {
            ulong clientId = keyvalue.Value.ClientId;
            SpawnCharacter(clientId);
        }
    }

    void Initialize()
    {
        if (networkManager == null)
        {
            networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.Log("NetworkManager singleton was null.");
                throw new Exception();
            }
        }
    }

    void OnEnable()
    {
        if (!IsSpawned || !HasAuthority)
        {
            // If we are network-spawned already, or not the authority, no need to proceed.
            return;
        }

        // First subscribe to the event, to make sure no clients are lost.
        SubscribeToEvent();

        // Then loop over all "already-joined" clients to spawn characters for them.
        // We do this in `OnEnable()` too, so that we can spawn characters for clients that we missed.
        foreach (var keyvalue in networkManager.ConnectedClients)
        {
            ulong clientId = keyvalue.Value.ClientId;
            SpawnCharacter(clientId);
        }
    }

    void OnDisable()
    {
        UnsubscribeToEvent();
    }

    void SubscribeToEvent()
    {
        if (!isSubscribedToEvent)
        {
            networkManager.OnConnectionEvent += OnConnectionEvent;
            isSubscribedToEvent = true;
        }
    }

    void UnsubscribeToEvent()
    {
        if (isSubscribedToEvent)
        {
            networkManager.OnConnectionEvent -= OnConnectionEvent;
            isSubscribedToEvent = false;
        }
    }

    void OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        if (data.EventType != ConnectionEvent.ClientConnected)
        {
            // This is not a client connected event.
            return;
        }

        SpawnCharacter(data.ClientId);
    }

    void SpawnCharacter(ulong clientId)
    {
        if (!HasAuthority)
        {
            // If we are not the authority, spawning the character shouldn't be possible.
            // Since we bind this function only on the authority, this shouldn't happen. But just to make sure...
            Debug.Log("\"SpawnCharacter\" was called on non-authority.");
            return;
        }

        if (networkManager.ConnectedClients[clientId].PlayerObject != null)
        {
            // This client already has a player object. Not sure if this can ever happen, but its good to be extra careful.
            return;
        }
        GameObject playerCharacter = Instantiate(playerPrefab, SelectSpawnPosition(clientId), Quaternion.identity);
        NetworkObject playerCharacterNetworkObject = playerCharacter.GetComponent<NetworkObject>();
        if (playerCharacterNetworkObject == null)
        {
            // The supplied player prefab isn't a network object.
            Debug.Log("\"playerPrefab\" doesn't have a NetworkObject component.");
            throw new Exception();
        }
        playerCharacterNetworkObject.SpawnAsPlayerObject(clientId, destroyWithScene: true);
    }

    Vector3 SelectSpawnPosition(ulong clientId)
    {
        var length = spawnPositions.Length;
        if (length == 0)
        {
            return Vector3.zero;
        }

        return spawnPositions[nextSpawnPositionIndex++ % length].transform.position;
    }
}
