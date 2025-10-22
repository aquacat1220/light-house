using UnityEngine;
using UnityEngine.InputSystem;

public class MovingLight : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.wKey.isPressed)
        {
            transform.Translate(Vector3.up * 0.05f);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            transform.Translate(Vector3.up * (-0.05f));
        }
        if (Keyboard.current.dKey.isPressed)
        {
            transform.Translate(Vector3.right * 0.05f);
        }
        if (Keyboard.current.aKey.isPressed)
        {
            transform.Translate(Vector3.right * (-0.05f));
        }
    }
}
