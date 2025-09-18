using UnityEngine;
using UnityEngine.InputSystem;

public class Stopwatch : MonoBehaviour
{
    [SerializeField]
    Timer _timer;
    [SerializeField]
    bool _autoEverything = false;

    TimerHandle _handle;

    void Awake()
    {
        _handle = _timer.AddAlarm(
                cooldown: 1f,
                callback: () => Debug.Log($"{Time.time}: {gameObject.name} triggered!"),
                startImmediately: _autoEverything,
                armImmediately: _autoEverything,
                autoRestart: _autoEverything,
                autoRearm: _autoEverything,
                initialCooldown: 0f,
                destroyAfterTriggered: false
            );
    }
    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            _handle.Start();
            Debug.Log($"{Time.time}: {gameObject.name} started!");
        }
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            _handle.Stop();
            Debug.Log($"{Time.time}: {gameObject.name} stopped!");
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            _handle.Arm();
            Debug.Log($"{Time.time}: {gameObject.name} armed!");
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            _handle.Disarm();
            Debug.Log($"{Time.time}: {gameObject.name} disarmed!");
        }
    }
}
