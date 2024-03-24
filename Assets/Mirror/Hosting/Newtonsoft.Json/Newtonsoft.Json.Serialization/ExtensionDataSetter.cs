using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public delegate void ExtensionDataSetter(object o, string key, object value);
}
