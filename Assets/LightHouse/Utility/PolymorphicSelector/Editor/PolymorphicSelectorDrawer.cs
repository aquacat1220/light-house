namespace LightHouse
{
    using System;
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

            typePopup.Reset(
                new TypeConstraint.IConstraint[]
                {
                    new TypeConstraint.UpperBound(fieldInfo.FieldType),
                    new TypeConstraint.DefaultConstructor()
                },
                (type) => type.IsVisible && !type.IsAbstract && !type.IsValueType && !typeof(UnityEngine.Object).IsAssignableFrom(type) && Attribute.IsDefined(type, typeof(SerializableAttribute))
            );

            if (property.managedReferenceValue != null)
                typePopup.Popup.value = property.managedReferenceValue.GetType().CSharpName();
            else
                typePopup.Popup.value = "Undetermined";
            typePopup.TypeSelected += (type) =>
            {
                if (type == null)
                    return;

                // Start
                // Code from https://github.com/mackysoft/Unity-SerializeReferenceExtensions/blob/main/Assets/MackySoft/MackySoft.SerializeReferenceExtensions/Editor/ManagedReferenceUtility.cs
                object result = null;
                if (property.managedReferenceValue != null)
                {
                    string json = JsonUtility.ToJson(property.managedReferenceValue);
                    result = JsonUtility.FromJson(json, type);
                }
                if (result == null)
                    result = Activator.CreateInstance(type);

                property.managedReferenceValue = result;
                // End
                property.serializedObject.ApplyModifiedProperties();
            };
            propertyField.BindProperty(property);

            return drawer;
        }
    }
}