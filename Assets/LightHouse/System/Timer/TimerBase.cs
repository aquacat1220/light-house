using System;
using System.Collections.Generic;
using UnityEngine;

public class AlarmInfoBase { }

public class Alarm
{
    TimerBase _timer;
    AlarmInfoBase _alarm;

    public Alarm(TimerBase timer, AlarmInfoBase alarm)
    {
        _timer = timer;
        _alarm = alarm;
    }

    public bool Start()
    {
        return _timer.StartAlarm(_alarm);
    }

    public bool Stop()
    {
        return _timer.StopAlarm(_alarm);
    }

    public bool Reset(float newCooldown)
    {
        return _timer.ResetAlarm(_alarm, newCooldown);
    }

    public void Remove()
    {
        _timer.RemoveAlarm(_alarm);
    }

    public bool Arm()
    {
        return _timer.ArmAlarm(_alarm);
    }

    public bool Disarm()
    {
        return _timer.DisarmAlarm(_alarm);
    }
}

// Think of the `Timer` component as a time bomb.
// We add new bombs with the `AddAlarm()` method.
// Call `Alarm.Start()`, the timer starts counting down.
// Call `Alarm.Stop()`, the timer stops counting down.
// When the timer reaches zero, it stops there until the bomb is armed with `Alarm.Arm()`.
// If the bomb is armed, it "goes off".
// The timer will restart/rearm/remove the bomb according to its settings, then trigger the callback.
// The callback then can decide to override the automatic behavior.
public class TimerBase : MonoBehaviour
{
    [SerializeField]
    TimerBase _initialParent;

    TimerBase _parent;
    public TimerBase Parent
    {
        get
        {
            return _parent;
        }
        set
        {
            if (_parent == value)
                // If `parent == value`, the function is a no-op.
                return;
            if (_parent != null)
            {
                // If `parent != value` and `parent != null`, we will need to break this link anyways.
                _parent._children.Remove(this);
                _parent = null;
            }
            if (value != null)
            {
                value._children.Add(this);
                _parent = value;
            }
        }
    }

    List<TimerBase> _children = new List<TimerBase>();

    public float RateMultiplier = 1f;

    void Awake()
    {
        Parent = _initialParent;
    }

    protected virtual void Tick(float deltaTime)
    {
        TickChildren(deltaTime);
    }

    public virtual bool StartAlarm(AlarmInfoBase alarm)
    {
        return false;
    }

    public virtual bool StopAlarm(AlarmInfoBase alarm)
    {
        return false;
    }

    public virtual bool ResetAlarm(AlarmInfoBase alarm, float newCooldown)
    {
        return false;
    }

    public virtual void RemoveAlarm(AlarmInfoBase alarm)
    {
        return;
    }

    public virtual bool ArmAlarm(AlarmInfoBase alarm)
    {
        return false;
    }

    public virtual bool DisarmAlarm(AlarmInfoBase alarm)
    {
        return false;
    }

    void TickChildren(float deltaTime)
    {
        foreach (var child in _children)
        {
            child.Tick(deltaTime * RateMultiplier);
        }
    }

    // By default, adds an alarm that is started and armed, will auto rearm, but won't auto restart.
    // Basically an one-time alarm that needs a restart after being triggered.
    public virtual Alarm AddAlarm(
        float cooldown,
        Action callback,
        bool startImmediately = true,
        bool armImmediately = true,
        bool autoRestart = true,
        bool autoRearm = true,
        float initialCooldown = 0f,
        bool destroyAfterTriggered = false
    )
    {
        return null;
    }
}
