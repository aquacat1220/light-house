using System;
using System.Collections.Generic;
using UnityEngine;

public class AlarmInfo : AlarmInfoBase
{
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


// Think of the `Timer` component as a time bomb.
// We add new bombs with the `AddAlarm()` method.
// Call `Alarm.Start()`, the timer starts counting down.
// Call `Alarm.Stop()`, the timer stops counting down.
// When the timer reaches zero, it stops there until the bomb is armed with `Alarm.Arm()`.
// If the bomb is armed, it "goes off".
// The timer will restart/rearm/remove the bomb according to its settings, then trigger the callback.
// The callback then can decide to override the automatic behavior.
public class Timer : TimerBase
{
    List<AlarmInfo> _alarms = new List<AlarmInfo>();


    protected override void Tick(float deltaTime)
    {
        float multDeltaTime = deltaTime * RateMultiplier;
        foreach (var alarm in _alarms)
        {
            TickAlarm(alarm, multDeltaTime);
        }
        _alarms.RemoveAll((alarm) => alarm.MarkedForRemoval);
        base.Tick(deltaTime);
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

            if (alarm.DestroyAfterTriggered)
                alarm.MarkedForRemoval = true;
            else
            {
                alarm.IsStarted = alarm.AutoRestart;
                alarm.IsArmed = alarm.AutoRearm;
            }
            alarm.Callback?.Invoke();

            if (alarm.IsStarted)
                // Add the underflow to the remaining cooldown, as the alarm must've kept ticking after triggering.
                alarm.RemainingCooldown += underflow;
        }
    }

    public override bool StartAlarm(AlarmInfoBase alarmBase)
    {
        AlarmInfo alarm = alarmBase as AlarmInfo;
        if (alarm == null)
            throw new Exception();
        if (_alarms.Find(x => alarm == x) == null)
            throw new Exception();
        if (alarm.MarkedForRemoval)
            return false;
        alarm.IsStarted = true;
        return true;
    }

    public override bool StopAlarm(AlarmInfoBase alarmBase)
    {
        AlarmInfo alarm = alarmBase as AlarmInfo;
        if (alarm == null)
            throw new Exception();
        if (_alarms.Find(x => alarm == x) == null)
            throw new Exception();
        if (alarm.MarkedForRemoval)
            return false;
        alarm.IsStarted = false;
        return true;
    }

    public override bool ResetAlarm(AlarmInfoBase alarmBase, float newCooldown)
    {
        AlarmInfo alarm = alarmBase as AlarmInfo;
        if (alarm == null)
            throw new Exception();
        if (_alarms.Find(x => alarm == x) == null)
            throw new Exception();
        if (alarm.MarkedForRemoval)
            return false;
        alarm.RemainingCooldown = newCooldown;
        return true;
    }

    public override void RemoveAlarm(AlarmInfoBase alarmBase)
    {
        AlarmInfo alarm = alarmBase as AlarmInfo;
        if (alarm == null)
            throw new Exception();
        if (_alarms.Find(x => alarm == x) == null)
            throw new Exception();
        alarm.MarkedForRemoval = true;
        return;
    }

    public override bool ArmAlarm(AlarmInfoBase alarmBase)
    {
        AlarmInfo alarm = alarmBase as AlarmInfo;
        if (alarm == null)
            throw new Exception();
        if (_alarms.Find(x => alarm == x) == null)
            throw new Exception();
        if (alarm.MarkedForRemoval)
            return false;
        alarm.IsArmed = true;
        return true;
    }

    public override bool DisarmAlarm(AlarmInfoBase alarmBase)
    {
        AlarmInfo alarm = alarmBase as AlarmInfo;
        if (alarm == null)
            throw new Exception();
        if (_alarms.Find(x => alarm == x) == null)
            throw new Exception();
        if (alarm.MarkedForRemoval)
            return false;
        alarm.IsArmed = false;
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
        if (cooldown <= 0f)
        {
            Debug.Log("Attempted to create an alarm with non-positive cooldown.");
            throw new Exception();
        }


        AlarmInfo alarm = new AlarmInfo(
            cooldown: cooldown,
            remainingCooldown: initialCooldown,
            isStarted: startImmediately,
            isArmed: armImmediately,
            autoRearm: autoRearm,
            autoRestart: autoRestart,
            destroyAfterTriggered: destroyAfterTriggered,
            markedForRemoval: false,
            callback: callback
        );

        _alarms.Add(alarm);

        return new Alarm(this, alarm);
    }
}
