using UnityEngine;

public class ItemSlotInput : MonoBehaviour
{
    public InputState<bool> PrimaryState = new();
    public InputState<bool> SecondaryState = new();
    public InputState<bool> Action1State = new();
    public InputState<bool> Action2State = new();
    public InputState<bool> ReloadState = new();

    void OnEnable()
    {
        PrimaryState.Enable();
        SecondaryState.Enable();
        Action1State.Enable();
        Action2State.Enable();
        ReloadState.Enable();
    }

    void OnDisable()
    {
        PrimaryState.Disable();
        SecondaryState.Disable();
        Action1State.Disable();
        Action2State.Disable();
        ReloadState.Disable();
    }
}
