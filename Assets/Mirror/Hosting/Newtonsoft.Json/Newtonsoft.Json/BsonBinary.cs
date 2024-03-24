using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Bson
{
	[Preserve]
	internal class BsonBinary : BsonValue
	{
		public BsonBinaryType BinaryType
		{
			get;
			set;
		}

		public BsonBinary(byte[] value, BsonBinaryType binaryType)
			: base(value, BsonType.Binary)
		{
			BinaryType = binaryType;
		}
	}
}
