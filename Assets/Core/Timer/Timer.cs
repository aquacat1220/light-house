using System;
using System.Collections.Generic;
using UnityEngine;

public class AlarmInfo
{
    public float Cooldown;
    public float RemainingCooldown;
    public bool IsRecurrent;
    public bool IsActive;
    public bool DestroyAfterTriggered;
    public bool MarkedForRemoval;
    public Action Callback;

    public AlarmInfo(float cooldown, float remainingCooldown, bool isRecurrent, bool isActive, bool destroyAfterTriggered, bool markedForRemoval, Action callback)
    {
        Cooldown = cooldown;
        RemainingCooldown = remainingCooldown;
        IsRecurrent = isRecurrent;
        IsActive = isActive;
        DestroyAfterTriggered = destroyAfterTriggered;
        MarkedForRemoval = markedForRemoval;
        Callback = callback;
    }
}

public class TimerHandle
{
    Timer _timer;
    AlarmInfo _alarm;

    public TimerHandle(Timer timer, AlarmInfo alarm)
    {
        _timer = timer;
        _alarm = alarm;
    }

    public void Activate()
    {
        _alarm.IsActive = true;
    }

    public void Deactivate()
    {
        _alarm.IsActive = false;
    }

    public void Remove()
    {
        _alarm.IsActive = false;
        _alarm.MarkedForRemoval = true;
    }

    public bool IsActive()
    {
        return _alarm.IsActive;
    }

    public float RemainingCooldown()
    {
        return _alarm.RemainingCooldown;
    }
}

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
        if (!alarm.IsActive || alarm.MarkedForRemoval)
            return;
        alarm.RemainingCooldown -= deltaTime;
        while (alarm.RemainingCooldown <= 0f && alarm.IsActive && !alarm.MarkedForRemoval)
        {
            alarm.RemainingCooldown += alarm.Cooldown;
            alarm.Callback?.Invoke();
            if (!alarm.IsRecurrent)
            {
                alarm.IsActive = false;
                alarm.RemainingCooldown = alarm.Cooldown;
            }
            if (alarm.DestroyAfterTriggered)
            {
                alarm.IsActive = false;
                alarm.MarkedForRemoval = true;
            }
        }
    }

    public TimerHandle AddAlarm(float cooldown, Action callback, bool startActive = true, bool isRecurrent = false, bool destroyAfterTriggered = true)
    {
        AlarmInfo alarm = new AlarmInfo(
            cooldown: cooldown,
            remainingCooldown: cooldown,
            isRecurrent: isRecurrent,
            isActive: startActive,
            destroyAfterTriggered: destroyAfterTriggered,
            markedForRemoval: false,
            callback: callback
        );

        _alarms.Add(alarm);

        return new TimerHandle(this, alarm);
    }
}
