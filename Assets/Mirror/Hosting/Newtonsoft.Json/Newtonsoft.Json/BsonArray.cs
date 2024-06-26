using Newtonsoft.Json.Shims;
using System.Collections;
using System.Collections.Generic;

namespace Newtonsoft.Json.Bson
{
	[Preserve]
	internal class BsonArray : BsonToken, IEnumerable<BsonToken>, IEnumerable
	{
		private readonly List<BsonToken> _children = new List<BsonToken>();

		public override BsonType Type
		{
			get
			{
				return BsonType.Array;
			}
		}

		public void Add(BsonToken token)
		{
			_children.Add(token);
			token.Parent = this;
		}

		public IEnumerator<BsonToken> GetEnumerator()
		{
			return _children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
