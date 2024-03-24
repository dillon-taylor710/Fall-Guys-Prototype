using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json
{
	[Preserve]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public abstract class JsonContainerAttribute : Attribute
	{
		internal bool? _isReference;

		internal bool? _itemIsReference;

		internal ReferenceLoopHandling? _itemReferenceLoopHandling;

		internal TypeNameHandling? _itemTypeNameHandling;

		public Type ItemConverterType
		{
			get;
		}

		public object[] ItemConverterParameters
		{
			get;
		}
	}
}
