using Newtonsoft.Json.Shims;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal static class EnumUtils
	{
		private static readonly ThreadSafeStore<Type, BidirectionalDictionary<string, string>> EnumMemberNamesPerType = new ThreadSafeStore<Type, BidirectionalDictionary<string, string>>(InitializeEnumType);

		private static BidirectionalDictionary<string, string> InitializeEnumType(Type type)
		{
			BidirectionalDictionary<string, string> bidirectionalDictionary = new BidirectionalDictionary<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				string name = fieldInfo.Name;
				string text = (from EnumMemberAttribute a in fieldInfo.GetCustomAttributes(typeof(EnumMemberAttribute), true)
				select a.Value).SingleOrDefault() ?? fieldInfo.Name;
				string first;
				if (bidirectionalDictionary.TryGetBySecond(text, out first))
				{
					throw new InvalidOperationException("Enum name '{0}' already exists on enum '{1}'.".FormatWith(CultureInfo.InvariantCulture, text, type.Name));
				}
				bidirectionalDictionary.Set(name, text);
			}
			return bidirectionalDictionary;
		}

		public static IList<object> GetValues(Type enumType)
		{
			if (!enumType.IsEnum())
			{
				throw new ArgumentException("Type '" + enumType.Name + "' is not an enum.");
			}
			List<object> list = new List<object>();
			foreach (FieldInfo item in from f in enumType.GetFields()
			where f.IsLiteral
			select f)
			{
				object value = item.GetValue(enumType);
				list.Add(value);
			}
			return list;
		}
	}
}
