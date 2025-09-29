using FishNet.Object;
using UnityEngine;

public class ItemTransform : NetworkBehaviour
{
    enum TransformMode
    {
        Attached,
        Detached
    }

    // Invariant:
    // 1. If `_mode == TransformMode.Detached`, `_position == transform.position && _rotation == transform.rotation.eulerAngles.z && _scale == transform.lossyScale`.
    // 2. If `_mode == TransformMode.Attached`, 
    TransformMode _mode = TransformMode.Detached;
    Vector2 _position;
    float _rotation;

    void Awake()
    {
        _position = transform.position;
        _rotation = transform.rotation.eulerAngles.z;
    }

    public void OnRegister(ItemSlot itemSlot)
    {
        AttachToTransform(itemSlot.transform);
    }

    public void OnUnregister()
    {
        DetachFromTransform();
    }

    void AttachToTransform(Transform parent)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.SetParent(parent, worldPositionStays: false);
        _mode = TransformMode.Attached;
    }

    void DetachFromTransform()
    {
        if (base.IsServerInitialized)
        {
            // We don't do this on the clients to ensure synchronization.
            // In theory, the client could first make a *guess*, then get corrected by the server.
            // But in this case, the correction might arrive before the guess, thus never correcting the wrong guess.
            _position = transform.parent.position;
            _rotation = transform.parent.rotation.eulerAngles.z;
            BroadcastNewPosRot(_position, _rotation);
        }
        // We do this instead of `transform.SetParent(null, worldPositionStays: true);`.
        // The above will change the item's scale.
        transform.SetParent(null, worldPositionStays: false);
        _mode = TransformMode.Detached;
        ReflectNewPosRot();
    }

    [ObserversRpc(ExcludeServer = true, BufferLast = true)]
    void BroadcastNewPosRot(Vector2 position, float rotation)
    {
        _position = position;
        _rotation = rotation;
        ReflectNewPosRot();
    }

    void ReflectNewPosRot()
    {
        if (_mode == TransformMode.Detached)
        {
            transform.position = _position;
            transform.rotation = Quaternion.Euler(0f, 0f, _rotation);
        }
    }
}
