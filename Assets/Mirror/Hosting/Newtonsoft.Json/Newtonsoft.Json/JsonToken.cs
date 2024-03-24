using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json
{
	[Preserve]
	public enum JsonToken
	{
		None,
		StartObject,
		StartArray,
		StartConstructor,
		PropertyName,
		Comment,
		Raw,
		Integer,
		Float,
		String,
		Boolean,
		Null,
		Undefined,
		EndObject,
		EndArray,
		EndConstructor,
		Date,
		Bytes
	}
}
