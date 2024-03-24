using Newtonsoft.Json.Shims;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Newtonsoft.Json.Linq
{
	[DefaultMember("Item")]
	[Preserve]
	public interface IJEnumerable<T> : IEnumerable<T>, IEnumerable where T : JToken
	{
	}
}
