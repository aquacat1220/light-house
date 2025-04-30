using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

// Automatically despawns the gameobject when a certain amount of time passes after getting disowned.
public class AutoDespawn : NetworkBehaviour
{
    [SerializeField]
    float _despawnDelay = 10.0f;

    Coroutine _countdown;

    public override void OnOwnershipServer(NetworkConnection prevOwner)
    {
        // If the new owner is valid, this is a normal ownership transfer. Return early.
        if (base.Owner.IsValid)
        {
            // This also means the despawn counter should stop.
            StopCoroutine(_countdown);
            // Not sure if this line is needed: maybe Unity nulls out stopped coroutines?
            _countdown = null;
            return;
        }
        // Else, the new owner is not valid.
        // We want the gameobject to be despawned.

        // Start a coroutine to despawn the object after `_despawnDelay` seconds.
        _countdown = StartCoroutine(DespawnCountdown());
    }

    IEnumerator DespawnCountdown()
    {
        yield return new WaitForSeconds(_despawnDelay);
        base.Despawn();
    }
}
