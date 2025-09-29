using System;
using FishNet.Object;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{
    [SerializeField]
    GameObject _item;

    GameObject _spawnedItem;

    void Awake()
    {
        if (_item == null)
        {
            Debug.Log("`_item` wasn't set.");
            throw new Exception();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!base.IsServerInitialized)
            // Spawning and equipping items are only possible on the server.
            return;
        GameObject collider = collision.gameObject;
        var _inventory = collider.GetComponent<PlayerCharacterInventory>();
        if (_inventory != null)
        {
            // The collider is the player character. Add the item to the inventory.
            if (_spawnedItem == null)
            {
                _spawnedItem = Instantiate(_item, transform.position, transform.rotation);
                Spawn(_spawnedItem, base.Owner, gameObject.scene);
            }
            if (_inventory.AddItem(_spawnedItem.GetComponent<Item>()))
                // `PlayerCharacterInventory.AddItem()` will return true only if the item was successfully equipped in the inventory.
                _spawnedItem = null;
        }
    }
}
