using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json
{
	[Preserve]
	public interface IJsonLineInfo
	{
		int LineNumber
		{
			get;
		}

		int LinePosition
		{
			get;
		}

		bool HasLineInfo();
	}
}
