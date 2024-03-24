using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public class JsonISerializableContract : JsonContainerContract
	{
		public ObjectConstructor<object> ISerializableCreator
		{
			get;
			set;
		}

		public JsonISerializableContract(Type underlyingType)
			: base(underlyingType)
		{
			ContractType = JsonContractType.Serializable;
		}
	}
}
