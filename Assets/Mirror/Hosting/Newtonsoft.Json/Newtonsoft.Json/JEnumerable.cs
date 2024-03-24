using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Newtonsoft.Json.Linq
{
	[DefaultMember("Item")]
	[Preserve]
	public struct JEnumerable<T> : IEnumerable<T>, IJEnumerable<T>, IEquatable<JEnumerable<T>>, IEnumerable where T : JToken
	{
		public static readonly JEnumerable<T> Empty = new JEnumerable<T>(Enumerable.Empty<T>());

		private readonly IEnumerable<T> _enumerable;

		public JEnumerable(IEnumerable<T> enumerable)
		{
			ValidationUtils.ArgumentNotNull(enumerable, "enumerable");
			_enumerable = enumerable;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (_enumerable == null)
			{
				return Empty.GetEnumerator();
			}
			return _enumerable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Equals(JEnumerable<T> other)
		{
			return object.Equals(_enumerable, other._enumerable);
		}

		public override bool Equals(object obj)
		{
			if (obj is JEnumerable<T>)
			{
				return Equals((JEnumerable<T>)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (_enumerable == null)
			{
				return 0;
			}
			return _enumerable.GetHashCode();
		}
	}
}
