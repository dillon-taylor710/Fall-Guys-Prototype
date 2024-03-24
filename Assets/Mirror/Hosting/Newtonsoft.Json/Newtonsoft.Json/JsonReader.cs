using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Newtonsoft.Json
{
	[Preserve]
	public abstract class JsonReader : IDisposable
	{
		protected internal enum State
		{
			Start,
			Complete,
			Property,
			ObjectStart,
			Object,
			ArrayStart,
			Array,
			Closed,
			PostValue,
			ConstructorStart,
			Constructor,
			Error,
			Finished
		}

		private JsonToken _tokenType;

		private object _value;

		internal char _quoteChar;

		internal State _currentState;

		private JsonPosition _currentPosition;

		private CultureInfo _culture;

		private DateTimeZoneHandling _dateTimeZoneHandling;

		private int? _maxDepth;

		private bool _hasExceededMaxDepth;

		internal DateParseHandling _dateParseHandling;

		internal FloatParseHandling _floatParseHandling;

		private string _dateFormatString;

		private List<JsonPosition> _stack;

		protected State CurrentState
		{
			get
			{
				return _currentState;
			}
		}

		public bool CloseInput
		{
			get;
			set;
		}

		public bool SupportMultipleContent
		{
			get;
			set;
		}

		public DateTimeZoneHandling DateTimeZoneHandling
		{
			get
			{
				return _dateTimeZoneHandling;
			}
			set
			{
				if (value < DateTimeZoneHandling.Local || value > DateTimeZoneHandling.RoundtripKind)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_dateTimeZoneHandling = value;
			}
		}

		public DateParseHandling DateParseHandling
		{
			get
			{
				return _dateParseHandling;
			}
			set
			{
				if (value < DateParseHandling.None || value > DateParseHandling.DateTimeOffset)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_dateParseHandling = value;
			}
		}

		public FloatParseHandling FloatParseHandling
		{
			get
			{
				return _floatParseHandling;
			}
			set
			{
				if (value < FloatParseHandling.Double || value > FloatParseHandling.Decimal)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_floatParseHandling = value;
			}
		}

		public string DateFormatString
		{
			get
			{
				return _dateFormatString;
			}
			set
			{
				_dateFormatString = value;
			}
		}

		public int? MaxDepth
		{
			get
			{
				return _maxDepth;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentException("Value must be positive.", "value");
				}
				_maxDepth = value;
			}
		}

		public virtual JsonToken TokenType
		{
			get
			{
				return _tokenType;
			}
		}

		public virtual object Value
		{
			get
			{
				return _value;
			}
		}

		public virtual Type ValueType
		{
			get
			{
				if (_value == null)
				{
					return null;
				}
				return _value.GetType();
			}
		}

		public virtual int Depth
		{
			get
			{
				int num = (_stack != null) ? _stack.Count : 0;
				if (JsonTokenUtils.IsStartToken(TokenType) || _currentPosition.Type == JsonContainerType.None)
				{
					return num;
				}
				return num + 1;
			}
		}

		public virtual string Path
		{
			get
			{
				if (_currentPosition.Type == JsonContainerType.None)
				{
					return string.Empty;
				}
				JsonPosition? currentPosition = (_currentState != State.ArrayStart && _currentState != State.ConstructorStart && _currentState != State.ObjectStart) ? new JsonPosition?(_currentPosition) : null;
				return JsonPosition.BuildPath(_stack, currentPosition);
			}
		}

		public CultureInfo Culture
		{
			get
			{
				return _culture ?? CultureInfo.InvariantCulture;
			}
			set
			{
				_culture = value;
			}
		}

		protected JsonReader()
		{
			_currentState = State.Start;
			_dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
			_dateParseHandling = DateParseHandling.DateTime;
			_floatParseHandling = FloatParseHandling.Double;
			CloseInput = true;
		}

		internal JsonPosition GetPosition(int depth)
		{
			if (_stack != null && depth < _stack.Count)
			{
				return _stack[depth];
			}
			return _currentPosition;
		}

		private void Push(JsonContainerType value)
		{
			UpdateScopeWithFinishedValue();
			if (_currentPosition.Type == JsonContainerType.None)
			{
				_currentPosition = new JsonPosition(value);
			}
			else
			{
				if (_stack == null)
				{
					_stack = new List<JsonPosition>();
				}
				_stack.Add(_currentPosition);
				_currentPosition = new JsonPosition(value);
				if (_maxDepth.HasValue && Depth + 1 > _maxDepth && !_hasExceededMaxDepth)
				{
					_hasExceededMaxDepth = true;
					throw JsonReaderException.Create(this, "The reader's MaxDepth of {0} has been exceeded.".FormatWith(CultureInfo.InvariantCulture, _maxDepth));
				}
			}
		}

		private JsonContainerType Pop()
		{
			JsonPosition currentPosition;
			if (_stack != null && _stack.Count > 0)
			{
				currentPosition = _currentPosition;
				_currentPosition = _stack[_stack.Count - 1];
				_stack.RemoveAt(_stack.Count - 1);
			}
			else
			{
				currentPosition = _currentPosition;
				_currentPosition = default(JsonPosition);
			}
			if (_maxDepth.HasValue && Depth <= _maxDepth)
			{
				_hasExceededMaxDepth = false;
			}
			return currentPosition.Type;
		}

		private JsonContainerType Peek()
		{
			return _currentPosition.Type;
		}

		public abstract bool Read();

		public virtual int? ReadAsInt32()
		{
			JsonToken contentToken = GetContentToken();
			switch (contentToken)
			{
			case JsonToken.None:
			case JsonToken.Null:
			case JsonToken.EndArray:
				return null;
			case JsonToken.Integer:
			case JsonToken.Float:
				if (!(Value is int))
				{
					SetToken(JsonToken.Integer, Convert.ToInt32(Value, CultureInfo.InvariantCulture), false);
				}
				return (int)Value;
			case JsonToken.String:
			{
				string s = (string)Value;
				return ReadInt32String(s);
			}
			default:
				throw JsonReaderException.Create(this, "Error reading integer. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
			}
		}

		internal int? ReadInt32String(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				SetToken(JsonToken.Null, null, false);
				return null;
			}
			int result;
			if (int.TryParse(s, NumberStyles.Integer, Culture, out result))
			{
				SetToken(JsonToken.Integer, result, false);
				return result;
			}
			SetToken(JsonToken.String, s, false);
			throw JsonReaderException.Create(this, "Could not convert string to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
		}

		public virtual string ReadAsString()
		{
			JsonToken contentToken = GetContentToken();
			switch (contentToken)
			{
			case JsonToken.None:
			case JsonToken.Null:
			case JsonToken.EndArray:
				return null;
			case JsonToken.String:
				return (string)Value;
			default:
				if (JsonTokenUtils.IsPrimitiveToken(contentToken) && Value != null)
				{
					string text = (Value is IFormattable) ? ((IFormattable)Value).ToString(null, Culture) : ((!(Value is Uri)) ? Value.ToString() : ((Uri)Value).OriginalString);
					SetToken(JsonToken.String, text, false);
					return text;
				}
				throw JsonReaderException.Create(this, "Error reading string. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
			}
		}

		public virtual byte[] ReadAsBytes()
		{
			JsonToken contentToken = GetContentToken();
			if (contentToken == JsonToken.None)
			{
				return null;
			}
			if (TokenType != JsonToken.StartObject)
			{
				switch (contentToken)
				{
				case JsonToken.String:
				{
					string text = (string)Value;
					Guid g;
					byte[] array2 = (text.Length == 0) ? new byte[0] : ((!ConvertUtils.TryConvertGuid(text, out g)) ? Convert.FromBase64String(text) : g.ToByteArray());
					SetToken(JsonToken.Bytes, array2, false);
					return array2;
				}
				case JsonToken.Null:
				case JsonToken.EndArray:
					return null;
				case JsonToken.Bytes:
					if (ValueType == typeof(Guid))
					{
						byte[] array = ((Guid)Value).ToByteArray();
						SetToken(JsonToken.Bytes, array, false);
						return array;
					}
					return (byte[])Value;
				case JsonToken.StartArray:
					return ReadArrayIntoByteArray();
				default:
					throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
				}
			}
			ReadIntoWrappedTypeObject();
			byte[] array3 = ReadAsBytes();
			ReaderReadAndAssert();
			if (TokenType != JsonToken.EndObject)
			{
				throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
			}
			SetToken(JsonToken.Bytes, array3, false);
			return array3;
		}

		internal byte[] ReadArrayIntoByteArray()
		{
			List<byte> list = new List<byte>();
			while (true)
			{
				JsonToken contentToken = GetContentToken();
				switch (contentToken)
				{
				case JsonToken.None:
					throw JsonReaderException.Create(this, "Unexpected end when reading bytes.");
				case JsonToken.Integer:
					break;
				case JsonToken.EndArray:
				{
					byte[] array = list.ToArray();
					SetToken(JsonToken.Bytes, array, false);
					return array;
				}
				default:
					throw JsonReaderException.Create(this, "Unexpected token when reading bytes: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
				}
				list.Add(Convert.ToByte(Value, CultureInfo.InvariantCulture));
			}
		}

		public virtual double? ReadAsDouble()
		{
			JsonToken contentToken = GetContentToken();
			switch (contentToken)
			{
			case JsonToken.None:
			case JsonToken.Null:
			case JsonToken.EndArray:
				return null;
			case JsonToken.Integer:
			case JsonToken.Float:
				if (!(Value is double))
				{
					double num = Convert.ToDouble(Value, CultureInfo.InvariantCulture);
					SetToken(JsonToken.Float, num, false);
				}
				return (double)Value;
			case JsonToken.String:
				return ReadDoubleString((string)Value);
			default:
				throw JsonReaderException.Create(this, "Error reading double. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
			}
		}

		internal double? ReadDoubleString(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				SetToken(JsonToken.Null, null, false);
				return null;
			}
			double result;
			if (double.TryParse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, Culture, out result))
			{
				SetToken(JsonToken.Float, result, false);
				return result;
			}
			SetToken(JsonToken.String, s, false);
			throw JsonReaderException.Create(this, "Could not convert string to double: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
		}

		public virtual bool? ReadAsBoolean()
		{
			JsonToken contentToken = GetContentToken();
			switch (contentToken)
			{
			case JsonToken.None:
			case JsonToken.Null:
			case JsonToken.EndArray:
				return null;
			case JsonToken.Integer:
			case JsonToken.Float:
			{
				bool flag = Convert.ToBoolean(Value, CultureInfo.InvariantCulture);
				SetToken(JsonToken.Boolean, flag, false);
				return flag;
			}
			case JsonToken.String:
				return ReadBooleanString((string)Value);
			case JsonToken.Boolean:
				return (bool)Value;
			default:
				throw JsonReaderException.Create(this, "Error reading boolean. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
			}
		}

		internal bool? ReadBooleanString(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				SetToken(JsonToken.Null, null, false);
				return null;
			}
			bool result;
			if (bool.TryParse(s, out result))
			{
				SetToken(JsonToken.Boolean, result, false);
				return result;
			}
			SetToken(JsonToken.String, s, false);
			throw JsonReaderException.Create(this, "Could not convert string to boolean: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
		}

		public virtual decimal? ReadAsDecimal()
		{
			JsonToken contentToken = GetContentToken();
			switch (contentToken)
			{
			case JsonToken.None:
			case JsonToken.Null:
			case JsonToken.EndArray:
				return null;
			case JsonToken.Integer:
			case JsonToken.Float:
				if (!(Value is decimal))
				{
					SetToken(JsonToken.Float, Convert.ToDecimal(Value, CultureInfo.InvariantCulture), false);
				}
				return (decimal)Value;
			case JsonToken.String:
				return ReadDecimalString((string)Value);
			default:
				throw JsonReaderException.Create(this, "Error reading decimal. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
			}
		}

		internal decimal? ReadDecimalString(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				SetToken(JsonToken.Null, null, false);
				return null;
			}
			decimal result;
			if (decimal.TryParse(s, NumberStyles.Number, Culture, out result))
			{
				SetToken(JsonToken.Float, result, false);
				return result;
			}
			SetToken(JsonToken.String, s, false);
			throw JsonReaderException.Create(this, "Could not convert string to decimal: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
		}

		public virtual DateTime? ReadAsDateTime()
		{
			switch (GetContentToken())
			{
			case JsonToken.None:
			case JsonToken.Null:
			case JsonToken.EndArray:
				return null;
			case JsonToken.Date:
				if (Value is DateTimeOffset)
				{
					SetToken(JsonToken.Date, ((DateTimeOffset)Value).DateTime, false);
				}
				return (DateTime)Value;
			case JsonToken.String:
			{
				string s = (string)Value;
				return ReadDateTimeString(s);
			}
			default:
				throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
			}
		}

		internal DateTime? ReadDateTimeString(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				SetToken(JsonToken.Null, null, false);
				return null;
			}
			DateTime dt;
			if (DateTimeUtils.TryParseDateTime(s, DateTimeZoneHandling, _dateFormatString, Culture, out dt))
			{
				dt = DateTimeUtils.EnsureDateTime(dt, DateTimeZoneHandling);
				SetToken(JsonToken.Date, dt, false);
				return dt;
			}
			if (DateTime.TryParse(s, Culture, DateTimeStyles.RoundtripKind, out dt))
			{
				dt = DateTimeUtils.EnsureDateTime(dt, DateTimeZoneHandling);
				SetToken(JsonToken.Date, dt, false);
				return dt;
			}
			throw JsonReaderException.Create(this, "Could not convert string to DateTime: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
		}

		public virtual DateTimeOffset? ReadAsDateTimeOffset()
		{
			JsonToken contentToken = GetContentToken();
			switch (contentToken)
			{
			case JsonToken.None:
			case JsonToken.Null:
			case JsonToken.EndArray:
				return null;
			case JsonToken.Date:
				if (Value is DateTime)
				{
					SetToken(JsonToken.Date, new DateTimeOffset((DateTime)Value), false);
				}
				return (DateTimeOffset)Value;
			case JsonToken.String:
			{
				string s = (string)Value;
				return ReadDateTimeOffsetString(s);
			}
			default:
				throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
			}
		}

		internal DateTimeOffset? ReadDateTimeOffsetString(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				SetToken(JsonToken.Null, null, false);
				return null;
			}
			DateTimeOffset dt;
			if (DateTimeUtils.TryParseDateTimeOffset(s, _dateFormatString, Culture, out dt))
			{
				SetToken(JsonToken.Date, dt, false);
				return dt;
			}
			if (DateTimeOffset.TryParse(s, Culture, DateTimeStyles.RoundtripKind, out dt))
			{
				SetToken(JsonToken.Date, dt, false);
				return dt;
			}
			SetToken(JsonToken.String, s, false);
			throw JsonReaderException.Create(this, "Could not convert string to DateTimeOffset: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
		}

		internal void ReaderReadAndAssert()
		{
			if (!Read())
			{
				throw CreateUnexpectedEndException();
			}
		}

		internal JsonReaderException CreateUnexpectedEndException()
		{
			return JsonReaderException.Create(this, "Unexpected end when reading JSON.");
		}

		internal void ReadIntoWrappedTypeObject()
		{
			ReaderReadAndAssert();
			if (Value.ToString() == "$type")
			{
				ReaderReadAndAssert();
				if (Value != null && Value.ToString().StartsWith("System.Byte[]", StringComparison.Ordinal))
				{
					ReaderReadAndAssert();
					if (Value.ToString() == "$value")
					{
						return;
					}
				}
			}
			throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.StartObject));
		}

		public void Skip()
		{
			if (TokenType == JsonToken.PropertyName)
			{
				Read();
			}
			if (JsonTokenUtils.IsStartToken(TokenType))
			{
				int depth = Depth;
				while (Read() && depth < Depth)
				{
				}
			}
		}

		protected void SetToken(JsonToken newToken)
		{
			SetToken(newToken, null, true);
		}

		protected void SetToken(JsonToken newToken, object value)
		{
			SetToken(newToken, value, true);
		}

		internal void SetToken(JsonToken newToken, object value, bool updateIndex)
		{
			_tokenType = newToken;
			_value = value;
			switch (newToken)
			{
			case JsonToken.Comment:
				break;
			case JsonToken.StartObject:
				_currentState = State.ObjectStart;
				Push(JsonContainerType.Object);
				break;
			case JsonToken.StartArray:
				_currentState = State.ArrayStart;
				Push(JsonContainerType.Array);
				break;
			case JsonToken.StartConstructor:
				_currentState = State.ConstructorStart;
				Push(JsonContainerType.Constructor);
				break;
			case JsonToken.EndObject:
				ValidateEnd(JsonToken.EndObject);
				break;
			case JsonToken.EndArray:
				ValidateEnd(JsonToken.EndArray);
				break;
			case JsonToken.EndConstructor:
				ValidateEnd(JsonToken.EndConstructor);
				break;
			case JsonToken.PropertyName:
				_currentState = State.Property;
				_currentPosition.PropertyName = (string)value;
				break;
			case JsonToken.Raw:
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Null:
			case JsonToken.Undefined:
			case JsonToken.Date:
			case JsonToken.Bytes:
				SetPostValueState(updateIndex);
				break;
			}
		}

		internal void SetPostValueState(bool updateIndex)
		{
			if (Peek() != 0)
			{
				_currentState = State.PostValue;
			}
			else
			{
				SetFinished();
			}
			if (updateIndex)
			{
				UpdateScopeWithFinishedValue();
			}
		}

		private void UpdateScopeWithFinishedValue()
		{
			if (_currentPosition.HasIndex)
			{
				_currentPosition.Position++;
			}
		}

		private void ValidateEnd(JsonToken endToken)
		{
			JsonContainerType jsonContainerType = Pop();
			if (GetTypeForCloseToken(endToken) != jsonContainerType)
			{
				throw JsonReaderException.Create(this, "JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, jsonContainerType));
			}
			if (Peek() != 0)
			{
				_currentState = State.PostValue;
			}
			else
			{
				SetFinished();
			}
		}

		protected void SetStateBasedOnCurrent()
		{
			JsonContainerType jsonContainerType = Peek();
			switch (jsonContainerType)
			{
			case JsonContainerType.Object:
				_currentState = State.Object;
				break;
			case JsonContainerType.Array:
				_currentState = State.Array;
				break;
			case JsonContainerType.Constructor:
				_currentState = State.Constructor;
				break;
			case JsonContainerType.None:
				SetFinished();
				break;
			default:
				throw JsonReaderException.Create(this, "While setting the reader state back to current object an unexpected JsonType was encountered: {0}".FormatWith(CultureInfo.InvariantCulture, jsonContainerType));
			}
		}

		private void SetFinished()
		{
			if (SupportMultipleContent)
			{
				_currentState = State.Start;
			}
			else
			{
				_currentState = State.Finished;
			}
		}

		private JsonContainerType GetTypeForCloseToken(JsonToken token)
		{
			switch (token)
			{
			case JsonToken.EndObject:
				return JsonContainerType.Object;
			case JsonToken.EndArray:
				return JsonContainerType.Array;
			case JsonToken.EndConstructor:
				return JsonContainerType.Constructor;
			default:
				throw JsonReaderException.Create(this, "Not a valid close JsonToken: {0}".FormatWith(CultureInfo.InvariantCulture, token));
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_currentState != State.Closed && disposing)
			{
				Close();
			}
		}

		public virtual void Close()
		{
			_currentState = State.Closed;
			_tokenType = JsonToken.None;
			_value = null;
		}

		internal void ReadAndAssert()
		{
			if (!Read())
			{
				throw JsonSerializationException.Create(this, "Unexpected end when reading JSON.");
			}
		}

		internal bool ReadAndMoveToContent()
		{
			if (Read())
			{
				return MoveToContent();
			}
			return false;
		}

		internal bool MoveToContent()
		{
			JsonToken tokenType = TokenType;
			while (tokenType == JsonToken.None || tokenType == JsonToken.Comment)
			{
				if (!Read())
				{
					return false;
				}
				tokenType = TokenType;
			}
			return true;
		}

		private JsonToken GetContentToken()
		{
			JsonToken tokenType;
			do
			{
				if (!Read())
				{
					SetToken(JsonToken.None);
					return JsonToken.None;
				}
				tokenType = TokenType;
			}
			while (tokenType == JsonToken.Comment);
			return tokenType;
		}
	}
}
