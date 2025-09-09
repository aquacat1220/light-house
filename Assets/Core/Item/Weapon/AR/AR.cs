using System;
using System.Collections;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class AR : NetworkBehaviour
{
    [SerializeField]
    Item _item;
    [SerializeField]
    AutoFire _autoFire;
    [SerializeField]
    Light2D _muzzleFlash;
    [SerializeField]
    Transform _muzzleTransform;
    [SerializeField]
    Light2D _flashlight;
    [SerializeField]
    float _damage;
    [SerializeField]
    GameObject _bulletTrace;

    float _muzzleFlashPerFire = 0.6f;
    float _muzzleFlashMax = 3.0f;
    float _muzzleFlashDecay = 2.0f;

    ItemSystem _itemSystem;
    Hand _hand;

    void Awake()
    {
        if (_item == null)
        {
            Debug.Log("`_item` wasn't set.");
            throw new Exception();
        }
        _item.RegisterImpl = Register;
        _item.UnregisterImpl = Unregister;

        if (_autoFire == null)
        {
            Debug.Log("`_singleFire` wasn't set.");
            throw new Exception();
        }

        if (_muzzleFlash == null)
        {
            Debug.Log("`_muzzleFlash` wasn't set.");
            throw new Exception();
        }
        StartCoroutine(ReduceMuzzleFlash());

        if (_muzzleTransform == null)
        {
            Debug.Log("`_muzzleTransform` wasn't set.");
            throw new Exception();
        }

        if (_flashlight == null)
        {
            Debug.Log("`_flashlight` wasn't set.");
            throw new Exception();
        }

        if (_bulletTrace == null)
        {
            Debug.Log("`_bulletTrace` wasn't set.");
            throw new Exception();
        }
    }

    IEnumerator ReduceMuzzleFlash()
    {
        while (true)
        {
            _muzzleFlash.intensity = Mathf.Max(_muzzleFlash.intensity - _muzzleFlashDecay * Time.deltaTime, 0);
            yield return null;
        }
    }

    // This function is not synced, and should be called on all observers.
    // Item systems should take care of that.
    bool Register(ItemRegisterContext registerContext)
    {
        if (registerContext is PlayerCharacterItemRegisterContext playerCharacterItemRegisterContext)
        {
            var itemSystem = playerCharacterItemRegisterContext.ItemSystem;
            var hand = playerCharacterItemRegisterContext.Hand;
            // Reset transform to zero, so the AR will be located right at the anchor.
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
            if (hand == Hand.Left)
            {
                transform.SetParent(itemSystem.LeftItemAnchor, false);
            }
            else
            {
                transform.SetParent(itemSystem.RightItemAnchor, false);
            }
            if (base.IsServerInitialized)
            {
                _autoFire.Register(Fire);
            }
            if (base.IsOwner)
            {
                if (hand == Hand.Left)
                {
                    itemSystem.LeftItemPrimary += OnPrimary;
                    itemSystem.LeftItemSecondary += OnSecondary;
                }
                else
                {
                    itemSystem.RightItemPrimary += OnPrimary;
                    itemSystem.RightItemSecondary += OnSecondary;
                }
                itemSystem.GetComponent<PlayerCharacterSight>()?.RegisterLight(_flashlight);
            }
            _itemSystem = itemSystem;
            _hand = hand;
            return true;

        }
        return false;
    }

    // This function is not synced, and should be called on all observers.
    // Item systems should take care of that.
    void Unregister()
    {
        Assert.IsNotNull(_itemSystem);
        if (_itemSystem is PlayerCharacterItemSystem itemSystem)
        {
            if (base.IsServerInitialized)
            {
                SetUnregisterTransform(transform.parent.position, transform.parent.eulerAngles.z);
                _autoFire.Unregister();
            }
            transform.SetParent(null, false);
            if (base.IsOwner)
            {
                if (_hand == Hand.Left)
                {
                    itemSystem.LeftItemPrimary -= OnPrimary;
                    itemSystem.LeftItemSecondary -= OnSecondary;
                }
                else
                {
                    itemSystem.RightItemPrimary -= OnPrimary;
                    itemSystem.RightItemSecondary -= OnSecondary;
                }
                itemSystem.GetComponent<PlayerCharacterSight>()?.UnregisterLight(_flashlight);
            }
            _itemSystem = null;
            return;
        }

        Debug.Log("`Pistol` encountered unknown `ItemSystem` variant during `Unregister()`, which shouldn't have been `Register()`ed in the first place.");
        throw new Exception();
    }

    // Set's the final transform of the unregistered pistol.
    // `scale` is omitted, since we never change it anyway.
    [ObserversRpc(RunLocally = true, BufferLast = true)]
    void SetUnregisterTransform(Vector2 position, float rotation)
    {
        transform.localPosition = position;
        transform.localEulerAngles = new Vector3(0f, 0f, rotation);
    }

    [Client(RequireOwnership = true)]
    void OnPrimary(bool wasPerformed)
    {
        Debug.Log("DFDF AR primary.");
        if (wasPerformed)
        {
            _autoFire.StartFireClient();
            return;
        }
        if (!wasPerformed)
        {
            _autoFire.StopFireClient();
            return;
        }

    }

    [Client(RequireOwnership = true)]
    void OnSecondary(bool wasPerformed)
    {
        if (!wasPerformed)
            return;
        UnregisterServer();
    }

    [ServerRpc]
    void UnregisterServer()
    {
        if (_itemSystem is PlayerCharacterItemSystem itemSystem)
        {
            itemSystem.UnregisterItem(_hand);
        }
    }

    [Server]
    void Fire()
    {
        RaycastHit2D hit = Physics2D.Raycast(_muzzleTransform.position, _muzzleTransform.up);
        if (hit)
        {
            HealthSystem healthSystem = hit.rigidbody?.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.ApplyDamage(_damage);
            }
        }
        // Due to `SingleFire` implementations, this function gets called only on the server.
        // It's our responsibility to sync the firing logic back to clients and observers.
        FireObserver();
    }

    [ObserversRpc(RunLocally = true)]
    void FireObserver()
    {
        RaycastHit2D hit = Physics2D.Raycast(_muzzleTransform.position, _muzzleTransform.up);
        if (hit)
        {
            var bulletTrace = Instantiate(_bulletTrace, _muzzleTransform.position, _muzzleTransform.rotation);
            bulletTrace.GetComponent<BulletTrace>()?.SetEndPosition(hit.point);
        }
        _muzzleFlash.intensity = Mathf.Min(_muzzleFlash.intensity + _muzzleFlashPerFire, _muzzleFlashMax);
        Debug.Log("Fired");
    }

}
