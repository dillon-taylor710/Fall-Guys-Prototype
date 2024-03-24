using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json
{
	[Flags]
	[Preserve]
	public enum TypeNameHandling
	{
		None = 0x0,
		Objects = 0x1,
		Arrays = 0x2,
		All = 0x3,
		Auto = 0x4
	}
}
