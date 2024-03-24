using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal class TypeInformation
	{
		public Type Type
		{
			get;
			set;
		}

		public PrimitiveTypeCode TypeCode
		{
			get;
			set;
		}
	}
}
