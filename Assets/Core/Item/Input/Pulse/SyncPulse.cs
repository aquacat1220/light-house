using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class SyncPulse : NetworkBehaviour
{
    [SerializeField]
    bool _syncToServer = true;
    [SerializeField]
    bool _syncToObservers = true;

    [SerializeField]
    UnityEvent _pulseUp;
    [SerializeField]
    UnityEvent _pulseDown;
    [SerializeField]
    UnityEvent<bool> _pulseChange;

    bool _pulseState = false;

    public override void OnStopClient()
    {
        // If we are destroyed/despawned, or no longer an observer, stop the pulse from going forever.
        // A bufferlast RPC will put us back to sync.
        PulseChangeLocal(false);
    }

    public void OnPulseUp()
    {
        if (_syncToServer && !_syncToObservers)
            PulseChangeLocal(true);
        else if (!_syncToServer && _syncToObservers)
            PulseChange(true);
        else if (_syncToServer && _syncToObservers)
        {
            // Hosts can double as a server and an observer.
            // If we do `PulseChangeLocal(); PulseChange();`, observing hosts will trigger the callback twice.
            // Call `PulseChangeLocal()` to trigger on server, then `PulseChangeExcludeServer()` to trigger on observers, excluding the observing host.
            PulseChangeLocal(true);
            PulseChangeExcludeServer(true);
        }
    }

    public void OnPulseDown()
    {
        if (_syncToServer && !_syncToObservers)
            PulseChangeLocal(false);
        else if (!_syncToServer && _syncToObservers)
            PulseChange(false);
        else if (_syncToServer && _syncToObservers)
        {
            PulseChangeLocal(false);
            PulseChangeExcludeServer(false);
        }
    }

    public void OnPulseChange(bool isUp)
    {
        if (isUp)
            OnPulseUp();
        else
            OnPulseDown();
    }

    [ObserversRpc(BufferLast = true)]
    void PulseChange(bool isUp)
    {
        PulseChangeLocal(isUp);
    }

    [ObserversRpc(ExcludeServer = true, BufferLast = true)]
    void PulseChangeExcludeServer(bool isUp)
    {
        PulseChangeLocal(isUp);
    }

    void PulseChangeLocal(bool isUp)
    {
        if (isUp)
            PulseUpLocal();
        else
            PulseDownLocal();
    }

    void PulseUpLocal()
    {
        if (_pulseState)
            return;
        _pulseState = true;
        _pulseUp?.Invoke();
        _pulseChange?.Invoke(_pulseState);
    }

    void PulseDownLocal()
    {
        if (!_pulseState)
            return;
        _pulseState = false;
        _pulseDown?.Invoke();
        _pulseChange?.Invoke(_pulseState);
    }
}
