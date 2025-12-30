namespace LightHouse
{
    using FishNet.Object;
    using UnityEngine;
    using Fn;
    using UnityEngine.InputSystem;
    using System;

    public class PlayerCharacterInput : NetworkBehaviour
    {
        // Triggered when the move input changes. Argument holds the new input value.
        public event Action<Vector2> Move;
        // Triggered when the look input changes. Argument holds the new input value.
        public event Action<Vector2> Look;
        // Triggered when the primary action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        public event Action<bool> Primary;
        // Triggered when the secondary action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        public event Action<bool> Secondary;
        public event Action<bool> Action1;
        public event Action<bool> Action2;
        public event Action<bool> Action3;
        public event Action<bool> SwapItem;
        public event Action<bool> SwapToBackup1;
        public event Action<bool> SwapToBackup2;
        public event Action<bool> DropItem;

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
            Move?.Invoke(context.ReadValue<Vector2>());
        }

        void OnLook(InputAction.CallbackContext context)
        {
            Look?.Invoke(context.ReadValue<Vector2>());
        }

        void OnPrimary(InputAction.CallbackContext context)
        {
            if (context.performed)
                Primary?.Invoke(true);
            else if (context.canceled)
                Primary?.Invoke(false);
        }

        void OnSecondary(InputAction.CallbackContext context)
        {
            if (context.performed)
                Secondary?.Invoke(true);
            else if (context.canceled)
                Secondary?.Invoke(false);
        }

        void OnAction1(InputAction.CallbackContext context)
        {
            if (context.performed)
                Action1?.Invoke(true);
            else if (context.canceled)
                Action1?.Invoke(false);
        }

        void OnAction2(InputAction.CallbackContext context)
        {
            if (context.performed)
                Action2?.Invoke(true);
            else if (context.canceled)
                Action2?.Invoke(false);
        }

        void OnAction3(InputAction.CallbackContext context)
        {
            if (context.performed)
                Action3?.Invoke(true);
            else if (context.canceled)
                Action3?.Invoke(false);
        }

        void OnSwapItem(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwapItem?.Invoke(true);
            else if (context.canceled)
                SwapItem?.Invoke(false);
        }

        void OnSwapToBackup1(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwapToBackup1?.Invoke(true);
            else if (context.canceled)
                SwapToBackup1?.Invoke(false);
        }

        void OnSwapToBackup2(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwapToBackup2?.Invoke(true);
            else if (context.canceled)
                SwapToBackup2?.Invoke(false);
        }

        void OnDropItem(InputAction.CallbackContext context)
        {
            if (context.performed)
                DropItem?.Invoke(true);
            else if (context.canceled)
                DropItem?.Invoke(false);
        }
    }
}
