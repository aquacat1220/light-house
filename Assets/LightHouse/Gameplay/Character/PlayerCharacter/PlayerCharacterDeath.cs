using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCharacterDeath : NetworkBehaviour
{
    [SerializeField]
    UnityEvent Death;

    // [Server]
    public void Die()
    {
        if (!base.IsServerInitialized)
            return;
        DieLocal();
        DieRpc();
    }

    [ObserversRpc(ExcludeServer = true, BufferLast = true)]
    void DieRpc()
    {
        DieLocal();
    }

    void DieLocal()
    {
        Death?.Invoke();
    }
}
