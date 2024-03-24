using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Bson
{
	[Preserve]
	internal abstract class BsonToken
	{
		public abstract BsonType Type
		{
			get;
		}

		public BsonToken Parent
		{
			get;
			set;
		}

		public int CalculatedSize
		{
			get;
			set;
		}
	}
}
