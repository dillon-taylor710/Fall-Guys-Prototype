using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Bson
{
	[Preserve]
	internal class BsonProperty
	{
		public BsonString Name
		{
			get;
			set;
		}

		public BsonToken Value
		{
			get;
			set;
		}
	}
}
