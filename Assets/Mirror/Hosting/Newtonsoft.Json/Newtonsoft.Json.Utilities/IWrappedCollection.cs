using Newtonsoft.Json.Shims;
using System.Collections;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal interface IWrappedCollection : ICollection, IEnumerable, IList
	{
		object UnderlyingCollection
		{
			get;
		}
	}
}
