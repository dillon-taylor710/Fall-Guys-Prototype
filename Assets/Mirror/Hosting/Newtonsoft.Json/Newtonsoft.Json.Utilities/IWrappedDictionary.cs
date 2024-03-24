using Newtonsoft.Json.Shims;
using System.Collections;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal interface IWrappedDictionary : ICollection, IDictionary, IEnumerable
	{
		object UnderlyingDictionary
		{
			get;
		}
	}
}
