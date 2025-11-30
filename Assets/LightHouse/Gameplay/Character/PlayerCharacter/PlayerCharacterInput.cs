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
        Event<bool> _itemAction1;
        [SerializeField]
        Event<bool> _itemAction2;
        [SerializeField]
        Event<bool> _reload;
        // Triggered when the select item 1 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _selectItem1;
        // Triggered when the drop item 1 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _dropItem1;
        // Triggered when the select item 2 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _selectItem2;
        // Triggered when the drop item 2 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _dropItem2;
        // Triggered when the select item 3 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _selectItem3;
        // Triggered when the drop item 3 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _dropItem3;
        // Triggered when the select item 4 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _selectItem4;
        // Triggered when the drop item 4 action is performed or canceled. Argument is `true` when the action is performed, `false` when canceled.
        [SerializeField]
        Event<bool> _dropItem4;

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
                InputManager.Singleton.InputActions.Player.Move.performed += OnMove;
                InputManager.Singleton.InputActions.Player.Move.canceled += OnMove;
                InputManager.Singleton.InputActions.Player.Look.performed += OnLook;
                InputManager.Singleton.InputActions.Player.Look.canceled += OnLook;
                InputManager.Singleton.InputActions.Player.Primary.performed += OnPrimary;
                InputManager.Singleton.InputActions.Player.Primary.canceled += OnPrimary;
                InputManager.Singleton.InputActions.Player.Secondary.performed += OnSecondary;
                InputManager.Singleton.InputActions.Player.Secondary.canceled += OnSecondary;
                InputManager.Singleton.InputActions.Player.ItemAction1.performed += OnItemAction1;
                InputManager.Singleton.InputActions.Player.ItemAction1.canceled += OnItemAction1;
                InputManager.Singleton.InputActions.Player.ItemAction2.performed += OnItemAction2;
                InputManager.Singleton.InputActions.Player.ItemAction2.canceled += OnItemAction2;
                InputManager.Singleton.InputActions.Player.Reload.performed += OnReload;
                InputManager.Singleton.InputActions.Player.Reload.canceled += OnReload;
                InputManager.Singleton.InputActions.Player.SelectItem1.performed += OnSelectItem1;
                InputManager.Singleton.InputActions.Player.SelectItem1.canceled += OnSelectItem1;
                InputManager.Singleton.InputActions.Player.DropItem1.performed += OnDropItem1;
                InputManager.Singleton.InputActions.Player.DropItem1.canceled += OnDropItem1;
                InputManager.Singleton.InputActions.Player.SelectItem2.performed += OnSelectItem2;
                InputManager.Singleton.InputActions.Player.SelectItem2.canceled += OnSelectItem2;
                InputManager.Singleton.InputActions.Player.DropItem2.performed += OnDropItem2;
                InputManager.Singleton.InputActions.Player.DropItem2.canceled += OnDropItem2;
                InputManager.Singleton.InputActions.Player.SelectItem3.performed += OnSelectItem3;
                InputManager.Singleton.InputActions.Player.SelectItem3.canceled += OnSelectItem3;
                InputManager.Singleton.InputActions.Player.DropItem3.performed += OnDropItem3;
                InputManager.Singleton.InputActions.Player.DropItem3.canceled += OnDropItem3;
                InputManager.Singleton.InputActions.Player.SelectItem4.performed += OnSelectItem4;
                InputManager.Singleton.InputActions.Player.SelectItem4.canceled += OnSelectItem4;
                InputManager.Singleton.InputActions.Player.DropItem4.performed += OnDropItem4;
                InputManager.Singleton.InputActions.Player.DropItem4.canceled += OnDropItem4;
                _isSubscribedToInputManager = true;
            }
        }

        void UnsubscribeFromInputManager()
        {
            if (_isSubscribedToInputManager)
            {
                InputManager.Singleton.InputActions.Player.Move.performed -= OnMove;
                InputManager.Singleton.InputActions.Player.Move.canceled -= OnMove;
                InputManager.Singleton.InputActions.Player.Look.performed -= OnLook;
                InputManager.Singleton.InputActions.Player.Look.canceled -= OnLook;
                InputManager.Singleton.InputActions.Player.Primary.performed -= OnPrimary;
                InputManager.Singleton.InputActions.Player.Primary.canceled -= OnPrimary;
                InputManager.Singleton.InputActions.Player.Secondary.performed -= OnSecondary;
                InputManager.Singleton.InputActions.Player.Secondary.canceled -= OnSecondary;
                InputManager.Singleton.InputActions.Player.ItemAction1.performed -= OnItemAction1;
                InputManager.Singleton.InputActions.Player.ItemAction1.canceled -= OnItemAction1;
                InputManager.Singleton.InputActions.Player.ItemAction2.performed -= OnItemAction2;
                InputManager.Singleton.InputActions.Player.ItemAction2.canceled -= OnItemAction2;
                InputManager.Singleton.InputActions.Player.Reload.performed -= OnReload;
                InputManager.Singleton.InputActions.Player.Reload.canceled -= OnReload;
                InputManager.Singleton.InputActions.Player.SelectItem1.performed -= OnSelectItem1;
                InputManager.Singleton.InputActions.Player.SelectItem1.canceled -= OnSelectItem1;
                InputManager.Singleton.InputActions.Player.DropItem1.performed -= OnDropItem1;
                InputManager.Singleton.InputActions.Player.DropItem1.canceled -= OnDropItem1;
                InputManager.Singleton.InputActions.Player.SelectItem2.performed -= OnSelectItem2;
                InputManager.Singleton.InputActions.Player.SelectItem2.canceled -= OnSelectItem2;
                InputManager.Singleton.InputActions.Player.DropItem2.performed -= OnDropItem2;
                InputManager.Singleton.InputActions.Player.DropItem2.canceled -= OnDropItem2;
                InputManager.Singleton.InputActions.Player.SelectItem3.performed -= OnSelectItem3;
                InputManager.Singleton.InputActions.Player.SelectItem3.canceled -= OnSelectItem3;
                InputManager.Singleton.InputActions.Player.DropItem3.performed -= OnDropItem3;
                InputManager.Singleton.InputActions.Player.DropItem3.canceled -= OnDropItem3;
                InputManager.Singleton.InputActions.Player.SelectItem4.performed -= OnSelectItem4;
                InputManager.Singleton.InputActions.Player.SelectItem4.canceled -= OnSelectItem4;
                InputManager.Singleton.InputActions.Player.DropItem4.performed -= OnDropItem4;
                InputManager.Singleton.InputActions.Player.DropItem4.canceled -= OnDropItem4;
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

        void OnItemAction1(InputAction.CallbackContext context)
        {
            if (context.performed)
                _itemAction1?.Invoke(true);
            else if (context.canceled)
                _itemAction1?.Invoke(false);
        }

        void OnItemAction2(InputAction.CallbackContext context)
        {
            if (context.performed)
                _itemAction2?.Invoke(true);
            else if (context.canceled)
                _itemAction2?.Invoke(false);
        }

        void OnReload(InputAction.CallbackContext context)
        {
            if (context.performed)
                _reload?.Invoke(true);
            else if (context.canceled)
                _reload?.Invoke(false);
        }

        void OnSelectItem1(InputAction.CallbackContext context)
        {
            if (context.performed)
                _selectItem1?.Invoke(true);
            else if (context.canceled)
                _selectItem1?.Invoke(false);
        }

        void OnDropItem1(InputAction.CallbackContext context)
        {
            if (context.performed)
                _dropItem1?.Invoke(true);
            else if (context.canceled)
                _dropItem1?.Invoke(false);
        }

        void OnSelectItem2(InputAction.CallbackContext context)
        {
            if (context.performed)
                _selectItem2?.Invoke(true);
            else if (context.canceled)
                _selectItem2?.Invoke(false);
        }

        void OnDropItem2(InputAction.CallbackContext context)
        {
            if (context.performed)
                _dropItem2?.Invoke(true);
            else if (context.canceled)
                _dropItem2?.Invoke(false);
        }

        void OnSelectItem3(InputAction.CallbackContext context)
        {
            if (context.performed)
                _selectItem3?.Invoke(true);
            else if (context.canceled)
                _selectItem3?.Invoke(false);
        }

        void OnDropItem3(InputAction.CallbackContext context)
        {
            if (context.performed)
                _dropItem3?.Invoke(true);
            else if (context.canceled)
                _dropItem3?.Invoke(false);
        }

        void OnSelectItem4(InputAction.CallbackContext context)
        {
            if (context.performed)
                _selectItem4?.Invoke(true);
            else if (context.canceled)
                _selectItem4?.Invoke(false);
        }

        void OnDropItem4(InputAction.CallbackContext context)
        {
            if (context.performed)
                _dropItem4?.Invoke(true);
            else if (context.canceled)
                _dropItem4?.Invoke(false);
        }
    }
}
