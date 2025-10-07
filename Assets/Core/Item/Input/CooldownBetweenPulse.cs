using UnityEngine;
using UnityEngine.Events;

// Stops pulses from traveling down when the cooldown isn't done.
// The cooldown starts when a pulse ends.
public class PulseCooldown : MonoBehaviour
{
    [SerializeField]
    float _cooldown = 1f;

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
            cooldown: _cooldown,
            callback: PulseUp,
            startImmediately: true,
            armImmediately: false,
            autoRestart: false,
            autoRearm: false,
            initialCooldown: 0f,
            destroyAfterTriggered: false
        );
    }

    public void OnPulseUp()
    {
        _alarm.Arm();
    }

    public void OnPulseDown()
    {
        _alarm.Disarm();
        _alarm.Start();
        PulseDown();
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
