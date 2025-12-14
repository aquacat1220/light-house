namespace LightHouse
{
    using System;
    using System.Linq;
    using Unity.VisualScripting;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(PolySelectorAttribute))]
    public class PolySelectorDrawer : PropertyDrawer
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
                var textField = new TextField("PolySelectorDrawers are for `[SerializeReference]` fields.");
                textField.enabledSelf = false;
                return textField;
            }

            if (_polymorphicSelectorDrawerTemplate == null)
                _polymorphicSelectorDrawerTemplate = Resources.Load<VisualTreeAsset>("PolySelectorDrawerTemplate");
            var drawer = _polymorphicSelectorDrawerTemplate.Instantiate();

            var label = drawer.Q<Label>(className: "poly-selector-drawer__label");
            var typePopup = drawer.Q<TypePopup>(className: "poly-selector-drawer__type-popup");
            var drag = drawer.Q<VisualElement>(className: "poly-selector-drawer__drag");
            var propertyField = drawer.Q<PropertyField>(className: "poly-selector-drawer__property-field");

            label.text = preferredLabel;

            Type fieldType = TypeFromTypeName(property.managedReferenceFieldTypename);

            typePopup.Reset(
                new TypeConstraint.IConstraint[]
                {
                    new TypeConstraint.UpperBound(fieldType),
                    new TypeConstraint.DefaultConstructor()
                },
                (type) => type.IsVisible && !type.IsAbstract && !type.IsValueType && !typeof(UnityEngine.Object).IsAssignableFrom(type) && Attribute.IsDefined(type, typeof(SerializableAttribute))
            );

            // if (property.managedReferenceValue != null)
            //     typePopup.Popup.value = property.managedReferenceValue.GetType().CSharpName();
            // else
            //     typePopup.Popup.value = "Undetermined";
            typePopup.TypeSelected += (type) =>
            {
                if (type == null)
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    return;
                }

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

            drag.userData = property;
            drag.RegisterCallback<PointerDownEvent>((evt) =>
            {
                if (evt.target != drag)
                    return;
                drag.CapturePointer(evt.pointerId);
                drag.AddToClassList("poly-selector-drawer__drag--dragging");
            });
            drag.RegisterCallback<PointerUpEvent>((evt) =>
            {
                if (evt.target != drag)
                    return;
                drag.ReleasePointer(evt.pointerId);
                drag.RemoveFromClassList("poly-selector-drawer__drag--dragging");
                var destination = drag.panel.Pick(evt.position);
                if (destination == drag)
                {
                    // Clicking on self should perform a shallow copy.
                    string json = JsonUtility.ToJson(property.managedReferenceValue);
                    property.managedReferenceValue = JsonUtility.FromJson(json, property.managedReferenceValue.GetType());
                    property.serializedObject.ApplyModifiedProperties();
                    return;
                }
                if (!destination.ClassListContains("poly-selector-drawer__drag"))
                    return;
                property.serializedObject.Update();
                if (property.managedReferenceValue == null)
                    return;
                SerializedProperty destinationProperty = (SerializedProperty)destination.userData;
                destinationProperty.serializedObject.Update();
                if (!TypeFromTypeName(destinationProperty.managedReferenceFieldTypename).IsAssignableFrom(property.managedReferenceValue.GetType()))
                {
                    Debug.Log($"Destination requires type: {TypeFromTypeName(destinationProperty.managedReferenceFieldTypename).CSharpName()}, but source property has type: {property.managedReferenceValue.GetType().CSharpName()}, which wasn't compatible.");
                    return;
                }
                destinationProperty.managedReferenceValue = property.managedReferenceValue;
                destinationProperty.serializedObject.ApplyModifiedProperties();
            });

            propertyField.TrackPropertyValue(property, OnPropertyChange);
            propertyField.BindProperty(property);
            OnPropertyChange(property);

            void OnPropertyChange(SerializedProperty changedProperty)
            {
                if (changedProperty.managedReferenceValue != null)
                {
                    typePopup.Popup.value = changedProperty.managedReferenceValue.GetType().CSharpName();
                    var rng = new SplitMix64((ulong)changedProperty.managedReferenceId);
                    drag.style.backgroundColor = new Color(r: rng.NextFloat(), g: rng.NextFloat(), b: rng.NextFloat());
                }
                else
                    typePopup.Popup.value = "Undetermined";
            }

            return drawer;
        }

        Type TypeFromTypeName(string typeName)
        {
            int idx = typeName.IndexOf(" ");
            string assemblyName = typeName[..idx];
            string rest = typeName[idx..];
            string fullName = rest.Replace("/", "+");
            return Type.GetType($"{fullName}, {assemblyName}");
        }
    }
}