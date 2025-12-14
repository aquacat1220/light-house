namespace LightHouse
{
    using FishNet.Object;
    using UnityEngine;
    using Fn;
    using UnityEngine.InputSystem;

    public class PlayerCharacterInput : NetworkBehaviour
    {
        // Triggered when the move input changes. Argument holds the new input value.
        [SerializeField]
        Event<Vector2> _move;
        // Triggered when the look input changes. Argument holds the new input value.
        [SerializeField]
        Event<Vector2> _look;
        // Triggered when the primary action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _primary;
        // Triggered when the secondary action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _secondary;
        [SerializeField]
        Event<bool> _action1;
        [SerializeField]
        Event<bool> _action2;
        [SerializeField]
        Event<bool> _action3;
        [SerializeField]
        Event<bool> _swapItem;
        [SerializeField]
        Event<bool> _swapToBackup1;
        [SerializeField]
        Event<bool> _swapToBackup2;
        [SerializeField]
        Event<bool> _dropItem;

        bool _isSubscribedToInputManager = false;

        public override void OnStartClient()
        {
            if (base.isActiveAndEnabled && base.IsOwner)
            {
                // We are the owning client of this character. Inputs should be passed down.
                SubscribeToInputManager();
            }
        }

        public override void OnStopClient()
        {
            // We don't check for ownership here, since calling `UnsubscribeFromInputManager()` when we are not subscribed shouldn't cause any problems.
            UnsubscribeFromInputManager();
        }

        void OnEnable()
        {
            if (base.IsClientInitialized && base.IsOwner)
            {
                // We are the owning client of this character. Inputs should be passed down.
                SubscribeToInputManager();
            }
        }

        void OnDisable()
        {
            // We don't check for ownership here, since calling `UnsubscribeFromInputManager()` when we are not subscribed shouldn't cause any problems.
            UnsubscribeFromInputManager();
        }

        void SubscribeToInputManager()
        {
            if (!_isSubscribedToInputManager)
            {
                InputManager.Singleton.InputActions.Gameplay.Move.performed += OnMove;
                InputManager.Singleton.InputActions.Gameplay.Move.canceled += OnMove;
                InputManager.Singleton.InputActions.Gameplay.Look.performed += OnLook;
                InputManager.Singleton.InputActions.Gameplay.Look.canceled += OnLook;
                InputManager.Singleton.InputActions.Gameplay.Primary.performed += OnPrimary;
                InputManager.Singleton.InputActions.Gameplay.Primary.canceled += OnPrimary;
                InputManager.Singleton.InputActions.Gameplay.Secondary.performed += OnSecondary;
                InputManager.Singleton.InputActions.Gameplay.Secondary.canceled += OnSecondary;
                InputManager.Singleton.InputActions.Gameplay.Action1.performed += OnAction1;
                InputManager.Singleton.InputActions.Gameplay.Action1.canceled += OnAction1;
                InputManager.Singleton.InputActions.Gameplay.Action2.performed += OnAction2;
                InputManager.Singleton.InputActions.Gameplay.Action2.canceled += OnAction2;
                InputManager.Singleton.InputActions.Gameplay.Action3.performed += OnAction3;
                InputManager.Singleton.InputActions.Gameplay.Action3.canceled += OnAction3;
                InputManager.Singleton.InputActions.Gameplay.SwapItem.performed += OnSwapItem;
                InputManager.Singleton.InputActions.Gameplay.SwapItem.canceled += OnSwapItem;
                InputManager.Singleton.InputActions.Gameplay.SwapToBackup1.performed += OnSwapToBackup1;
                InputManager.Singleton.InputActions.Gameplay.SwapToBackup1.canceled += OnSwapToBackup1;
                InputManager.Singleton.InputActions.Gameplay.SwapToBackup2.performed += OnSwapToBackup2;
                InputManager.Singleton.InputActions.Gameplay.SwapToBackup2.canceled += OnSwapToBackup2;
                InputManager.Singleton.InputActions.Gameplay.DropItem.performed += OnDropItem;
                InputManager.Singleton.InputActions.Gameplay.DropItem.canceled += OnDropItem;
                _isSubscribedToInputManager = true;
            }
        }

        void UnsubscribeFromInputManager()
        {
            if (_isSubscribedToInputManager)
            {
                InputManager.Singleton.InputActions.Gameplay.Move.performed -= OnMove;
                InputManager.Singleton.InputActions.Gameplay.Move.canceled -= OnMove;
                InputManager.Singleton.InputActions.Gameplay.Look.performed -= OnLook;
                InputManager.Singleton.InputActions.Gameplay.Look.canceled -= OnLook;
                InputManager.Singleton.InputActions.Gameplay.Primary.performed -= OnPrimary;
                InputManager.Singleton.InputActions.Gameplay.Primary.canceled -= OnPrimary;
                InputManager.Singleton.InputActions.Gameplay.Secondary.performed -= OnSecondary;
                InputManager.Singleton.InputActions.Gameplay.Secondary.canceled -= OnSecondary;
                InputManager.Singleton.InputActions.Gameplay.Action1.performed -= OnAction1;
                InputManager.Singleton.InputActions.Gameplay.Action1.canceled -= OnAction1;
                InputManager.Singleton.InputActions.Gameplay.Action2.performed -= OnAction2;
                InputManager.Singleton.InputActions.Gameplay.Action2.canceled -= OnAction2;
                InputManager.Singleton.InputActions.Gameplay.Action3.performed -= OnAction3;
                InputManager.Singleton.InputActions.Gameplay.Action3.canceled -= OnAction3;
                InputManager.Singleton.InputActions.Gameplay.SwapItem.performed -= OnSwapItem;
                InputManager.Singleton.InputActions.Gameplay.SwapItem.canceled -= OnSwapItem;
                InputManager.Singleton.InputActions.Gameplay.SwapToBackup1.performed -= OnSwapToBackup1;
                InputManager.Singleton.InputActions.Gameplay.SwapToBackup1.canceled -= OnSwapToBackup1;
                InputManager.Singleton.InputActions.Gameplay.SwapToBackup2.performed -= OnSwapToBackup2;
                InputManager.Singleton.InputActions.Gameplay.SwapToBackup2.canceled -= OnSwapToBackup2;
                InputManager.Singleton.InputActions.Gameplay.DropItem.performed -= OnDropItem;
                InputManager.Singleton.InputActions.Gameplay.DropItem.canceled -= OnDropItem;
                _isSubscribedToInputManager = false;
            }
        }

        void OnMove(InputAction.CallbackContext context)
        {
            _move?.Invoke(context.ReadValue<Vector2>());
        }

        void OnLook(InputAction.CallbackContext context)
        {
            _look?.Invoke(context.ReadValue<Vector2>());
        }

        void OnPrimary(InputAction.CallbackContext context)
        {
            if (context.performed)
                _primary?.Invoke(true);
            else if (context.canceled)
                _primary?.Invoke(false);
        }

        void OnSecondary(InputAction.CallbackContext context)
        {
            if (context.performed)
                _secondary?.Invoke(true);
            else if (context.canceled)
                _secondary?.Invoke(false);
        }

        void OnAction1(InputAction.CallbackContext context)
        {
            if (context.performed)
                _action1?.Invoke(true);
            else if (context.canceled)
                _action1?.Invoke(false);
        }

        void OnAction2(InputAction.CallbackContext context)
        {
            if (context.performed)
                _action2?.Invoke(true);
            else if (context.canceled)
                _action2?.Invoke(false);
        }

        void OnAction3(InputAction.CallbackContext context)
        {
            if (context.performed)
                _action3?.Invoke(true);
            else if (context.canceled)
                _action3?.Invoke(false);
        }

        void OnSwapItem(InputAction.CallbackContext context)
        {
            if (context.performed)
                _swapItem?.Invoke(true);
            else if (context.canceled)
                _swapItem?.Invoke(false);
        }

        void OnSwapToBackup1(InputAction.CallbackContext context)
        {
            if (context.performed)
                _swapToBackup1?.Invoke(true);
            else if (context.canceled)
                _swapToBackup1?.Invoke(false);
        }

        void OnSwapToBackup2(InputAction.CallbackContext context)
        {
            if (context.performed)
                _swapToBackup2?.Invoke(true);
            else if (context.canceled)
                _swapToBackup2?.Invoke(false);
        }

        void OnDropItem(InputAction.CallbackContext context)
        {
            if (context.performed)
                _dropItem?.Invoke(true);
            else if (context.canceled)
                _dropItem?.Invoke(false);
        }
    }
}
