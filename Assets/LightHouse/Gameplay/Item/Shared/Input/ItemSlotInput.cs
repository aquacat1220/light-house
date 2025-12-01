namespace LightHouse
{
    using UnityEngine;

    public class ItemSlotInput : MonoBehaviour
    {
        public InputState<bool> PrimaryState = new();
        public InputState<bool> SecondaryState = new();
        public InputState<bool> Action1State = new();
        public InputState<bool> Action2State = new();
        public InputState<bool> Action3State = new();

        void OnEnable()
        {
            PrimaryState.Enable();
            SecondaryState.Enable();
            Action1State.Enable();
            Action2State.Enable();
            Action3State.Enable();
        }

        void OnDisable()
        {
            PrimaryState.Disable();
            SecondaryState.Disable();
            Action1State.Disable();
            Action2State.Disable();
            Action3State.Disable();
        }
    }
}
