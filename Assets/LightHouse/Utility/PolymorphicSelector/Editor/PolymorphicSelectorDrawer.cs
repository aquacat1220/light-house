namespace LightHouse
{
    using System.Linq;
    using Unity.VisualScripting;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(PolymorphicSelectorAttribute))]
    public class PolymorphicSelectorDrawer : PropertyDrawer
    {
        static VisualTreeAsset _polymorphicSelectorDrawerTemplate;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.hasMultipleDifferentValues)
            {
                var textField = new TextField("Multi-object editing is not possible with this drawer.");
                textField.enabledSelf = false;
                return textField;
            }
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                var textField = new TextField("PolymorphicSelectorDrawers are for `[SerializeReference]` fields.");
                textField.enabledSelf = false;
                return textField;
            }

            if (_polymorphicSelectorDrawerTemplate == null)
                _polymorphicSelectorDrawerTemplate = Resources.Load<VisualTreeAsset>("PolymorphicSelectorDrawerTemplate");
            var drawer = _polymorphicSelectorDrawerTemplate.Instantiate();

            var typePopup = drawer.Q<TypePopup>(className: "polymorphic-selector-drawer__type-popup");
            var propertyField = drawer.Q<PropertyField>(className: "polymorphic-selector-drawer__property-field");

            if (property.managedReferenceValue != null)
                typePopup.Popup.value = property.managedReferenceValue.GetType().CSharpFullName();
            else
                typePopup.Popup.value = "Undetermined";

            return drawer;
        }
    }
}