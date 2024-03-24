using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	[Preserve]
	public sealed class JsonPropertyAttribute : Attribute
	{
		internal NullValueHandling? _nullValueHandling;

		internal DefaultValueHandling? _defaultValueHandling;

		internal ReferenceLoopHandling? _referenceLoopHandling;

		internal ObjectCreationHandling? _objectCreationHandling;

		internal TypeNameHandling? _typeNameHandling;

		internal bool? _isReference;

		internal int? _order;

		internal Required? _required;

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

		public string PropertyName
		{
			get;
			set;
		}

		public JsonPropertyAttribute(string propertyName)
		{
			this.PropertyName = propertyName;
		}
	}
}
