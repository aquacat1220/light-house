namespace LightHouse
{
    using System;
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
        Fn.Event<bool> _mouse;
        [SerializeField]
        GameObject _marker;

        bool mouseState = false;
        bool pulseState = false;

        [Serializable]
        public class OnPulseChangeFn : Fn.IFn<Fn.ITuple<bool>, Fn.Tuple>
        {
            public PulseInputTest PulseInputTest;
            public Fn.Tuple Invoke(Fn.ITuple<bool> param)
            {
                PulseInputTest.OnPulseChange(param.Item1);
                return Fn.Tuple.Unit;
            }
        }

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
            _mouse.Invoke(mouseState);
        }

        void OnRelease(InputAction.CallbackContext ctx)
        {
            mouseState = false;
            _mouseRelease?.Invoke();
            _mouse.Invoke(mouseState);
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
}
