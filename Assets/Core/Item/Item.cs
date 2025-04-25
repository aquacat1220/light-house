using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Assertions;

// Base class of all items.
// Items can *register* and *unregister* from an item system, which will control the interaction.
public class Item : NetworkBehaviour
{

    public Func<ItemRegisterContext, bool> RegisterImpl;
    public Action UnregisterImpl;

    // The currently registered itemsystem.
    ItemSystem _itemSystem;

    // We do this check in `Start()`, because the handlers are expected to be set by the implementors in `Awake()`.
    public void Start()
    {
        if (RegisterImpl == null)
        {
            Debug.Log("`RegisterImpl` wasn't set.");
            throw new Exception();
        }
        if (UnregisterImpl == null)
        {
            Debug.Log("`UnregisterImpl` wasn't set.");
            throw new Exception();
        }
    }

    public override void OnStartClient()
    {
        Debug.Log("Item client start.");
    }

    // This function shouldn't be called directly: the `ItemSystem`'s matching function should be called instead.
    // Registering might fail, and return `false`.
    // This function is not synced, and should be called on all instances (including the server) to ensure synchronization.
    public bool Register(ItemRegisterContext registerContext)
    {
        if (registerContext is PlayerCharacterItemRegisterContext playerCharacterItemRegisterContext)
        {
            if (RegisterImpl.Invoke(registerContext))
            {
                _itemSystem = playerCharacterItemRegisterContext.ItemSystem;
                return true;
            }
            return false;
        }

        // Unrecognized `ItemSystem` variants are ignored.
        return false;
    }

    public void Unregister()
    {
        Assert.IsNotNull(_itemSystem);

        if (_itemSystem is PlayerCharacterItemSystem playerCharacterItemSystem)
        {
            _itemSystem = null;
            UnregisterImpl.Invoke();
            return;
        }

        Debug.Log("`Item` encountered unknown `ItemSystem` variant during `Unregister()`, which shouldn't have been `Register()`ed in the first place.");
        throw new Exception();
    }
}
