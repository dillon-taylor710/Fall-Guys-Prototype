using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json
{
	[Preserve]
	internal enum JsonContainerType
	{
		None,
		Object,
		Array,
		Constructor
	}
}
