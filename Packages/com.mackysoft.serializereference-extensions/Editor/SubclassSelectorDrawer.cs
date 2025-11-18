using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MackySoft.SerializeReferenceExtensions.Editor
{

	[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
	public class SubclassSelectorDrawer : PropertyDrawer
	{

		struct TypePopupCache
		{
			public AdvancedTypePopup TypePopup { get; }
			public AdvancedDropdownState State { get; }
			public TypePopupCache(AdvancedTypePopup typePopup, AdvancedDropdownState state)
			{
				TypePopup = typePopup;
				State = state;
			}
		}

		const int k_MaxTypePopupLineCount = 13;

		static readonly string k_NullDisplayName = TypeMenuUtility.k_NullDisplayName;
		static readonly GUIContent k_IsNotManagedReferenceLabel = new GUIContent("The property type is not manage reference.");

		readonly Dictionary<string, TypePopupCache> m_TypePopups = new Dictionary<string, TypePopupCache>();
		readonly Dictionary<string, string> m_TypeNameCaches = new Dictionary<string, string>();

		SerializedProperty m_TargetProperty;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			if (property.propertyType != SerializedPropertyType.ManagedReference)
			{
				// If this property is not a managed reference, polymorphism isn't allowed anyway. Use the default drawer.
				EditorGUI.PropertyField(position, property, label, true);
			}
			else
			{
				string originalText = label.text;
				Rect selectorRect = new Rect(position);
				selectorRect.height = EditorGUIUtility.singleLineHeight;
				Rect typepopupRect = EditorGUI.PrefixLabel(selectorRect, new GUIContent($"{originalText} Type"));

				if (EditorGUI.DropdownButton(typepopupRect, new GUIContent(GetTypeName(property)), FocusType.Keyboard))
				{
					TypePopupCache popup = GetTypePopup(property);
					m_TargetProperty = property;
					popup.TypePopup.Show(typepopupRect);
				}

				Rect propertyRect = new Rect(position);
				propertyRect.y += EditorGUIUtility.singleLineHeight;
#if UNITY_2021_3_OR_NEWER
				// Override the label text with the ToString() of the managed reference.
				var subclassSelectorAttribute = (SubclassSelectorAttribute)attribute;
				if (subclassSelectorAttribute.UseToStringAsLabel && !property.hasMultipleDifferentValues)
				{
					object managedReferenceValue = property.managedReferenceValue;
					if (managedReferenceValue != null)
					{
						originalText = managedReferenceValue.ToString();
					}
				}
#endif
				EditorGUI.PropertyField(propertyRect, property, new GUIContent($"{originalText} ({GetTypeName(property)})"), true);
			}

			EditorGUI.EndProperty();
		}

		TypePopupCache GetTypePopup(SerializedProperty property)
		{
			// Cache this string. This property internally call Assembly.GetName, which result in a large allocation.
			string managedReferenceFieldTypename = property.managedReferenceFieldTypename;

			if (!m_TypePopups.TryGetValue(managedReferenceFieldTypename, out TypePopupCache result))
			{
				var state = new AdvancedDropdownState();

				Type baseType = ManagedReferenceUtility.GetType(managedReferenceFieldTypename);

				TypeSearch.IConstraint[] constraints = { new TypeSearch.UpperBound(baseType) };
				IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(x => x.GetTypes())
					.Where((type) => TypeSearch.IsValidRootType(type));
				var popup = new AdvancedTypePopup(
					TypeSearch.GetCandidates(constraints, types),
					constraints,
					k_MaxTypePopupLineCount,
					state
				);
				popup.OnTypeSelected += type =>
				{
					// Apply changes to individual serialized objects.
					foreach (var targetObject in m_TargetProperty.serializedObject.targetObjects)
					{
						SerializedObject individualObject = new SerializedObject(targetObject);
						SerializedProperty individualProperty = individualObject.FindProperty(m_TargetProperty.propertyPath);
						object obj = individualProperty.SetManagedReference(type);
						individualProperty.isExpanded = (obj != null);

						individualObject.ApplyModifiedProperties();
						individualObject.Update();
					}
				};

				result = new TypePopupCache(popup, state);
				m_TypePopups.Add(managedReferenceFieldTypename, result);
			}
			return result;
		}

		string GetTypeName(SerializedProperty property)
		{
			// Cache this string.
			string managedReferenceFullTypename = property.managedReferenceFullTypename;

			if (string.IsNullOrEmpty(managedReferenceFullTypename))
			{
				return k_NullDisplayName;
			}
			if (m_TypeNameCaches.TryGetValue(managedReferenceFullTypename, out string cachedTypeName))
			{
				return cachedTypeName;
			}

			Type type = ManagedReferenceUtility.GetType(managedReferenceFullTypename);
			AddTypeMenuAttribute typeMenu = TypeMenuUtility.GetAttribute(type);
			string typeName = typeMenu?.GetTypeNameWithoutPath();
			if (string.IsNullOrWhiteSpace(typeName))
			{
				typeName = type.Name;
			}
			m_TypeNameCaches.Add(managedReferenceFullTypename, typeName);
			return typeName;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var propertyHeight = property.isExpanded ? EditorGUI.GetPropertyHeight(property, true) : EditorGUI.GetPropertyHeight(property, false);

			if (property.propertyType == SerializedPropertyType.ManagedReference)
			{
				propertyHeight += EditorGUIUtility.singleLineHeight;
			}
			return propertyHeight;
		}

	}
}
