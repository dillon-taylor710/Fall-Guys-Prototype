using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json
{
	[Preserve]
	public enum StringEscapeHandling
	{
		Default,
		EscapeNonAscii,
		EscapeHtml
	}
}
