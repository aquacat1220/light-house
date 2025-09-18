using System;
using FishNet.Object;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{
    [SerializeField]
    GameObject _item;

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
        if (collider.GetComponent<PlayerCharacterInput>() == null)
            // The collider isn't the player character.
            return;

        for (var i = 0; i < collider.transform.childCount; i++)
        {
            Transform child = collider.transform.GetChild(i);
            ItemSlot itemSlot = child.GetComponent<ItemSlot>();
            if (itemSlot == null)
                continue;
            if (itemSlot.Item != null)
                continue;
            GameObject item = Instantiate(_item, transform.position, transform.rotation);
            Spawn(item, base.Owner, gameObject.scene);
            itemSlot.Equip(item.GetComponent<Item>());
            break;
        }
    }
}
