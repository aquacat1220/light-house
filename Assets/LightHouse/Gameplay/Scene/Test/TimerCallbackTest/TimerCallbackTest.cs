using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class TimerCallbackTest : MonoBehaviour
{
    Alarm _alarm;
    bool isStarted = true;
    bool isArmed = true;

    void Awake()
    {
        _alarm = TimerManager.Singleton.AddAlarm(
            cooldown: 1f,
            callback: Callback,
            initialCooldown: 1f
        );
    }

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (isStarted)
            {
                isStarted = false;
                Debug.Log($"{Time.time}: isStarted: {isStarted}.");
            }
            else
            {
                isStarted = true;
                Debug.Log($"{Time.time}: isStarted: {isStarted}.");
            }
        }
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            if (isArmed)
            {
                isArmed = false;
                Debug.Log($"{Time.time}: isArmed: {isArmed}.");
            }
            else
            {
                isArmed = true;
                Debug.Log($"{Time.time}: isArmed: {isArmed}.");
            }
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            _alarm.Start();
            isStarted = true;
            Debug.Log($"{Time.time}: isStarted: {isStarted}.");
            _alarm.Arm();
            isArmed = true;
            Debug.Log($"{Time.time}: isArmed: {isArmed}.");
        }
    }

    void Callback()
    {
        if (!isStarted)
        {
            var result = _alarm.Stop();
            Assert.IsTrue(result);
        }
        if (!isArmed)
        {
            var result = _alarm.Disarm();
            Assert.IsTrue(result);
        }
        Debug.Log($"{Time.time}: Callback triggered.");
    }
}
