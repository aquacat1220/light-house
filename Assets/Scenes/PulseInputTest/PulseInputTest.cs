using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PulseInputTest : MonoBehaviour
{
    [SerializeField]
    UnityEvent _mousePress;
    [SerializeField]
    UnityEvent _mouseRelease;
    [SerializeField]
    GameObject _marker;

    bool mouseState = false;
    bool pulseState = false;

    public void OnPulseChange(bool isUp)
    {
        pulseState = isUp;
        var pulseMarker = Instantiate(_marker);
        if (pulseState)
            pulseMarker.transform.position = Camera.main.transform.position - Vector3.up * 1f + Vector3.forward * 10f;
        else
            pulseMarker.transform.position = Camera.main.transform.position - Vector3.up * 2f + Vector3.forward * 10f;
    }

    void Awake()
    {
        InputManager.Singleton.InputActions.Player.Primary.performed += OnClick;
        InputManager.Singleton.InputActions.Player.Primary.canceled += OnRelease;
    }

    void OnClick(InputAction.CallbackContext ctx)
    {
        mouseState = true;
        _mousePress?.Invoke();
    }

    void OnRelease(InputAction.CallbackContext ctx)
    {
        mouseState = false;
        _mouseRelease?.Invoke();
    }

    void Update()
    {
        Camera.main.transform.Translate(Vector3.right * Time.deltaTime);

        var mouseMarker = Instantiate(_marker);
        if (mouseState)
            mouseMarker.transform.position = Camera.main.transform.position + Vector3.up * 2f + Vector3.forward * 10f;
        else
            mouseMarker.transform.position = Camera.main.transform.position + Vector3.up * 1f + Vector3.forward * 10f;

        var pulseMarker = Instantiate(_marker);
        if (pulseState)
            pulseMarker.transform.position = Camera.main.transform.position - Vector3.up * 1f + Vector3.forward * 10f;
        else
            pulseMarker.transform.position = Camera.main.transform.position - Vector3.up * 2f + Vector3.forward * 10f;

    }
}
