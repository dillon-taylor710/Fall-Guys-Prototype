using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json
{
	[Preserve]
	internal enum ReadType
	{
		Read,
		ReadAsInt32,
		ReadAsBytes,
		ReadAsString,
		ReadAsDecimal,
		ReadAsDateTime,
		ReadAsDateTimeOffset,
		ReadAsDouble,
		ReadAsBoolean
	}
}
