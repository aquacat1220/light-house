using System;
using System.Collections.Generic;
using UnityEngine;

public class SubtickAlarmInfo : AlarmInfoBase
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

    public SubtickAlarmInfo(float cooldown, bool isArmed, bool isStarted, bool isRemoved, bool willTick, bool autoRearm, bool autoRestart, bool destroyAfterTriggered, Action callback)
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

// Think of the `Timer` component as a time bomb.
// We add new bombs with the `AddAlarm()` method.
// Call `Alarm.Start()`, the timer starts counting down.
// Call `Alarm.Stop()`, the timer stops counting down.
// When the timer reaches zero, it stops there until the bomb is armed with `Alarm.Arm()`.
// If the bomb is armed, it "goes off".
// The timer will restart/rearm/remove the bomb according to its settings, then trigger the callback.
// The callback then can decide to override the automatic behavior.
public class SubtickTimer : TimerBase
{
    float _time = 0f;
    Heap<SubtickAlarmInfo, float> _tick = Heap.MinHeap<SubtickAlarmInfo, float>();
    Dictionary<SubtickAlarmInfo, float> _noTick = new Dictionary<SubtickAlarmInfo, float>();

    protected override void Tick(float deltaTime)
    {
        TickAlarms(deltaTime);
        base.Tick(deltaTime);
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


    public override bool StartAlarm(AlarmInfoBase alarmBase)
    {
        SubtickAlarmInfo alarm = alarmBase as SubtickAlarmInfo;
        if (alarm == null)
            throw new Exception();
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

    public override bool StopAlarm(AlarmInfoBase alarmBase)
    {
        SubtickAlarmInfo alarm = alarmBase as SubtickAlarmInfo;
        if (alarm == null)
            throw new Exception();
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

    public override bool ResetAlarm(AlarmInfoBase alarmBase, float newCooldown)
    {
        SubtickAlarmInfo alarm = alarmBase as SubtickAlarmInfo;
        if (alarm == null)
            throw new Exception();
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

    public override void RemoveAlarm(AlarmInfoBase alarmBase)
    {
        SubtickAlarmInfo alarm = alarmBase as SubtickAlarmInfo;
        if (alarm == null)
            throw new Exception();
        if (alarm.IsRemoved)
            return;
        alarm.IsRemoved = true;

        if (alarm.WillTick)
            _tick.Remove(alarm);
        else
            _noTick.Remove(alarm);
    }

    // Returns true if the alarm is now armed, false if the alarm was already removed from the timer.
    public override bool ArmAlarm(AlarmInfoBase alarmBase)
    {
        SubtickAlarmInfo alarm = alarmBase as SubtickAlarmInfo;
        if (alarm == null)
            throw new Exception();
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

    public override bool DisarmAlarm(AlarmInfoBase alarmBase)
    {
        SubtickAlarmInfo alarm = alarmBase as SubtickAlarmInfo;
        if (alarm == null)
            throw new Exception();
        if (alarm.IsRemoved)
            return false;
        if (!alarm.IsArmed)
            return true;
        alarm.IsArmed = false;

        // Disarming an alarm never changes its tick status.
        return true;
    }

    // By default, adds an alarm that is started and armed, will auto rearm, but won't auto restart.
    // Basically an one-time alarm that needs a restart after being triggered.
    public override Alarm AddAlarm(
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
        var alarm = new SubtickAlarmInfo(
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
