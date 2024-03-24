using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Linq
{
	[Preserve]
	public enum JTokenType
	{
		None,
		Object,
		Array,
		Constructor,
		Property,
		Comment,
		Integer,
		Float,
		String,
		Boolean,
		Null,
		Undefined,
		Date,
		Raw,
		Bytes,
		Guid,
		Uri,
		TimeSpan
	}
}
