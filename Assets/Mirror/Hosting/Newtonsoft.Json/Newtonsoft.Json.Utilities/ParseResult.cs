using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal enum ParseResult
	{
		None,
		Success,
		Overflow,
		Invalid
	}
}
