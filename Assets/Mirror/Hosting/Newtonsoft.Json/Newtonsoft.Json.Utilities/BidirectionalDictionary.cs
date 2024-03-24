using Newtonsoft.Json.Shims;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal class BidirectionalDictionary<TFirst, TSecond>
	{
		private readonly IDictionary<TFirst, TSecond> _firstToSecond;

		private readonly IDictionary<TSecond, TFirst> _secondToFirst;

		private readonly string _duplicateFirstErrorMessage;

		private readonly string _duplicateSecondErrorMessage;

		public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer)
			: this(firstEqualityComparer, secondEqualityComparer, "Duplicate item already exists for '{0}'.", "Duplicate item already exists for '{0}'.")
		{
		}

		public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer, string duplicateFirstErrorMessage, string duplicateSecondErrorMessage)
		{
			_firstToSecond = new Dictionary<TFirst, TSecond>(firstEqualityComparer);
			_secondToFirst = new Dictionary<TSecond, TFirst>(secondEqualityComparer);
			_duplicateFirstErrorMessage = duplicateFirstErrorMessage;
			_duplicateSecondErrorMessage = duplicateSecondErrorMessage;
		}

		public void Set(TFirst first, TSecond second)
		{
			TSecond value;
			if (_firstToSecond.TryGetValue(first, out value) && !value.Equals(second))
			{
				throw new ArgumentException(_duplicateFirstErrorMessage.FormatWith(CultureInfo.InvariantCulture, first));
			}
			TFirst value2;
			if (_secondToFirst.TryGetValue(second, out value2) && !value2.Equals(first))
			{
				throw new ArgumentException(_duplicateSecondErrorMessage.FormatWith(CultureInfo.InvariantCulture, second));
			}
			_firstToSecond.Add(first, second);
			_secondToFirst.Add(second, first);
		}

		public bool TryGetByFirst(TFirst first, out TSecond second)
		{
			return _firstToSecond.TryGetValue(first, out second);
		}

		public bool TryGetBySecond(TSecond second, out TFirst first)
		{
			return _secondToFirst.TryGetValue(second, out first);
		}
	}
}
