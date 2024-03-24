using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json
{
	[Preserve]
	public enum Required
	{
		Default,
		AllowNull,
		Always,
		DisallowNull
	}
}
