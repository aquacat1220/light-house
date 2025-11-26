using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MackySoft.SerializeReferenceExtensions.Editor
{

	public class AdvancedTypePopupItem : AdvancedDropdownItem
	{
		public Type Type { get; }

		public AdvancedTypePopupItem(Type type, string name) : base(name)
		{
			Type = type;
		}
	}

	public class AdvancedGenericTypePopupItem : AdvancedTypePopupItem
	{
		public bool PropagatedConstraints = false;
		public bool Unsatisfiable = false;
		public TypeSearch.IConstraint[] Constraints { get; private set; }

		public AdvancedGenericTypePopupItem(Type type, TypeSearch.IConstraint[] constraints, string name) : base(type, name)
		{
			Constraints = constraints;
		}
	}

	public class AdvancedTypeParameterPopupItem : AdvancedDropdownItem
	{
		public AdvancedGenericTypePopupItem Parent { get; private set; }
		public Type TypeParameter { get; private set; }
		public Type SelectedType { get; set; }
		public List<TypeSearch.IConstraint> Constraints { get; set; }

		public AdvancedTypeParameterPopupItem(AdvancedGenericTypePopupItem parent, Type typeParameter, string name) : base(name)
		{
			Parent = parent;
			TypeParameter = typeParameter;
		}
	}

	/// <summary>
	/// A type popup with a fuzzy finder.
	/// </summary>
	public class AdvancedTypePopup : AdvancedDropdown
	{

		const int k_MaxNamespaceNestCount = 16;
		const int k_MaxChildTypePopupLineCount = 13;
		const int k_ChildPopupOffset = 8;

		public static void AddTo(AdvancedDropdownItem root, IEnumerable<Type> types, TypeSearch.IConstraint[] constraints = null)
		{
			int itemCount = 0;

			// Add null item.
			var nullItem = new AdvancedTypePopupItem(null, TypeMenuUtility.k_NullDisplayName)
			{
				id = itemCount++
			};
			root.AddChild(nullItem);

			Type[] typeArray = types.OrderByType().ToArray();

			// Single namespace if the root has one namespace and the nest is unbranched.
			bool isSingleNamespace = true;
			string[] namespaces = new string[k_MaxNamespaceNestCount];
			foreach (Type type in typeArray)
			{
				string[] splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);
				if (splittedTypePath.Length <= 1)
				{
					continue;
				}
				// If they explicitly want sub category, let them do.
				if (TypeMenuUtility.GetAttribute(type) != null)
				{
					isSingleNamespace = false;
					break;
				}
				for (int k = 0; (splittedTypePath.Length - 1) > k; k++)
				{
					string ns = namespaces[k];
					if (ns == null)
					{
						namespaces[k] = splittedTypePath[k];
					}
					else if (ns != splittedTypePath[k])
					{
						isSingleNamespace = false;
						break;
					}
				}

				if (!isSingleNamespace)
				{
					break;
				}
			}

			// Add type items.
			foreach (Type type in typeArray)
			{
				string[] splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);
				if (splittedTypePath.Length == 0)
				{
					continue;
				}

				AdvancedDropdownItem parent = root;

				// Add namespace items.
				if (!isSingleNamespace)
				{
					for (int k = 0; (splittedTypePath.Length - 1) > k; k++)
					{
						AdvancedDropdownItem foundItem = GetItem(parent, splittedTypePath[k]);
						if (foundItem != null)
						{
							parent = foundItem;
						}
						else
						{
							var newItem = new AdvancedDropdownItem(splittedTypePath[k])
							{
								id = itemCount++,
							};
							parent.AddChild(newItem);
							parent = newItem;
						}
					}
				}

				// Add type item.
				if (!type.ContainsGenericParameters)
				{
					// `type` doesn't contain generic parameters.
					var item = new AdvancedTypePopupItem(type, splittedTypePath[splittedTypePath.Length - 1])
					{
						id = itemCount++
					};
					parent.AddChild(item);
				}
				else
				{
					// `type` contains generic parameters. Make sure it is a generic definition.
					if (!type.IsGenericTypeDefinition)
						throw new Exception();
					var item = new AdvancedGenericTypePopupItem(type, constraints, splittedTypePath[splittedTypePath.Length - 1])
					{
						id = itemCount++
					};
					foreach (Type parameter in type.GetGenericArguments())
					{
						var parameterItem = new AdvancedTypeParameterPopupItem(item, parameter, parameter.Name)
						{
							id = itemCount++
						};
						item.AddChild(parameterItem);
					}
					parent.AddChild(item);
				}
			}
		}

		static AdvancedDropdownItem GetItem(AdvancedDropdownItem parent, string name)
		{
			foreach (AdvancedDropdownItem item in parent.children)
			{
				if (item.name == name)
				{
					return item;
				}
			}
			return null;
		}

		static readonly float k_HeaderHeight = EditorGUIUtility.singleLineHeight * 2f;

		Type[] m_Types;
		TypeSearch.IConstraint[] m_Constraints;
		AdvancedTypePopup m_InnerPopup;
		AdvancedDropdownItem m_CachedRoot;
		Rect m_LastScreenRect;

		public event Action<Type> OnTypeSelected;

		public AdvancedTypePopup(IEnumerable<Type> types, TypeSearch.IConstraint[] constraints, int maxLineCount, AdvancedDropdownState state) : base(state)
		{
			m_Types = types.ToArray();
			m_Constraints = constraints;
			minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * maxLineCount + k_HeaderHeight);
		}

		public new void Show(Rect rect)
		{
			m_LastScreenRect = EditorGUIUtility.GUIToScreenRect(rect);
			Refresh();
		}

		void Refresh()
		{
			Rect localRect = EditorGUIUtility.ScreenToGUIRect(m_LastScreenRect);
			Rect childRect = new Rect(localRect);
			childRect.position += new Vector2(k_ChildPopupOffset, k_ChildPopupOffset);
			base.Show(localRect);
			if (m_InnerPopup != null)
			{
				m_InnerPopup.Show(childRect);
			}
		}

		protected override AdvancedDropdownItem BuildRoot()
		{
			if (m_CachedRoot != null)
				return m_CachedRoot;
			var root = new AdvancedDropdownItem("Select Type");
			AddTo(root, m_Types, m_Constraints);
			m_CachedRoot = root;
			return m_CachedRoot;
		}

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			base.ItemSelected(item);
			if (item is AdvancedTypePopupItem typePopupItem)
			{
				OnTypeSelected?.Invoke(typePopupItem.Type);
				return;
			}
			else if (item is AdvancedTypeParameterPopupItem typeParameterItem)
			{
				if (typeParameterItem.Constraints == null)
				{
					// Type parameter popup items receive constraints from their parent item.
					AdvancedGenericTypePopupItem parent = typeParameterItem.Parent;
					if (parent.Unsatisfiable)
					{
						// This type was deemed unsatisfiable.
						Refresh();
						return;
					}
					if (parent.PropagatedConstraints)
						throw new Exception();
					var parameterConstraints = TypeSearch.PropagateConstraints(parent.Type, parent.Constraints);
					parent.PropagatedConstraints = true;
					if (parameterConstraints == null)
					{
						// The constraint was unsatisfiable.
						parent.Unsatisfiable = true;
						// parent.name += " (Unsatisfiable)";
						// Name changing doesn't work.
						foreach (AdvancedTypeParameterPopupItem parameterItem in parent.children)
						{
							// parameterItem.name += " (Unsatisfiable)";
							// Name changing doesn't work.
						}
						Refresh();
						return;
					}
					foreach (AdvancedTypeParameterPopupItem parameterItem in parent.children)
					{
						parameterItem.Constraints = parameterConstraints[parameterItem.TypeParameter];
					}
				}

				IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(x => x.GetTypes())
					.Where((type) => (type.IsPublic || type.IsNestedPublic));
				m_InnerPopup = new AdvancedTypePopup(
					TypeSearch.GetCandidates(typeParameterItem.Constraints.ToArray(), types),
					typeParameterItem.Constraints.ToArray(),
					k_MaxChildTypePopupLineCount,
					new AdvancedDropdownState()
				);


				Action<Type> onInnerPopupSelected = null;
				onInnerPopupSelected = (Type type) =>
				{
					typeParameterItem.SelectedType = type;
					m_InnerPopup.OnTypeSelected -= onInnerPopupSelected;
					m_InnerPopup = null;
					Type constructedType = TryConstructGeneric(typeParameterItem.Parent);
					if (constructedType != null)
						OnTypeSelected?.Invoke(constructedType);
					else
						// Without the delayed call, `Refresh()` will show this dropdown for only a single frame.
						// I suspect it has to do something with focus.
						EditorApplication.delayCall += Refresh;
				};
				m_InnerPopup.OnTypeSelected += onInnerPopupSelected;
				Refresh();
				return;
			}
		}

		static Type TryConstructGeneric(AdvancedGenericTypePopupItem item)
		{
			Type genericDefinition = item.Type;
			List<Type> typeArguments = new();

			foreach (AdvancedTypeParameterPopupItem child in item.children)
			{
				if (child.SelectedType != null)
					typeArguments.Add(child.SelectedType);
				else
					return null;
			}
			return genericDefinition.MakeGenericType(typeArguments.ToArray());
		}
	}
}