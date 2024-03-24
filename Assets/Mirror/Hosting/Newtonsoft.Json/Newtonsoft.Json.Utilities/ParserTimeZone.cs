using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal enum ParserTimeZone
	{
		Unspecified,
		Utc,
		LocalWestOfUtc,
		LocalEastOfUtc
	}
}
