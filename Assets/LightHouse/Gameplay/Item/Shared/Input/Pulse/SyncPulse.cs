using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

// Runs on the server, syncing server-side input pulses to observers and the server.
public class SyncPulse : NetworkBehaviour
{
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
        PulseChange(true);
    }

    public void OnPulseDown()
    {
        PulseChange(false);
    }

    public void OnPulseChange(bool isUp)
    {
        if (isUp)
            OnPulseUp();
        else
            OnPulseDown();
    }

    [ObserversRpc(BufferLast = true, RunLocally = true)]
    void PulseChange(bool isUp)
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
