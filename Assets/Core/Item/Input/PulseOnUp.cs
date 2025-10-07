using UnityEngine;
using UnityEngine.Events;

public class PulseOnUp : MonoBehaviour
{
    [SerializeField]
    float _pulseLength = 0.25f;

    [SerializeField]
    UnityEvent _pulseUp;
    [SerializeField]
    UnityEvent _pulseDown;
    [SerializeField]
    UnityEvent<bool> _pulseChange;

    Alarm _alarm;

    bool _pulseState = false;

    void Awake()
    {
        _alarm = TimerManager.Singleton.AddAlarm(
            cooldown: _pulseLength,
            callback: PulseDown,
            startImmediately: false,
            armImmediately: true,
            autoRestart: false,
            autoRearm: true,
            initialCooldown: _pulseLength,
            destroyAfterTriggered: false
        );
    }

    public void OnPulseUp()
    {
        PulseUp();
        _alarm.Start();
    }

    public void OnPulseDown()
    {
    }

    public void OnPulseChange(bool isUp)
    {
        if (isUp)
            OnPulseUp();
        else
            OnPulseDown();
    }

    void PulseUp()
    {
        if (_pulseState)
            return;
        _pulseState = true;
        _pulseUp?.Invoke();
        _pulseChange?.Invoke(_pulseState);
    }

    void PulseDown()
    {
        if (!_pulseState)
            return;
        _pulseState = false;
        _pulseDown?.Invoke();
        _pulseChange?.Invoke(_pulseState);
    }
}
