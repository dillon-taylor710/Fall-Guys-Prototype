using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json
{
	[Preserve]
	[Flags]
	public enum DefaultValueHandling
	{
		Include = 0x0,
		Ignore = 0x1,
		Populate = 0x2,
		IgnoreAndPopulate = 0x3
	}
}
