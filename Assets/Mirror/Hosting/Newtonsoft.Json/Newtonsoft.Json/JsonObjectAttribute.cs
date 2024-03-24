using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json
{
	[Preserve]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
	public sealed class JsonObjectAttribute : JsonContainerAttribute
	{
		private MemberSerialization _memberSerialization;

		internal Required? _itemRequired;

		public MemberSerialization MemberSerialization
		{
			get
			{
				return _memberSerialization;
			}
		}
	}
}
