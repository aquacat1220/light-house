using FishNet.Object;

// Child classes of `ItemSystem` should define their own `ItemRegisterContext` variant too.
// See `./Assets/Characters/PlayerCharacter/PlayerCharacterItemSystem.cs` for example.

// Context object to be passed in `Item.Register()`.
// Holds data that should be known to the item to be registered to the itemsystem.
// i.e. Which hand is the item equipped on?
public abstract class ItemRegisterContext { }

// Base class of all item systems.
// Item systems are components that can register an item.
public abstract class ItemSystem : NetworkBehaviour { }
