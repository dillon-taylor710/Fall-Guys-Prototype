using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public delegate object ObjectConstructor<T>(params object[] args);
}
