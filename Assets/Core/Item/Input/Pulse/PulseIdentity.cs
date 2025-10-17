using UnityEngine;
using UnityEngine.Events;

public class PulseIdentity : MonoBehaviour
{
    [SerializeField]
    UnityEvent _pulseUp;
    [SerializeField]
    UnityEvent _pulseDown;
    [SerializeField]
    UnityEvent<bool> _pulseChange;

    public void OnPulseUp()
    {
        _pulseUp?.Invoke();
        _pulseChange?.Invoke(true);
    }

    public void OnPulseDown()
    {
        _pulseDown?.Invoke();
        _pulseChange?.Invoke(false);
    }

    public void OnPulseChange(bool isUp)
    {
        if (isUp)
            OnPulseUp();
        else
            OnPulseDown();
    }
}
