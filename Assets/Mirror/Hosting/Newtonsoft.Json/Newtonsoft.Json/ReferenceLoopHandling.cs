using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json
{
	[Preserve]
	public enum ReferenceLoopHandling
	{
		Error,
		Ignore,
		Serialize
	}
}
