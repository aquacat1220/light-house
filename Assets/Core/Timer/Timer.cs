using System;
using System.Collections.Generic;
using UnityEngine;

public class AlarmInfo
{
    public Timer Timer;
    public float Cooldown;
    public float RemainingCooldown;
    public bool IsStarted;
    public bool IsArmed;
    public bool AutoRearm;
    public bool AutoRestart;
    public bool DestroyAfterTriggered;
    public bool MarkedForRemoval;
    public Action Callback;

    public AlarmInfo(float cooldown, float remainingCooldown, bool isStarted, bool isArmed, bool autoRearm, bool autoRestart, bool destroyAfterTriggered, bool markedForRemoval, Action callback)
    {
        Cooldown = cooldown;
        RemainingCooldown = remainingCooldown;
        IsStarted = isStarted;
        IsArmed = isArmed;
        AutoRearm = autoRearm;
        AutoRestart = autoRestart;
        DestroyAfterTriggered = destroyAfterTriggered;
        MarkedForRemoval = markedForRemoval;
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
        var oldStarted = _alarm.IsStarted;
        _alarm.IsStarted = true;
        _timer.RefreshAlarm(_alarm);
        return oldStarted;
    }

    public bool Stop()
    {
        var oldStarted = _alarm.IsStarted;
        _alarm.IsStarted = false;
        _timer.RefreshAlarm(_alarm);
        return oldStarted;
    }

    public float Reset()
    {
        return Reset(_alarm.Cooldown);
    }

    public float Reset(float newRemainingCooldown)
    {
        var oldRemainingCooldown = _alarm.RemainingCooldown;
        _alarm.RemainingCooldown = newRemainingCooldown;
        _timer.RefreshAlarm(_alarm);
        return oldRemainingCooldown;
    }

    public bool Arm()
    {
        var oldArmed = _alarm.IsArmed;
        _alarm.IsArmed = true;
        _timer.RefreshAlarm(_alarm);
        return oldArmed;
    }

    public bool Disarm()
    {
        var oldArmed = _alarm.IsArmed;
        _alarm.IsArmed = false;
        _timer.RefreshAlarm(_alarm);
        return oldArmed;
    }

    public void Remove()
    {
        _alarm.MarkedForRemoval = true;
    }

    public Action Callback(Action newCallback)
    {
        Action oldCallback = _alarm.Callback;
        _alarm.Callback = newCallback;
        _timer.RefreshAlarm(_alarm);
        return oldCallback;
    }

    // public bool AutoRestart(bool autoRestart)
    // {
    //     var oldAutoRestart = _alarm.AutoRestart;
    //     _alarm.AutoRestart = autoRestart;
    //     return oldAutoRestart;
    // }

    // public bool AutoRearm(bool autoRearm)
    // {
    //     var oldAutoRearm = _alarm.AutoRearm;
    //     _alarm.AutoRearm = autoRearm;
    //     return oldAutoRearm;
    // }

    // public bool DestroyAfterTriggered(bool destroyAfterTriggered)
    // {
    //     var oldDestroyAfterTriggered = _alarm.DestroyAfterTriggered;
    //     _alarm.DestroyAfterTriggered = destroyAfterTriggered;
    //     return oldDestroyAfterTriggered;
    // }
}

// Think of the `Timer` component as a time bomb.
// We add new bombs with the `AddAlarm()` method.
// Call `TimerHandle.Start()`, the timer starts counting down.
// Call `TimerHandle.Stop()`, the timer stops counting down.
// When the timer reaches zero, it stops there until the bomb is armed with `TimerHandle.Arm()`.
// If the bomb is armed, it "goes off" by triggering the callback.
// *After* the bomb goes off, the bomb will automatically reset its timer, but won't restart or rearm, unless `autoRestart` and `autoRearm` is set.
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
    List<AlarmInfo> _alarms = new List<AlarmInfo>();

    public float RateMultiplier = 1f;

    void Awake()
    {
        Parent = _initialParent;
    }

    protected void Tick(float deltaTime)
    {
        float multDeltaTime = deltaTime * RateMultiplier;
        foreach (var alarm in _alarms)
        {
            TickAlarm(alarm, multDeltaTime);
        }
        _alarms.RemoveAll((alarm) => alarm.MarkedForRemoval);
        foreach (var child in _children)
        {
            child.Tick(multDeltaTime);
        }
    }

    void TickAlarm(AlarmInfo alarm, float deltaTime)
    {
        // If the alarm is started, reduce its remaining cooldown.
        if (alarm.IsStarted)
            alarm.RemainingCooldown -= deltaTime;

        // Alarms with cooldown shorter than the game tickrate may trigger more than once per tick.
        // Thus we put the triggering cycle in a loop.
        while (!alarm.MarkedForRemoval && alarm.RemainingCooldown <= 0f)
        {
            if (!alarm.IsArmed)
            {
                // This alarm isn't armed. Set its cooldown to 0, and break the loop.
                alarm.RemainingCooldown = 0f;
                break;
            }
            float underflow = alarm.RemainingCooldown;
            // Reset the alarm's remaining cooldown to `alarm.Cooldown`, so the callback will always see `alarm.RemainingCooldown == alarm.Cooldown` when triggered.
            alarm.RemainingCooldown = alarm.Cooldown;
            // Similarly, ensure that the callback will always see `alarm.IsStarted == false && alarm.IsArmed == false` when triggered.
            // This is because the alarms are conceptually "one-time", and will optionally "restart/rearm" after being triggered.
            alarm.IsStarted = false;
            alarm.IsArmed = false;
            alarm.Callback?.Invoke();
            // The callback can freely change `IsStarted`, `IsArmed`, `AutoRestart`, `AutoRearm`, and other values. Be careful.

            if (alarm.AutoRestart)
                alarm.IsStarted = true;

            if (alarm.IsStarted)
                // Add the underflow to the remaining cooldown, as the alarm must've kept ticking after triggering.
                alarm.RemainingCooldown += underflow;

            if (alarm.AutoRearm)
                alarm.IsArmed = true;

            if (alarm.DestroyAfterTriggered)
                alarm.MarkedForRemoval = true;
        }
    }

    // By default, adds an alarm that is started and armed, will auto rearm, but won't auto restart.
    // Basically an one-time alarm that needs a restart after being triggered.
    public Alarm AddAlarm(float cooldown, Action callback, bool startImmediately = true, bool armImmediately = true, bool autoRestart = true, bool autoRearm = true, float initialCooldown = -1, bool destroyAfterTriggered = false)
    {
        if (cooldown <= 0f)
        {
            Debug.Log("Attempted to create an alarm with non-positive cooldown.");
            throw new Exception();
        }

        float remainingCooldown = cooldown;
        if (initialCooldown >= 0f)
            remainingCooldown = initialCooldown;


        AlarmInfo alarm = new AlarmInfo(
            cooldown: cooldown,
            remainingCooldown: remainingCooldown,
            isStarted: startImmediately,
            isArmed: armImmediately,
            autoRearm: autoRearm,
            autoRestart: autoRestart,
            destroyAfterTriggered: destroyAfterTriggered,
            markedForRemoval: false,
            callback: callback
        );

        _alarms.Add(alarm);
        RefreshAlarm(alarm);

        return new Alarm(this, alarm);
    }

    // Used to refresh an alarm after its info changes, so that the alarm doesn't have to wait another tick to be triggered.
    public void RefreshAlarm(AlarmInfo alarm)
    {
        TickAlarm(alarm, 0f);
    }
}
