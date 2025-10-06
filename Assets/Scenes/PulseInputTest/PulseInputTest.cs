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
    }

    void Update()
    {
        Camera.main.transform.Translate(Vector3.right * Time.deltaTime);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _mousePress?.Invoke();
            mouseState = true;
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _mouseRelease?.Invoke();
            mouseState = false;
        }

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
