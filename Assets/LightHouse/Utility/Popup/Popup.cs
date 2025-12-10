namespace LightHouse
{
    using System;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;

    // Define the custom control type.
    [UxmlElement]
    public partial class Popup : BindableElement, INotifyValueChanged<string>
    {
        static StyleSheet StyleSheet;
        Label _label;
        VisualElement _container;
        VisualElement _topbar;

        string _value = "Popup Default Text";
        [UxmlAttribute, CreateProperty]
        public string value
        {
            get => _value;
            set
            {
                if (value == _value)
                    return;

                var previous = _value;
                SetValueWithoutNotify(value);

                using (var evt = ChangeEvent<string>.GetPooled(previous, value))
                {
                    evt.target = this;
                    SendEvent(evt);
                }
            }
        }

        public void SetValueWithoutNotify(string newValue)
        {
            _value = newValue;
            _label.text = _value;
        }

        bool _popupOpen = false;

        bool _wantPopupOpen = false;
        [UxmlAttribute, CreateProperty]
        public bool WantPopupOpen
        {
            get => _wantPopupOpen;
            set
            {
                _wantPopupOpen = value;
                if (_wantPopupOpen)
                    OpenPopup();
                else
                    ClosePopup();
            }
        }

        bool _useThisAsContainer = false;

        bool _togglePopupOnClick = true;
        [UxmlAttribute, CreateProperty]
        public bool TogglePopupOnClick
        {
            get => _togglePopupOnClick;
            set
            {
                _togglePopupOnClick = value;
            }
        }

        bool _draggable = false;
        [UxmlAttribute, CreateProperty]
        public bool Draggable
        {
            get => _draggable;
            set
            {
                _draggable = value;
            }
        }

        IVisualElementScheduledItem _scheduledExecute;

        public event Action Clicked;

        public Popup()
        {
            _useThisAsContainer = true;
            if (StyleSheet == null)
                StyleSheet = Resources.Load<StyleSheet>("Popup");
            this.styleSheets.Add(StyleSheet);
            this.AddToClassList("popup");

            _label = new Label();
            _label.AddToClassList("popup__label");
            _label.text = value;
            this.Add(_label);

            var icon = new VisualElement();
            icon.AddToClassList("popup__icon");
            this.Add(icon);

            _container = new VisualElement();
            _container.AddToClassList("popup__container");
            _container.AddToClassList("popup__container--closed");
            this.Add(_container);
            _useThisAsContainer = false;

            _topbar = new VisualElement();
            _topbar.AddToClassList("popup__container-topbar");
            _container.Add(_topbar);

            var closeIcon = new VisualElement();
            closeIcon.AddToClassList("popup__container-close-icon");
            _topbar.Add(closeIcon);

            Clickable clickable = new Clickable(OnClick);
            _label.AddManipulator(clickable);

            _topbar.RegisterCallback<PointerDownEvent>(OnContainerDragStart);
            _topbar.RegisterCallback<PointerUpEvent>(OnContainerDragEnd);
            _topbar.RegisterCallback<PointerMoveEvent>(OnContainerDrag);

            Clickable closeIconClickable = new Clickable(() => WantPopupOpen = false);
            closeIcon.AddManipulator(closeIconClickable);

            this.RegisterCallback<AttachToPanelEvent>(evt => OnAttachToPanel());
            this.RegisterCallback<DetachFromPanelEvent>(evt => OnDetachFromPanel());
        }

        public override VisualElement contentContainer
        {
            get
            {
                if (_useThisAsContainer)
                    return this;
                return _container;
            }
        }

        // Check if we can open the popup, and open it if so.
        void OpenPopup()
        {
            if (_scheduledExecute != null)
            {
                _scheduledExecute.Pause();
                _scheduledExecute = null;
            }
            // Popup is already opened.
            if (_popupOpen)
                return;
            // The panel is null. We can't add the popup to anywhere.
            if (this.panel == null)
                return;
            _scheduledExecute = schedule.Execute(() =>
                {
                    _scheduledExecute = null;
                    // The panel is null. We can't add the popup to anywhere.
                    if (this.panel == null)
                        return;
                    _container.RemoveFromClassList("popup__container--closed");
                    _container.AddToClassList("popup__container--opened");
                    // We are connected to a valid panel. Move the popup container to panel root.
                    this.panel.visualTree.Add(_container);
                    var worldBottomLeft = VisualElementExtensions.LocalToWorld(this, new Vector2(0f, this.resolvedStyle.height));
                    _container.style.top = worldBottomLeft.y;
                    _container.style.left = worldBottomLeft.x;
                    _popupOpen = true;
                }
            ).StartingIn(50);
        }

        // Close the popup.
        void ClosePopup()
        {
            if (_scheduledExecute != null)
            {
                _scheduledExecute.Pause();
                _scheduledExecute = null;
            }
            // Popup is already closed.
            if (!_popupOpen)
                return;
            // No need to check the panel.
            _container.RemoveFromClassList("popup__container--opened");
            _container.AddToClassList("popup__container--closed");
            _useThisAsContainer = true;
            // For some reason, line 120 will throw an error on a non-existant visualtree. The below check should prevent it from happening.
            if (this.panel.visualTree.IndexOf(_container) < 0)
                return;
            this.Add(_container);
            _useThisAsContainer = false;
            _container.style.top = StyleKeyword.Null;
            _container.style.left = StyleKeyword.Null;
            _popupOpen = false;
        }

        void OnAttachToPanel()
        {
            // The container will be moving alot, and we want styles from ancestors to propagate correctly.
            // Ideally we would propagate stylesheets when `this` detects hierarchy change, but we don't have that yet.
            // So we use the attachtopanel event instead; elements aren't going to move anyways... so it should be fine.
            VisualElement parent = this;
            while (parent != null)
            {
                for (int i = 0; i < parent.styleSheets.count; i++)
                    _container.styleSheets.Add(parent.styleSheets[i]);

                parent = parent.parent;
            }
            // If there is a blocked popup-open, do that now.
            if (WantPopupOpen)
                OpenPopup();
        }

        void OnDetachFromPanel()
        {
            // We can safely clear all stylesheets because we know stylesheets are going to be attached later.
            _container.styleSheets.Clear();
            // Popups can't be opened when we don't have a panel.
            ClosePopup();
        }

        void OnClick(EventBase evt)
        {
            Clicked?.Invoke();
            if (!TogglePopupOnClick)
                return;
            WantPopupOpen = !WantPopupOpen;
        }

        void OnContainerDragStart(PointerDownEvent evt)
        {
            if (evt.target == _topbar)
                _topbar.CapturePointer(evt.pointerId);
        }

        void OnContainerDragEnd(PointerUpEvent evt)
        {
            if (evt.target == _topbar)
                _topbar.ReleasePointer(evt.pointerId);
        }

        void OnContainerDrag(PointerMoveEvent evt)
        {
            if (evt.target != evt.currentTarget || !evt.target.HasPointerCapture(evt.pointerId))
                return;
            if (!_popupOpen)
                return;
            _container.style.top = _container.style.top.value.value + evt.deltaPosition.y;
            _container.style.left = _container.style.left.value.value + evt.deltaPosition.x;
        }
    }
}