using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Shims;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal class ReflectionObject
	{
		public ObjectConstructor<object> Creator
		{
			get;
			private set;
		}

		public IDictionary<string, ReflectionMember> Members
		{
			get;
			private set;
		}

		public ReflectionObject()
		{
			Members = new Dictionary<string, ReflectionMember>();
		}

		public object GetValue(object target, string member)
		{
			return Members[member].Getter(target);
		}

		public Type GetType(string member)
		{
			return Members[member].MemberType;
		}

		public static ReflectionObject Create(Type t, params string[] memberNames)
		{
			return Create(t, null, memberNames);
		}

		public static ReflectionObject Create(Type t, MethodBase creator, params string[] memberNames)
		{
			ReflectionObject reflectionObject = new ReflectionObject();
			ReflectionDelegateFactory reflectionDelegateFactory = JsonTypeReflector.ReflectionDelegateFactory;
			if (creator != null)
			{
				reflectionObject.Creator = reflectionDelegateFactory.CreateParameterizedConstructor(creator);
			}
			else if (ReflectionUtils.HasDefaultConstructor(t, false))
			{
				Func<object> ctor = reflectionDelegateFactory.CreateDefaultConstructor<object>(t);
				reflectionObject.Creator = ((object[] args) => ctor());
			}
			MethodCall<object, object> call;
			MethodCall<object, object> call2;
			foreach (string text in memberNames)
			{
				MemberInfo[] member = t.GetMember(text, BindingFlags.Instance | BindingFlags.Public);
				if (member.Length != 1)
				{
					throw new ArgumentException("Expected a single member with the name '{0}'.".FormatWith(CultureInfo.InvariantCulture, text));
				}
				MemberInfo memberInfo = member.Single();
				ReflectionMember reflectionMember = new ReflectionMember();
				switch (memberInfo.MemberType())
				{
				case MemberTypes.Field:
				case MemberTypes.Property:
					if (ReflectionUtils.CanReadMemberValue(memberInfo, false))
					{
						reflectionMember.Getter = reflectionDelegateFactory.CreateGet<object>(memberInfo);
					}
					if (ReflectionUtils.CanSetMemberValue(memberInfo, false, false))
					{
						reflectionMember.Setter = reflectionDelegateFactory.CreateSet<object>(memberInfo);
					}
					break;
				case MemberTypes.Method:
				{
					MethodInfo methodInfo = (MethodInfo)memberInfo;
					if (methodInfo.IsPublic)
					{
						ParameterInfo[] parameters = methodInfo.GetParameters();
						if (parameters.Length == 0 && methodInfo.ReturnType != typeof(void))
						{
							call = reflectionDelegateFactory.CreateMethodCall<object>(methodInfo);
							reflectionMember.Getter = ((object target) => call(target));
						}
						else if (parameters.Length == 1 && methodInfo.ReturnType == typeof(void))
						{
							call2 = reflectionDelegateFactory.CreateMethodCall<object>(methodInfo);
							reflectionMember.Setter = delegate(object target, object arg)
							{
								call2(target, arg);
							};
						}
					}
					break;
				}
				default:
					throw new ArgumentException("Unexpected member type '{0}' for member '{1}'.".FormatWith(CultureInfo.InvariantCulture, memberInfo.MemberType(), memberInfo.Name));
				}
				if (ReflectionUtils.CanReadMemberValue(memberInfo, false))
				{
					reflectionMember.Getter = reflectionDelegateFactory.CreateGet<object>(memberInfo);
				}
				if (ReflectionUtils.CanSetMemberValue(memberInfo, false, false))
				{
					reflectionMember.Setter = reflectionDelegateFactory.CreateSet<object>(memberInfo);
				}
				reflectionMember.MemberType = ReflectionUtils.GetMemberUnderlyingType(memberInfo);
				reflectionObject.Members[text] = reflectionMember;
			}
			return reflectionObject;
		}
	}
}
