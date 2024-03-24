using Newtonsoft.Json.Shims;
using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal class ReflectionMember
	{
		public Type MemberType
		{
			get;
			set;
		}

		public Func<object, object> Getter
		{
			get;
			set;
		}

		public Action<object, object> Setter
		{
			[CompilerGenerated]
			set
			{
				Setter = value;
			}
		}
	}
}
