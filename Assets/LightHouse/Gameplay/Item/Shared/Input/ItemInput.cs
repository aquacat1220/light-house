namespace LightHouse
{
    using UnityEngine;
    using Fn;
    using System;

    public class ItemInput : MonoBehaviour
    {
        [SerializeField]
        Item _item;

        public event Action<bool> Primary;
        public event Action<bool> Secondary;
        public event Action<bool> Action1;
        public event Action<bool> Action2;
        public event Action<bool> Action3;

        InputState<bool> _primaryState = new();
        InputState<bool> _secondaryState = new();
        InputState<bool> _action1State = new();
        InputState<bool> _action2State = new();
        InputState<bool> _action3State = new();

        void Awake()
        {
            if (_item == null)
            {
                Debug.Log("`_item` was not set.");
                throw new Exception();
            }
            _item.Register += OnRegister;
            _item.Unregister += OnUnregister;
            _primaryState.Change += OnPrimary;
            _secondaryState.Change += OnSecondary;
            _action1State.Change += OnAction1;
            _action2State.Change += OnAction2;
            _action3State.Change += OnAction3;
        }

        void OnDestroy()
        {
            _item.Register -= OnRegister;
            _item.Unregister -= OnUnregister;
            _primaryState.Change -= OnPrimary;
            _secondaryState.Change -= OnSecondary;
            _action1State.Change -= OnAction1;
            _action2State.Change -= OnAction2;
            _action3State.Change -= OnAction3;
        }

        void OnEnable()
        {
            _primaryState.Enable();
            _secondaryState.Enable();
            _action1State.Enable();
            _action2State.Enable();
            _action3State.Enable();
        }

        void OnDisable()
        {
            _primaryState.Disable();
            _secondaryState.Disable();
            _action1State.Disable();
            _action2State.Disable();
            _action3State.Disable();
        }

        void OnRegister(ItemSlot itemSlot)
        {
            var itemSlotInput = itemSlot.GetComponent<ItemSlotInput>();
            if (itemSlotInput == null)
            {
                Debug.Log("`itemSlot` doesn't have an `ItemSlotInput` component during registering. Is this normal?");
                return;
            }

            _primaryState.Parent = itemSlotInput.PrimaryState;
            _secondaryState.Parent = itemSlotInput.SecondaryState;
            _action1State.Parent = itemSlotInput.Action1State;
            _action2State.Parent = itemSlotInput.Action2State;
            _action3State.Parent = itemSlotInput.Action3State;
        }

        void OnUnregister()
        {
            _primaryState.Parent = null;
            _secondaryState.Parent = null;
            _action1State.Parent = null;
            _action2State.Parent = null;
            _action3State.Parent = null;
        }

        void OnPrimary(bool newState)
        {
            Primary?.Invoke(newState);
        }

        void OnSecondary(bool newState)
        {
            Secondary?.Invoke(newState);
        }

        void OnAction1(bool newState)
        {
            Action1?.Invoke(newState);
        }

        void OnAction2(bool newState)
        {
            Action2?.Invoke(newState);
        }

        void OnAction3(bool newState)
        {
            Action3?.Invoke(newState);
        }
    }
}
