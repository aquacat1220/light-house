namespace LightHouse
{
    using System;
    using System.Collections.Generic;
    using NaughtyAttributes;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class HudManager : MonoBehaviour
    {
        public static HudManager Singleton { get; private set; }
        [SerializeField]
        [Required]
        UIDocument _hudDocument;

        [SerializeField]
        [Required]
        VisualTreeAsset _hudRoot;

        VisualElement _hudContainer;

        HashSet<HudHandle> _huds = new();

        public class HudHandle
        {
            public TemplateContainer Hud { get; private set; }
            public HudHandle(TemplateContainer menu)
            {
                Hud = menu;
            }
        }

        public HudHandle AddMenu(VisualTreeAsset hud)
        {
            var hudInstance = hud.Instantiate();
            var handle = new HudHandle(hudInstance);

            hudInstance.AddToClassList("hud-root__hud");
            _hudContainer.Add(hudInstance);

            _huds.Add(handle);
            return handle;
        }

        public void RemoveMenu(HudHandle handle)
        {
            if (_huds.Remove(handle))
                handle.Hud.RemoveFromHierarchy();
        }

        void Awake()
        {
            _hudDocument.visualTreeAsset = _hudRoot;
            _hudContainer = _hudDocument?.rootVisualElement?.Q(className: "hud-root");
            if (_hudContainer == null)
            {
                Debug.Log("`_hudRoot` does not have a .hud-root.");
                throw new Exception();
            }

            if (Singleton != null)
            {
                Debug.Log("`Singleton` was non-null, implying there are multiple instances of `MenuManager`s in this scene.");
                throw new Exception();
            }
            Singleton = this;
        }
    }
}
