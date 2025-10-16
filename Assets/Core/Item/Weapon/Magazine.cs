using UnityEngine;
using UnityEngine.Events;

// A client-predicted magazine.
// The mag (conceptually) maintains a "remaining ammo" count.
// Each time `OnFire()` is called it will check if we have any ammo, and if so, decrement it and trigger the `_fire` event.
// On the server this is completely authoritative.
// On the client `OnFire()` should be predictively called.
// But `OnFire()` doesn't implements prediction on its own; corrections should be manually made with `CorrectAmmo()`.
// This correction function is designed to rely on downstream components that implement prediction; hook these up so the mag can correct itself when the downstream corrects it.
// And `_fire` will be predictively called, so listeners should implement prediction.
public class Magazine : MonoBehaviour
{
    [SerializeField]
    UnityEvent _fire;
    [SerializeField]
    uint _magazineSize = 10;

    uint _shotsFired = 0;
    uint _nextReloadAt;

    void Awake()
    {
        _nextReloadAt = _magazineSize;
    }

    public void OnFire()
    {
        Debug.Log($"Attempting to fire. _shotsFired: {_shotsFired}, _nextReloadAt: {_nextReloadAt}.");
        if (_shotsFired >= _nextReloadAt)
            return;
        _shotsFired += 1;
        _fire?.Invoke();
    }

    public void CorrectAmmo(int correction)
    {
        _shotsFired = _shotsFired + (uint)correction;
    }
}
