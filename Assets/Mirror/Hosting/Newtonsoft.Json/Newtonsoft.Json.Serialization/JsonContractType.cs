using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	internal enum JsonContractType
	{
		None,
		Object,
		Array,
		Primitive,
		String,
		Dictionary,
		Dynamic,
		Serializable,
		Linq
	}
}
