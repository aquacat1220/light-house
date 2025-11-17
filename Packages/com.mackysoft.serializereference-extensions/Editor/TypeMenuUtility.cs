using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace MackySoft.SerializeReferenceExtensions.Editor
{
	public static class TypeMenuUtility
	{

		public const string k_NullDisplayName = "<null>";

		public static AddTypeMenuAttribute GetAttribute(Type type)
		{
			return Attribute.GetCustomAttribute(type, typeof(AddTypeMenuAttribute)) as AddTypeMenuAttribute;
		}

		public static string[] GetSplittedTypePath(Type type)
		{
			AddTypeMenuAttribute typeMenu = GetAttribute(type);
			if (typeMenu != null)
			{
				return typeMenu.GetSplittedMenuName();
			}
			else
			{
				return type.FullName.Split(".");
			}
		}

		public static IEnumerable<Type> OrderByType(this IEnumerable<Type> source)
		{
			return source.OrderBy(type =>
			{
				if (type == null)
				{
					return -999;
				}
				return GetAttribute(type)?.Order ?? 0;
			}).ThenBy(type =>
			{
				if (type == null)
				{
					return null;
				}
				return GetAttribute(type)?.MenuName ?? type.Name;
			});
		}

	}
}