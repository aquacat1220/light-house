using UnityEngine;

public class ItemSlotInput : MonoBehaviour
{
    public InputState<bool> PrimaryState = new();
    public InputState<bool> SecondaryState = new();

    void OnEnable()
    {
        PrimaryState.Enable();
        SecondaryState.Enable();
    }

    void OnDisable()
    {
        PrimaryState.Disable();
        SecondaryState.Disable();
    }
}
