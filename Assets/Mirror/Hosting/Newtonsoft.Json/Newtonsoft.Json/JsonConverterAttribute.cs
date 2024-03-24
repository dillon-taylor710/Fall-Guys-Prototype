using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json
{
	[Preserve]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class JsonConverterAttribute : Attribute
	{
		private readonly Type _converterType;

		public Type ConverterType
		{
			get
			{
				return _converterType;
			}
		}

		public object[] ConverterParameters
		{
			get;
		}
	}
}
