using System;
using System.Collections.Generic;
using UnityEngine;

public class AlarmInfo
{
    public Timer Timer;
    public float Cooldown;
    public bool IsArmed;
    public bool IsStarted;
    public bool IsRemoved;
    public bool WillTick;
    public bool AutoRearm;
    public bool AutoRestart;
    public bool DestroyAfterTriggered;
    public Action Callback;

    public AlarmInfo(float cooldown, bool isArmed, bool isStarted, bool isRemoved, bool willTick, bool autoRearm, bool autoRestart, bool destroyAfterTriggered, Action callback)
    {
        Cooldown = cooldown;
        IsArmed = isArmed;
        IsStarted = isStarted;
        IsRemoved = isRemoved;
        WillTick = willTick;
        AutoRearm = autoRearm;
        AutoRestart = autoRestart;
        DestroyAfterTriggered = destroyAfterTriggered;
        Callback = callback;
    }
}

public class Alarm
{
    Timer _timer;
    AlarmInfo _alarm;

    public Alarm(Timer timer, AlarmInfo alarm)
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
public class Timer : MonoBehaviour
{
    [SerializeField]
    Timer _initialParent;

    Timer _parent;
    public Timer Parent
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

    List<Timer> _children = new List<Timer>();

    float _time = 0f;
    MinHeap<AlarmInfo, float> _tick = new MinHeap<AlarmInfo, float>();
    Dictionary<AlarmInfo, float> _noTick = new Dictionary<AlarmInfo, float>();

    public float RateMultiplier = 1f;

    void Awake()
    {
        Parent = _initialParent;
    }

    protected void Tick(float deltaTime)
    {
        TickAlarms(deltaTime);
        TickChildren(deltaTime);
    }

    void TickAlarms(float deltaTime)
    {
        var remainingDeltaTime = deltaTime;
        while (remainingDeltaTime > 0f)
        {
            if (_tick.Peek() == null)
            {
                // No alarms to tick! Spend the remaining time doing nothing.
                _time += remainingDeltaTime * RateMultiplier;
                remainingDeltaTime = 0f;
                continue;
            }
            var alarmTime = _tick.Peek().Value.Priority;
            // We don't want to see alarms ringing in the past.
            alarmTime = Math.Max(alarmTime, _time);

            // Try spending a portion of the remaining delta time to reach the alarm's trigger.
            if ((alarmTime - _time) > remainingDeltaTime * RateMultiplier)
            {
                // The remaining delta time isn't enough to reach the trigger.
                _time += remainingDeltaTime * RateMultiplier;
                remainingDeltaTime = 0f;
                continue;
            }
            // The remaining delta time is enough to reach the trigger.
            var spentDeltaTime = (alarmTime - _time) / RateMultiplier;
            remainingDeltaTime -= spentDeltaTime;
            _time = alarmTime;

            // It is safe to use `.Value()` here because we know the heap has an item to pop.
            var alarm = _tick.Pop().Value.Item;
            if (alarm.IsArmed)
            {
                // A started & armed alarm reached its trigger.
                if (alarm.DestroyAfterTriggered)
                    alarm.IsRemoved = true;
                else
                {
                    alarm.IsStarted = alarm.AutoRestart;
                    alarm.IsArmed = alarm.AutoRearm;
                    // If the alarm is restarted, it should be ticked.
                    alarm.WillTick = alarm.IsStarted;
                    if (alarm.WillTick)
                        _tick.Push(alarm, _time + alarm.Cooldown);
                    else
                        _noTick.Add(alarm, alarm.Cooldown);
                }
                alarm.Callback?.Invoke();
            }
            else
            {
                // A started & disarmed alarm reached its trigger.
                alarm.WillTick = false;
                _noTick.Add(alarm, 0f);
            }
        }
    }


    public bool StartAlarm(AlarmInfo alarm)
    {
        if (alarm.IsRemoved)
            return false;
        if (alarm.IsStarted)
            return true;
        alarm.IsStarted = true;

        if (alarm.WillTick)
        {
            // Stopped alarms can't be ticking.
            throw new Exception();
        }
        else
        {
            if (_noTick.Remove(alarm, out float prevAlarmTime))
            {
                alarm.WillTick = true;
                _tick.Push(alarm, _time + prevAlarmTime);
                return true;
            }
            // `alarm.WillTick == false`, but wasn't found in the `_noTick` dict.
            throw new Exception();
        }
    }

    public bool StopAlarm(AlarmInfo alarm)
    {
        if (alarm.IsRemoved)
            return false;
        if (!alarm.IsStarted)
            return true;
        alarm.IsStarted = false;

        if (alarm.WillTick)
        {
            // Alarm was previously ticking.
            if (_tick.Remove(alarm)?.Priority is float alarmTime)
            {
                alarm.WillTick = false;
                _noTick.Add(alarm, alarmTime - _time);
                return true;
            }
            throw new Exception();
        }
        else
        {
            return true;
        }
    }

    public bool ResetAlarm(AlarmInfo alarm, float newCooldown)
    {
        if (alarm.IsRemoved)
            return false;
        if (alarm.WillTick)
        {
            // Alarm was previously ticking.
            if (_tick.Remove(alarm)?.Priority is float alarmTime)
            {
                _tick.Push(alarm, _time + newCooldown);
                return true;
            }
            throw new Exception();
        }
        else
        {
            _noTick[alarm] = newCooldown;
            return true;
        }
    }

    public void RemoveAlarm(AlarmInfo alarm)
    {
        if (alarm.IsRemoved)
            return;
        alarm.IsRemoved = true;

        if (alarm.WillTick)
            _tick.Remove(alarm);
        else
            _noTick.Remove(alarm);
    }

    // Returns true if the alarm is now armed, false if the alarm was already removed from the timer.
    public bool ArmAlarm(AlarmInfo alarm)
    {
        if (alarm.IsRemoved)
            return false;
        if (alarm.IsArmed)
            return true;
        alarm.IsArmed = true;

        if (alarm.WillTick)
        {
            // Leave the alarm tick to trigger.
            return true;
        }
        else
        {
            // If the alarm is started, bring it to tick.
            if (alarm.IsStarted)
            {
                if (_noTick.Remove(alarm, out float prevAlarmTime))
                {
                    alarm.WillTick = true;
                    _tick.Push(alarm, _time + prevAlarmTime);
                    return true;
                }
                // `alarm.WillTick == false`, but wasn't found in the `_noTick` dict.
                throw new Exception();
            }
            // Else, just leave it there.
            return true;
        }
    }

    public bool DisarmAlarm(AlarmInfo alarm)
    {
        if (alarm.IsRemoved)
            return false;
        if (!alarm.IsArmed)
            return true;
        alarm.IsArmed = false;

        // Disarming an alarm never changes its tick status.
        return true;
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
    public Alarm AddAlarm(
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
        var alarm = new AlarmInfo(
            cooldown: cooldown,
            isArmed: armImmediately,
            isStarted: startImmediately,
            isRemoved: false,
            willTick: startImmediately,
            autoRearm: autoRearm,
            autoRestart: autoRestart,
            destroyAfterTriggered: destroyAfterTriggered,
            callback: callback
        );

        if (startImmediately)
            _tick.Push(alarm, _time + initialCooldown);
        else
            _noTick.Add(alarm, initialCooldown);
        return new Alarm(this, alarm);
    }
}
