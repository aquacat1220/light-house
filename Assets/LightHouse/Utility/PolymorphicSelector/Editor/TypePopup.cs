namespace LightHouse
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.Properties;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.UIElements;

    // Define the custom control type.
    [UxmlElement]
    public partial class TypePopup : VisualElement
    {
        public enum TypePopupItemDataEnum
        {
            Info,
            Nongeneric,
            Generic,
            GenericParameter
        }
        public class Info
        {
        }
        public class Nongeneric
        {
            public Type Type;
        }
        public class Generic
        {
            public Type Type;
            public bool Disabled = false;
        }
        public class GenericParameter
        {
            public Type TypeParameter;
            public TypeConstraint.IConstraint[] Constraints;
            public Type SelectedType;
            public int Index;
            public bool Disabled = false;
        }
        public class TypePopupItemData
        {
            public TypePopupItemDataEnum Enum;
            public int Id;
            public Info Info;
            public Nongeneric Nongeneric;
            public Generic Generic;
            public GenericParameter GenericParameter;
        }

        static VisualTreeAsset _typePopupTemplate;

        TypeConstraint.IConstraint[] _constraints;
        Func<Type, bool> _filter;

        public Popup Popup { get; }
        TreeView _treeview;

        public event Action<Type> TypeSelected;

        public TypePopup() : this(null, null) { }

        public TypePopup(TypeConstraint.IConstraint[] constraints, Func<Type, bool> filter)
        {
            if (_typePopupTemplate == null)
                _typePopupTemplate = Resources.Load<VisualTreeAsset>("TypePopupTemplate");
            _typePopupTemplate.CloneTree(this);

            this.AddToClassList("type-popup");
            Popup = this.Q<Popup>(className: "type-popup__popup");
            _treeview = this.Q<TreeView>(className: "type-popup__treeview");

            Popup.Clicked += () =>
            {
                // We want to build tree items the first time we click on the popup.
                // We always have at least one element, because if no candidates are found we will add a label telling the user so.
                if (_treeview.GetRootElementForId(0) != null)
                    return;
                // Fetch all candidates, add nongenerics as leaf nodes, add generics with type parameters as children.
                var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm => asm.GetTypes())
                    .Where(_filter);
                var candidateTypes = TypeConstraint.GetCandidates(_constraints, allTypes);
                if (candidateTypes.Count == 0)
                {
                    Popup.value = "Unsatisfiable";
                    TypeSelected?.Invoke(null);
                    return;
                }

                // id 0 is for the info on top.
                int id = 0;
                var items = candidateTypes.Select(
                    candidateType =>
                    {
                        if (!candidateType.ContainsGenericParameters)
                        {
                            var data = new TypePopupItemData();
                            data.Enum = TypePopupItemDataEnum.Nongeneric;
                            data.Id = id;
                            var nongeneric = new Nongeneric();
                            nongeneric.Type = candidateType;
                            data.Nongeneric = nongeneric;
                            var item = new TreeViewItemData<TypePopupItemData>(id++, data, null);
                            return item;
                        }
                        else
                        {
                            var data = new TypePopupItemData();
                            data.Enum = TypePopupItemDataEnum.Generic;
                            data.Id = id;
                            var generic = new Generic();
                            generic.Type = candidateType;
                            data.Generic = generic;
                            int index = 0;
                            var children = candidateType.GetGenericArguments().Select(
                                typeParameter =>
                                {
                                    var data = new TypePopupItemData();
                                    data.Enum = TypePopupItemDataEnum.GenericParameter;
                                    data.Id = id;
                                    var genericParameter = new GenericParameter();
                                    genericParameter.Index = index++;
                                    genericParameter.TypeParameter = typeParameter;
                                    data.GenericParameter = genericParameter;
                                    var item = new TreeViewItemData<TypePopupItemData>(id++, data, null);
                                    return item;
                                }
                            );
                            var item = new TreeViewItemData<TypePopupItemData>(id++, data, children.ToList());
                            return item;
                        }
                    }
                );

                var infoData = new TypePopupItemData();
                infoData.Enum = TypePopupItemDataEnum.Info;
                infoData.Id = id;
                var info = new Info();
                infoData.Info = info;
                items = items.Prepend(
                    new TreeViewItemData<TypePopupItemData>(id++, infoData, null)
                );

                _treeview.SetRootItems(items.ToList());
                _treeview.Rebuild();
            };

            _treeview.makeItem = () =>
            {
                var item = new VisualElement();
                item.AddToClassList("type-popup__treeview-item");
                var label = new Label();
                label.AddToClassList("type-popup__treeview-item-label");
                var innerPopup = new TypePopup();
                innerPopup.AddToClassList("type-popup__treeview-item-inner-popup");
                innerPopup.AddToClassList("type-popup__treeview-item-inner-popup--disabled");
                item.Add(label);
                item.Add(innerPopup);
                return item;
            };
            _treeview.bindItem = (element, index) =>
            {
                var data = _treeview.GetItemDataForIndex<TypePopupItemData>(index);
                var id = _treeview.GetIdForIndex(index);
                var label = element.Q<Label>(className: "type-popup__treeview-item-label");
                var innerPopup = element.Q<TypePopup>(className: "type-popup__treeview-item-inner-popup");
                innerPopup.AddToClassList("type-popup__treeview-item-inner-popup--disabled");
                innerPopup.Reset(null, null);
                if (data.Enum == TypePopupItemDataEnum.Info)
                {
                    label.text = $"ID: {data.Id} INFO";
                    return;
                }
                else if (data.Enum == TypePopupItemDataEnum.Nongeneric)
                {
                    label.text = $"ID: {data.Id} {data.Nongeneric.Type.CSharpName()}";
                    return;
                }
                else if (data.Enum == TypePopupItemDataEnum.Generic)
                {
                    if (data.Generic.Disabled)
                        label.text = $"ID: {data.Id} {data.Generic.Type.CSharpName()} - Unsatisfiable";
                    else
                        label.text = $"ID: {data.Id} {data.Generic.Type.CSharpName()}";
                    return;
                }
                else if (data.Enum == TypePopupItemDataEnum.GenericParameter)
                {
                    if (data.GenericParameter.Disabled)
                    {
                        label.text = $"ID: {data.Id} {data.GenericParameter.TypeParameter.CSharpName()} - Unsatisfiable";
                        return;
                    }
                    else
                    {
                        if (data.GenericParameter.Constraints == null)
                        {
                            int parentId = _treeview.viewController.GetParentId(id);
                            var parentData = _treeview.GetItemDataForId<TypePopupItemData>(parentId);
                            if (parentData.Enum != TypePopupItemDataEnum.Generic || parentData.Generic.Disabled)
                                throw new Exception();
                            foreach (var constraint in _constraints)
                            {
                                if (constraint is TypeConstraint.UpperBound)
                                    Debug.Log(((TypeConstraint.UpperBound)constraint).Parent);
                            }
                            Debug.Log(parentData.Generic.Type);
                            var parameterConstraints = TypeConstraint.PropagateConstraints(parentData.Generic.Type, _constraints);
                            if (parameterConstraints == null)
                            {
                                Debug.Log("HERE");
                                parentData.Generic.Disabled = true;
                                foreach (int childId in _treeview.viewController.GetChildrenIds(parentId))
                                {
                                    var childData = _treeview.GetItemDataForId<TypePopupItemData>(childId);
                                    childData.GenericParameter.Disabled = true;
                                }
                                _treeview.RefreshItems();
                                return;
                            }
                            foreach (int childId in _treeview.viewController.GetChildrenIds(parentId))
                            {
                                var childData = _treeview.GetItemDataForId<TypePopupItemData>(childId);
                                childData.GenericParameter.Constraints = parameterConstraints[childData.GenericParameter.TypeParameter].ToArray();
                            }
                        }

                        label.text = $"ID: {data.Id} {data.GenericParameter.TypeParameter.CSharpName()}";
                        innerPopup.TypeSelected += (type) =>
                        {
                            if (type == null)
                            {
                                // A `null` type means the typepopup determined that the constraints were not satisfiable.
                                int parentId = _treeview.viewController.GetParentId(id);
                                var parentData = _treeview.GetItemDataForId<TypePopupItemData>(parentId);
                                if (parentData.Enum != TypePopupItemDataEnum.Generic || parentData.Generic.Disabled)
                                    throw new Exception();
                                parentData.Generic.Disabled = true;
                                foreach (int childId in _treeview.viewController.GetChildrenIds(parentId))
                                {
                                    var childData = _treeview.GetItemDataForId<TypePopupItemData>(childId);
                                    childData.GenericParameter.Disabled = true;
                                }
                                _treeview.RefreshItems();
                                return;
                            }
                            data.GenericParameter.SelectedType = type;
                        }; ;
                        innerPopup.Reset(data.GenericParameter.Constraints, null); // Reset with propagated constraints.
                        if (data.GenericParameter.SelectedType != null)
                            innerPopup.Popup.value = data.GenericParameter.SelectedType.CSharpName();
                        innerPopup.RemoveFromClassList("type-popup__treeview-item-inner-popup--disabled");
                        return;
                    }
                }
            };
            _treeview.unbindItem = (element, index) =>
            {
                var data = _treeview.GetItemDataForIndex<TypePopupItemData>(index);
                var id = _treeview.GetIdForIndex(index);
                var label = element.Q<Label>(className: "type-popup__treeview-item-label");
                var innerPopup = element.Q<TypePopup>(className: "type-popup__treeview-item-inner-popup");
                innerPopup.AddToClassList("type-popup__treeview-item-inner-popup--disabled");
                innerPopup.Reset(null, null);
                innerPopup.TypeSelected = null;
            };

            _treeview.selectionChanged += (items) =>
            {
                TypePopupItemData data = (TypePopupItemData)items.First();
                int id = data.Id;
                if (_treeview.IsExpanded(id))
                    _treeview.CollapseItem(id);
                else
                    _treeview.ExpandItem(id);
            };
            _treeview.itemsChosen += (items) =>
            {
                TypePopupItemData data = (TypePopupItemData)items.First();
                if (data.Enum == TypePopupItemDataEnum.Nongeneric)
                {
                    Popup.value = data.Nongeneric.Type.CSharpName();
                    TypeSelected?.Invoke(data.Nongeneric.Type);
                    Popup.WantPopupOpen = false;
                    return;
                }
                else if (data.Enum == TypePopupItemDataEnum.Generic && !data.Generic.Disabled)
                {
                    Type[] typeArguments = data.Generic.Type.GetGenericArguments();
                    foreach (int childId in _treeview.viewController.GetChildrenIds(data.Id))
                    {
                        var childData = _treeview.GetItemDataForId<TypePopupItemData>(childId);
                        if (childData.GenericParameter.SelectedType == null || childData.GenericParameter.SelectedType.ContainsGenericParameters)
                            return;
                        typeArguments[childData.GenericParameter.Index] = childData.GenericParameter.SelectedType;
                    }
                    Type constructedType = data.Generic.Type.MakeGenericType(typeArguments);
                    if (constructedType != null)
                    {
                        Popup.value = constructedType.CSharpName();
                        TypeSelected?.Invoke(constructedType);
                        Popup.WantPopupOpen = false;
                    }
                    return;
                }
            };
            Reset(constraints, filter);
        }

        public void Reset(TypeConstraint.IConstraint[] constraints, Func<Type, bool> filter)
        {
            if (constraints == null)
                constraints = new TypeConstraint.IConstraint[0];
            if (filter == null)
                filter = (type) => type.IsVisible;

            Popup.WantPopupOpen = false;
            Popup.value = "";
            _treeview.SetRootItems<TypePopupItemData>(null);
            _constraints = constraints;
            _filter = filter;
        }

        // We don't want this element to have children.
        public override VisualElement contentContainer => null;
    }
}