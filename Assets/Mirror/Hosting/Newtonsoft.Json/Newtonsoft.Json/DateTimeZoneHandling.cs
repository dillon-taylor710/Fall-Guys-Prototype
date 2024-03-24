using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json
{
	[Preserve]
	public enum DateTimeZoneHandling
	{
		Local,
		Utc,
		Unspecified,
		RoundtripKind
	}
}
