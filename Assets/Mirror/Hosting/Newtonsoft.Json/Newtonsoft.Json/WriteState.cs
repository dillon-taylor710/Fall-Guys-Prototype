using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json
{
	[Preserve]
	public enum WriteState
	{
		Error,
		Closed,
		Object,
		Array,
		Constructor,
		Property,
		Start
	}
}
