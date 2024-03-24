using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json
{
	[Preserve]
	[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
	public sealed class JsonConstructorAttribute : Attribute
	{
	}
}
