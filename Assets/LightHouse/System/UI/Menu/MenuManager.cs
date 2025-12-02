namespace LightHouse
{
    using System;
    using System.Collections.Generic;
    using NaughtyAttributes;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.UIElements;

    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Singleton { get; private set; }
        [SerializeField]
        [Required]
        UIDocument _menuDocument;

        [SerializeField]
        [Required]
        VisualTreeAsset _menuRoot;

        [SerializeField]
        bool _startVisible = true;

        bool _isVisible = true;
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (value && !_isVisible)
                {
                    InputManager.Singleton.InputMode = InputMode.UI;
                    _menuDocument.rootVisualElement.RemoveFromClassList("display-none");
                    _isVisible = value;
                }
                else if (!value && _isVisible)
                {
                    InputManager.Singleton.InputMode = InputMode.Gameplay;
                    _menuDocument.rootVisualElement.AddToClassList("display-none");
                    _isVisible = value;
                }
            }
        }

        // Is the component subscribed to `InputManager.Singleton.ShowMenuAction`?
        bool _isSubscribedToShowMenu = false;

        // Is the component listening for unhandled `Cancel` events?
        bool _isSubscribedToCancel = false;

        VisualElement _tab;
        VisualElement _menuContainer;

        Dictionary<MenuHandle, (Button TabButton, TemplateContainer Menu)> _menus = new();

        MenuHandle _selectedHandle = null;
        public MenuHandle SelectedHandle
        {
            get { return _selectedHandle; }
            private set
            {
                if (_selectedHandle == value)
                    return;

                if (_selectedHandle != null && _menus.ContainsKey(_selectedHandle))
                {
                    (var tabButton, var menu) = _menus[_selectedHandle];
                    _selectedHandle = null;
                    tabButton.RemoveFromClassList("menu-root__tab-button--selected");
                    menu.RemoveFromClassList("menu-root__menu--selected");
                    menu.AddToClassList("menu-root__menu--not-selected");
                }

                if (value != null && _menus.ContainsKey(value))
                {
                    (var tabButton, var menu) = _menus[value];
                    tabButton.RemoveFromClassList("menu-root__tab-button--hovered");
                    tabButton.AddToClassList("menu-root__tab-button--selected");
                    menu.RemoveFromClassList("menu-root__menu--not-selected");
                    menu.AddToClassList("menu-root__menu--selected");
                    _selectedHandle = value;
                }
            }
        }

        public class MenuHandle
        {
            public TemplateContainer Menu { get; private set; }
            public MenuHandle(TemplateContainer menu)
            {
                Menu = menu;
            }
        }

        public MenuHandle AddMenu(VisualTreeAsset menu, string name)
        {
            var tabButton = new Button();
            var menuInstance = menu.Instantiate();
            var handle = new MenuHandle(menuInstance);

            tabButton.text = name;
            tabButton.AddToClassList("menu-root__tab-button");
            tabButton.clicked += () => TabButtonClicked(handle);
            tabButton.RegisterCallback<PointerEnterEvent>((_) => TabButtonHoverStart(handle));
            tabButton.RegisterCallback<PointerLeaveEvent>((_) => TabButtonHoverEnd(handle));
            tabButton.RegisterCallback<FocusInEvent>((_) => TabButtonHoverStart(handle));
            tabButton.RegisterCallback<FocusOutEvent>((_) => TabButtonHoverEnd(handle));
            _tab.Add(tabButton);

            menuInstance.AddToClassList("menu-root__menu--not-selected");
            _menuContainer.Add(menuInstance);

            _menus.Add(handle, (tabButton, menuInstance));
            return handle;
        }

        public void RemoveMenu(MenuHandle handle)
        {
            if (_menus.Remove(handle, out var removed))
            {
                removed.TabButton.RemoveFromHierarchy();
                removed.Menu.RemoveFromHierarchy();
                if (SelectedHandle == handle)
                    SelectedHandle = null;
            }
        }

        void Awake()
        {
            _menuDocument.visualTreeAsset = _menuRoot;
            _tab = _menuDocument?.rootVisualElement?.Q(className: "menu-root__tab");
            _menuContainer = _menuDocument?.rootVisualElement?.Q(className: "menu-root__menu-container");
            if (_tab == null || _menuContainer == null)
            {
                Debug.Log("`_menuRoot` does not have a .menu-root__tab or a .menu-root__menu-container.");
                throw new Exception();
            }

            // Ensure UI visibility matches `_isVisible`'s initial value.
            // This is the LAST PART where we directly touch `_isVisible` without using the property `IsVisible`.
            if (_isVisible)
            {
                InputManager.Singleton.InputMode = InputMode.UI;
                _menuDocument.rootVisualElement.RemoveFromClassList("display-none");
            }
            else
            {
                InputManager.Singleton.InputMode = InputMode.Gameplay;
                _menuDocument.rootVisualElement.AddToClassList("display-none");
            }

            // Then make it follow the `_startVisible` value.
            IsVisible = _startVisible;

            if (Singleton != null)
            {
                Debug.Log("`Singleton` was non-null, implying there are multiple instances of `MenuManager`s in this scene.");
                throw new Exception();
            }
            Singleton = this;
        }

        void OnEnable()
        {
            if (!_isSubscribedToShowMenu)
            {
                InputManager.Singleton.InputActions.Gameplay.ShowMenu.performed += OnShowMenu;
                _isSubscribedToShowMenu = true;
            }

            if (!_isSubscribedToCancel)
            {
                InputManager.Singleton.InputActions.UI.Cancel.performed += OnCancel;
                _menuDocument.rootVisualElement.RegisterCallback<NavigationCancelEvent>(OnUnhandledCancel);
                _isSubscribedToCancel = true;
            }
        }

        void OnDisable()
        {
            if (_isSubscribedToShowMenu)
            {
                InputManager.Singleton.InputActions.Gameplay.ShowMenu.performed -= OnShowMenu;
                _isSubscribedToShowMenu = false;
            }

            if (_isSubscribedToCancel)
            {
                InputManager.Singleton.InputActions.UI.Cancel.performed -= OnCancel;
                // `?` is there to suppress an exception on game end.
                _menuDocument.rootVisualElement?.UnregisterCallback<NavigationCancelEvent>(OnUnhandledCancel);
                _isSubscribedToCancel = false;
            }
        }

        void OnShowMenu(InputAction.CallbackContext context)
        {
            IsVisible = true;
        }

        void OnCancel(InputAction.CallbackContext context)
        {
            IsVisible = false;
        }

        void OnUnhandledCancel(NavigationCancelEvent evt)
        {
            IsVisible = false;
            evt.StopPropagation();
        }

        void TabButtonClicked(MenuHandle handle)
        {
            SelectedHandle = handle;
        }

        void TabButtonHoverStart(MenuHandle handle)
        {
            if (!_menus.ContainsKey(handle))
            {
                Debug.Log("Callback triggered, but handle was not valid.");
                throw new Exception();
            }
            // We want the selected state to take precedence; no need to apply the hovered state if so.
            if (handle == SelectedHandle)
                return;

            (var tabButton, var _) = _menus[handle];
            tabButton.AddToClassList("menu-root__tab-button--hovered");
        }

        void TabButtonHoverEnd(MenuHandle handle)
        {
            if (!_menus.ContainsKey(handle))
            {
                Debug.Log("Callback triggered, but handle was not valid.");
                throw new Exception();
            }
            // Removing classes twice won't do any harm.

            (var tabButton, var _) = _menus[handle];
            tabButton.RemoveFromClassList("menu-root__tab-button--hovered");
        }
    }
}
