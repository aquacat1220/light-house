namespace LightHouse.Fn
{
    using System;
    using System.Linq;
    using Unity.VisualScripting;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(MethodInfo))]
    public class MethodInfoDrawer : PropertyDrawer
    {
        static VisualTreeAsset _methodInfoDrawerTemplate;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.hasMultipleDifferentValues)
                return new Label("Multi-object editing is not possible with this type.");

            if (_methodInfoDrawerTemplate == null)
                _methodInfoDrawerTemplate = Resources.Load<VisualTreeAsset>("MethodInfoDrawerTemplate");
            var drawer = _methodInfoDrawerTemplate.Instantiate();

            var objectField = drawer.Q<ObjectField>(className: "method-info-drawer__object");

            var popup = drawer.Q<Popup>(className: "method-info-drawer__popup");
            var treeView = drawer.Q<TreeView>(className: "method-info-drawer__treeview");
            treeView.makeItem = () => new Label();
            treeView.bindItem = (element, index) =>
            {
                var methodInfo = treeView.GetItemDataForIndex<System.Reflection.MethodInfo>(index);
                var id = treeView.GetIdForIndex(index);
                string text = "";
                if (methodInfo.ReturnType == null)
                    text += "void";
                else
                {
                    Debug.Log(methodInfo.ReturnType);
                    text += methodInfo.ReturnType.CSharpName();
                }
                text += $" {methodInfo.Name}(";
                text += string.Join(", ", methodInfo.GetParameters().Select((param) => $"{param.ParameterType.CSharpName()} {param.Name}"));
                text += ")";
                ((Label)element).text = text;
            };

            objectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(OnObjectChanged);
            popup.RegisterCallback<ChangeEvent<string>>(OnPopupChanged);
            treeView.itemsChosen += (items) =>
            {
                System.Reflection.MethodInfo item = (System.Reflection.MethodInfo)items.First();
                string text = "";
                if (item.ReturnType == null)
                    text += "void";
                else
                    text += item.ReturnType.CSharpName();
                text += $" {item.Name}(";
                text += string.Join(", ", item.GetParameters().Select((param) => $"{param.ParameterType.CSharpName()}"));
                text += ")";
                popup.value = text;
                popup.WantPopupOpen = false;
            };

            void OnObjectChanged(ChangeEvent<UnityEngine.Object> evt)
            {
                if (objectField.value == null)
                    return;

                string[] splitPath = property.propertyPath.Split(".");
                var parentProperty = property.serializedObject.FindProperty(string.Join(".", splitPath[..^1]));
                Type[] types = fieldInfo.DeclaringType.GetGenericArguments();
                Type[] parameterTypes = types[..^1];
                Type returnType = types[^1];
                if (returnType == typeof(Tuple))
                    returnType = null;

                if (!string.IsNullOrEmpty(popup.value))
                {
                    string methodName = popup.value.Split("(")[0].Split(" ")[^1];
                    var methodInfo = objectField.value.GetType().GetMethod(name: methodName, types: parameterTypes);
                    if (methodInfo == null || methodInfo.ReturnType != returnType)
                        popup.value = "";
                }

                // And since the object changed, we need to refresh all items in the treeview to match this new type.
                var methodInfos = objectField.value.GetType().GetMethods().Where(
                    (methodInfo) =>
                    {
                        if (methodInfo.ReturnType != returnType)
                            return false;
                        var parameters = methodInfo.GetParameters().Select((parameter) => parameter.ParameterType);
                        if (!parameterTypes.SequenceEqual(parameters))
                            return false;
                        return true;
                    }
                );
                int id = 0;
                var items = methodInfos.Select(
                    (methodInfo) =>
                    {
                        var item = new TreeViewItemData<System.Reflection.MethodInfo>(id++, methodInfo, null);
                        return item;
                    }
                ).ToList();
                treeView.SetRootItems(items);
                treeView.Rebuild();
            }

            void OnPopupChanged(ChangeEvent<string> evt)
            {
                if (objectField.value == null || string.IsNullOrEmpty(popup.value))
                    return;

                string[] splitPath = property.propertyPath.Split(".");
                var parentProperty = property.serializedObject.FindProperty(string.Join(".", splitPath[..^1]));
                Type[] types = fieldInfo.DeclaringType.GetGenericArguments();
                Type[] parameterTypes = types[..^1];
                Type returnType = types[^1];
                if (returnType == typeof(Tuple))
                    returnType = null;

                string methodName = popup.value.Split("(")[0].Split(" ")[^1];
                var methodInfo = objectField.value.GetType().GetMethod(name: methodName, types: parameterTypes);
                if (methodInfo == null || methodInfo.ReturnType != returnType)
                    popup.value = "";
            }

            return drawer;
        }
    }

}