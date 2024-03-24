using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public class JsonStringContract : JsonPrimitiveContract
	{
		public JsonStringContract(Type underlyingType)
			: base(underlyingType)
		{
			ContractType = JsonContractType.String;
		}
	}
}
