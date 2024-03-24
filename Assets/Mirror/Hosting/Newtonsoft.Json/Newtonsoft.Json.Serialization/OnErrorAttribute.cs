using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public sealed class OnErrorAttribute : Attribute
	{
	}
}
