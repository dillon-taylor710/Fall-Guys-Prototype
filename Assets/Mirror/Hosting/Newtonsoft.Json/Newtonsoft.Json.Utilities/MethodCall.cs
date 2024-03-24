using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal delegate TResult MethodCall<T, TResult>(T target, params object[] args);
}
