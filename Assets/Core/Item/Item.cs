using System;
using FishNet.Object;
using UnityEngine;

// Base class of all items.
// Items can *register* and *unregister* from an item system, which will control the interaction.
public class Item : NetworkBehaviour
{

    public Func<ItemRegisterContext, bool> RegisterImpl;
    public Action UnregisterImpl;


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

    // This function shouldn't be called directly: the `ItemSystem`'s matching function should be called instead.
    // Registering might fail, and return `false`.
    public bool Register(ItemRegisterContext registerContext)
    {
        if (registerContext is PlayerCharacterItemRegisterContext)
        {
            return RegisterImpl.Invoke(registerContext);
        }
        else
        {
            Debug.Log("Unknown variant of `ItemRegisterContext` was encountered during item registration.");
            throw new Exception();
        }
    }

    public void Unregister()
    {
        UnregisterImpl.Invoke();
    }
}
