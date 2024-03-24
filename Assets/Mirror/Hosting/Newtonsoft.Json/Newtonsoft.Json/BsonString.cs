using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Bson
{
	[Preserve]
	internal class BsonString : BsonValue
	{
		public int ByteCount
		{
			get;
			set;
		}

		public bool IncludeLength
		{
			get;
			set;
		}

		public BsonString(object value, bool includeLength)
			: base(value, BsonType.String)
		{
			IncludeLength = includeLength;
		}
	}
}
