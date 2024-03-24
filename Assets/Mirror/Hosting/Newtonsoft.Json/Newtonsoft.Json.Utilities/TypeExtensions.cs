using Newtonsoft.Json.Shims;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal static class TypeExtensions
	{
		public static MemberTypes MemberType(this MemberInfo memberInfo)
		{
			return memberInfo.MemberType;
		}

		public static bool ContainsGenericParameters(this Type type)
		{
			return type.ContainsGenericParameters;
		}

		public static bool IsInterface(this Type type)
		{
			return type.IsInterface;
		}

		public static bool IsGenericType(this Type type)
		{
			return type.IsGenericType;
		}

		public static bool IsGenericTypeDefinition(this Type type)
		{
			return type.IsGenericTypeDefinition;
		}

		public static Type BaseType(this Type type)
		{
			return type.BaseType;
		}

		public static bool IsEnum(this Type type)
		{
			return type.IsEnum;
		}

		public static bool IsClass(this Type type)
		{
			return type.IsClass;
		}

		public static bool IsSealed(this Type type)
		{
			return type.IsSealed;
		}

		public static bool IsAbstract(this Type type)
		{
			return type.IsAbstract;
		}

		public static bool IsValueType(this Type type)
		{
			return type.IsValueType;
		}

		public static bool AssignableToTypeName(this Type type, string fullTypeName, out Type match)
		{
			for (Type type2 = type; type2 != null; type2 = type2.BaseType())
			{
				if (string.Equals(type2.FullName, fullTypeName, StringComparison.Ordinal))
				{
					match = type2;
					return true;
				}
			}
			Type[] interfaces = type.GetInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (string.Equals(interfaces[i].Name, fullTypeName, StringComparison.Ordinal))
				{
					match = type;
					return true;
				}
			}
			match = null;
			return false;
		}

		public static bool ImplementInterface(this Type type, Type interfaceType)
		{
			for (Type type2 = type; type2 != null; type2 = type2.BaseType())
			{
				foreach (Type item in (IEnumerable<Type>)type2.GetInterfaces())
				{
					if (item == interfaceType || (item != null && item.ImplementInterface(interfaceType)))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
