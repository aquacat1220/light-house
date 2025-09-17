using UnityEngine;
using UnityEngine.InputSystem;

public class Stopwatch : MonoBehaviour
{
    [SerializeField]
    Timer _timer;
    [SerializeField]
    bool _isRecurrent;
    [SerializeField]
    bool _destroyAfterTriggered;

    TimerHandle _handle;

    void Awake()
    {
        _handle = _timer.AddAlarm(1f, () => Debug.Log($"{Time.time}: {gameObject.name} triggered!"), _isRecurrent, _destroyAfterTriggered);
    }

    void Update()
    {
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            _handle.Activate();
            Debug.Log($"{Time.time}: {gameObject.name} activated!");
        }
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            _handle.Deactivate();
            Debug.Log($"{Time.time}: {gameObject.name} deactivated!");
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            _handle.Remove();
            Debug.Log($"{Time.time}: {gameObject.name} removed!");
        }
    }
}
