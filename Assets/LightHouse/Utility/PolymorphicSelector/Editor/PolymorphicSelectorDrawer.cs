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

            var popup = drawer.Q<Popup>(className: "polymorphic-selector-drawer__popup");
            var treeView = drawer.Q<TreeView>(className: "polymorphic-selector-drawer__treeview");
            var propertyField = drawer.Q<PropertyField>(className: "polymorphic-selector-drawer__property-field");

            if (property.managedReferenceValue != null)
                popup.value = property.managedReferenceValue.GetType().CSharpFullName();
            else
                popup.value = fieldInfo.FieldType.CSharpFullName();

            popup.Clicked += () =>
            {
                // We want to build tree items the first time we click on the popup.
                // We always have at least one element, because if no candidates are found we will add a label telling the user so.
                if (treeView.GetRootElementForId(0) != null)
                    return;
                // Fetch all candidates, add nongenerics as leaf nodes, add generics with type parameters as children.
            };

            treeView.makeItem = () => new Label();
            treeView.bindItem = (element, index) =>
            {
                var methodInfo = treeView.GetItemDataForIndex<System.Reflection.MethodInfo>(index);
                var id = treeView.GetIdForIndex(index);
                string text = "";
                ((Label)element).text = text;
            };

            propertyField.BindProperty(property);
            treeView.selectionChanged += (items) =>
            {
                // If selected type is a generic type that hasn't been selected previously, propagate constraints.
            };

            return drawer;
        }
    }
}