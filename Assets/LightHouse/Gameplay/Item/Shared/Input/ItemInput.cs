namespace LightHouse
{
    using UnityEngine;
    using Fn;
    using System;

    public class ItemInput : MonoBehaviour
    {
        [SerializeField]
        Event<bool> _primary;
        [SerializeField]
        Event<bool> _secondary;
        [SerializeField]
        Event<bool> _action1;
        [SerializeField]
        Event<bool> _action2;
        [SerializeField]
        Event<bool> _reload;

        InputState<bool> _primaryState = new();
        InputState<bool> _secondaryState = new();
        InputState<bool> _action1State = new();
        InputState<bool> _action2State = new();
        InputState<bool> _reloadState = new();

        void Awake()
        {
            _primaryState.Change += OnPrimary;
            _secondaryState.Change += OnSecondary;
            _action1State.Change += OnAction1;
            _action2State.Change += OnAction2;
            _reloadState.Change += OnReload;
        }

        void OnDestroy()
        {
            _primaryState.Change -= OnPrimary;
            _secondaryState.Change -= OnSecondary;
            _action1State.Change -= OnAction1;
            _action2State.Change -= OnAction2;
            _reloadState.Change -= OnReload;
        }

        void OnEnable()
        {
            _primaryState.Enable();
            _secondaryState.Enable();
            _action1State.Enable();
            _action2State.Enable();
            _reloadState.Enable();
        }

        void OnDisable()
        {
            _primaryState.Disable();
            _secondaryState.Disable();
            _action1State.Disable();
            _action2State.Disable();
            _reloadState.Disable();
        }

        [Serializable]
        public class OnRegisterFn : IFn<ITuple<ItemSlot>, Fn.Tuple>
        {
            public ItemInput ItemInput;
            public Fn.Tuple Invoke(ITuple<ItemSlot> param)
            {
                ItemInput?.OnRegister(param.Item1);
                return Fn.Tuple.Unit;
            }
        }

        [Serializable]
        public class OnUnregisterFn : IFn<Fn.Tuple, Fn.Tuple>
        {
            public ItemInput ItemInput;
            public Fn.Tuple Invoke(Fn.Tuple _)
            {
                ItemInput?.OnUnregister();
                return Fn.Tuple.Unit;
            }
        }

        public void OnRegister(ItemSlot itemSlot)
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
            _reloadState.Parent = itemSlotInput.ReloadState;
        }

        public void OnUnregister()
        {
            _primaryState.Parent = null;
            _secondaryState.Parent = null;
            _action1State.Parent = null;
            _action2State.Parent = null;
            _reloadState.Parent = null;
        }

        void OnPrimary(bool newState)
        {
            _primary?.Invoke(newState);
        }

        void OnSecondary(bool newState)
        {
            _secondary?.Invoke(newState);
        }

        void OnAction1(bool newState)
        {
            _action1?.Invoke(newState);
        }

        void OnAction2(bool newState)
        {
            _action2?.Invoke(newState);
        }

        void OnReload(bool newState)
        {
            _reload?.Invoke(newState);
        }
    }
}
