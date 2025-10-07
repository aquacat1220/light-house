using UnityEngine;
using UnityEngine.Events;

public class RepeatUpDown : MonoBehaviour
{
    [SerializeField]
    float _cooldown = 0.25f;

    [SerializeField]
    UnityEvent _pulseUp;
    [SerializeField]
    UnityEvent _pulseDown;
    [SerializeField]
    UnityEvent<bool> _pulseChange;

    Alarm _alarm;

    void Awake()
    {
        _alarm = TimerManager.Singleton.AddAlarm(
            cooldown: _cooldown,
            callback: PulseUpDown,
            startImmediately: true,
            armImmediately: false,
            autoRestart: true,
            autoRearm: true,
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
    }

    public void OnPulseChange(bool isUp)
    {
        if (isUp)
            OnPulseUp();
        else
            OnPulseDown();
    }

    void PulseUpDown()
    {
        _pulseUp?.Invoke();
        _pulseChange?.Invoke(true);
        _pulseDown?.Invoke();
        _pulseChange?.Invoke(false);
    }
}
