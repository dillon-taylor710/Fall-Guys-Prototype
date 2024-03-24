using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Newtonsoft.Json
{
	[Preserve]
	public abstract class JsonWriter : IDisposable
	{
		internal enum State
		{
			Start,
			Property,
			ObjectStart,
			Object,
			ArrayStart,
			Array,
			ConstructorStart,
			Constructor,
			Closed,
			Error
		}

		private static readonly State[][] StateArray;

		internal static readonly State[][] StateArrayTempate;

		private List<JsonPosition> _stack;

		private JsonPosition _currentPosition;

		private State _currentState;

		private Formatting _formatting;

		private DateFormatHandling _dateFormatHandling;

		private DateTimeZoneHandling _dateTimeZoneHandling;

		private StringEscapeHandling _stringEscapeHandling;

		private FloatFormatHandling _floatFormatHandling;

		private string _dateFormatString;

		private CultureInfo _culture;

		public bool CloseOutput
		{
			get;
			set;
		}

		protected internal int Top
		{
			get
			{
				int num = (_stack != null) ? _stack.Count : 0;
				if (Peek() != 0)
				{
					num++;
				}
				return num;
			}
		}

		public WriteState WriteState
		{
			get
			{
				switch (_currentState)
				{
				case State.Error:
					return WriteState.Error;
				case State.Closed:
					return WriteState.Closed;
				case State.ObjectStart:
				case State.Object:
					return WriteState.Object;
				case State.ArrayStart:
				case State.Array:
					return WriteState.Array;
				case State.ConstructorStart:
				case State.Constructor:
					return WriteState.Constructor;
				case State.Property:
					return WriteState.Property;
				case State.Start:
					return WriteState.Start;
				default:
					throw JsonWriterException.Create(this, "Invalid state: " + _currentState, null);
				}
			}
		}

		internal string ContainerPath
		{
			get
			{
				if (_currentPosition.Type == JsonContainerType.None || _stack == null)
				{
					return string.Empty;
				}
				return JsonPosition.BuildPath(_stack, null);
			}
		}

		public string Path
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

		public Formatting Formatting
		{
			get
			{
				return _formatting;
			}
			set
			{
				if (value < Formatting.None || value > Formatting.Indented)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_formatting = value;
			}
		}

		public DateFormatHandling DateFormatHandling
		{
			get
			{
				return _dateFormatHandling;
			}
			set
			{
				if (value < DateFormatHandling.IsoDateFormat || value > DateFormatHandling.MicrosoftDateFormat)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_dateFormatHandling = value;
			}
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

		public StringEscapeHandling StringEscapeHandling
		{
			get
			{
				return _stringEscapeHandling;
			}
			set
			{
				if (value < StringEscapeHandling.Default || value > StringEscapeHandling.EscapeHtml)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_stringEscapeHandling = value;
				OnStringEscapeHandlingChanged();
			}
		}

		public FloatFormatHandling FloatFormatHandling
		{
			get
			{
				return _floatFormatHandling;
			}
			set
			{
				if (value < FloatFormatHandling.String || value > FloatFormatHandling.DefaultValue)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_floatFormatHandling = value;
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

		static JsonWriter()
		{
			StateArrayTempate = new State[8][]
			{
				new State[10]
				{
					State.Error,
					State.Error,
					State.Error,
					State.Error,
					State.Error,
					State.Error,
					State.Error,
					State.Error,
					State.Error,
					State.Error
				},
				new State[10]
				{
					State.ObjectStart,
					State.ObjectStart,
					State.Error,
					State.Error,
					State.ObjectStart,
					State.ObjectStart,
					State.ObjectStart,
					State.ObjectStart,
					State.Error,
					State.Error
				},
				new State[10]
				{
					State.ArrayStart,
					State.ArrayStart,
					State.Error,
					State.Error,
					State.ArrayStart,
					State.ArrayStart,
					State.ArrayStart,
					State.ArrayStart,
					State.Error,
					State.Error
				},
				new State[10]
				{
					State.ConstructorStart,
					State.ConstructorStart,
					State.Error,
					State.Error,
					State.ConstructorStart,
					State.ConstructorStart,
					State.ConstructorStart,
					State.ConstructorStart,
					State.Error,
					State.Error
				},
				new State[10]
				{
					State.Property,
					State.Error,
					State.Property,
					State.Property,
					State.Error,
					State.Error,
					State.Error,
					State.Error,
					State.Error,
					State.Error
				},
				new State[10]
				{
					State.Start,
					State.Property,
					State.ObjectStart,
					State.Object,
					State.ArrayStart,
					State.Array,
					State.Constructor,
					State.Constructor,
					State.Error,
					State.Error
				},
				new State[10]
				{
					State.Start,
					State.Property,
					State.ObjectStart,
					State.Object,
					State.ArrayStart,
					State.Array,
					State.Constructor,
					State.Constructor,
					State.Error,
					State.Error
				},
				new State[10]
				{
					State.Start,
					State.Object,
					State.Error,
					State.Error,
					State.Array,
					State.Array,
					State.Constructor,
					State.Constructor,
					State.Error,
					State.Error
				}
			};
			StateArray = BuildStateArray();
		}

		protected JsonWriter()
		{
			_currentState = State.Start;
			_formatting = Formatting.None;
			_dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
			CloseOutput = true;
		}

		internal static State[][] BuildStateArray()
		{
			List<State[]> list = StateArrayTempate.ToList();
			State[] item = StateArrayTempate[0];
			State[] item2 = StateArrayTempate[7];
			foreach (JsonToken value in EnumUtils.GetValues(typeof(JsonToken)))
			{
				if (list.Count <= (int)value)
				{
					switch (value)
					{
					case JsonToken.Integer:
					case JsonToken.Float:
					case JsonToken.String:
					case JsonToken.Boolean:
					case JsonToken.Null:
					case JsonToken.Undefined:
					case JsonToken.Date:
					case JsonToken.Bytes:
						list.Add(item2);
						break;
					default:
						list.Add(item);
						break;
					}
				}
			}
			return list.ToArray();
		}

		internal virtual void OnStringEscapeHandlingChanged()
		{
		}

		internal void UpdateScopeWithFinishedValue()
		{
			if (_currentPosition.HasIndex)
			{
				_currentPosition.Position++;
			}
		}

		private void Push(JsonContainerType value)
		{
			if (_currentPosition.Type != 0)
			{
				if (_stack == null)
				{
					_stack = new List<JsonPosition>();
				}
				_stack.Add(_currentPosition);
			}
			_currentPosition = new JsonPosition(value);
		}

		private JsonContainerType Pop()
		{
			JsonPosition currentPosition = _currentPosition;
			if (_stack != null && _stack.Count > 0)
			{
				_currentPosition = _stack[_stack.Count - 1];
				_stack.RemoveAt(_stack.Count - 1);
			}
			else
			{
				_currentPosition = default(JsonPosition);
			}
			return currentPosition.Type;
		}

		private JsonContainerType Peek()
		{
			return _currentPosition.Type;
		}

		public virtual void Close()
		{
			AutoCompleteAll();
		}

		public virtual void WriteStartObject()
		{
			InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);
		}

		public virtual void WriteEndObject()
		{
			InternalWriteEnd(JsonContainerType.Object);
		}

		public virtual void WriteStartArray()
		{
			InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);
		}

		public virtual void WriteEndArray()
		{
			InternalWriteEnd(JsonContainerType.Array);
		}

		public virtual void WriteStartConstructor(string name)
		{
			InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);
		}

		public virtual void WriteEndConstructor()
		{
			InternalWriteEnd(JsonContainerType.Constructor);
		}

		public virtual void WritePropertyName(string name)
		{
			InternalWritePropertyName(name);
		}

		public virtual void WritePropertyName(string name, bool escape)
		{
			WritePropertyName(name);
		}

		public virtual void WriteEnd()
		{
			WriteEnd(Peek());
		}

		public void WriteToken(JsonReader reader)
		{
			WriteToken(reader, true);
		}

		public void WriteToken(JsonReader reader, bool writeChildren)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			WriteToken(reader, writeChildren, true, true);
		}

		public void WriteToken(JsonToken token, object value)
		{
			switch (token)
			{
			case JsonToken.None:
				break;
			case JsonToken.StartObject:
				WriteStartObject();
				break;
			case JsonToken.StartArray:
				WriteStartArray();
				break;
			case JsonToken.StartConstructor:
				ValidationUtils.ArgumentNotNull(value, "value");
				WriteStartConstructor(value.ToString());
				break;
			case JsonToken.PropertyName:
				ValidationUtils.ArgumentNotNull(value, "value");
				WritePropertyName(value.ToString());
				break;
			case JsonToken.Comment:
				WriteComment((value != null) ? value.ToString() : null);
				break;
			case JsonToken.Integer:
				ValidationUtils.ArgumentNotNull(value, "value");
				WriteValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
				break;
			case JsonToken.Float:
				ValidationUtils.ArgumentNotNull(value, "value");
				if (value is decimal)
				{
					WriteValue((decimal)value);
				}
				else if (value is double)
				{
					WriteValue((double)value);
				}
				else if (value is float)
				{
					WriteValue((float)value);
				}
				else
				{
					WriteValue(Convert.ToDouble(value, CultureInfo.InvariantCulture));
				}
				break;
			case JsonToken.String:
				ValidationUtils.ArgumentNotNull(value, "value");
				WriteValue(value.ToString());
				break;
			case JsonToken.Boolean:
				ValidationUtils.ArgumentNotNull(value, "value");
				WriteValue(Convert.ToBoolean(value, CultureInfo.InvariantCulture));
				break;
			case JsonToken.Null:
				WriteNull();
				break;
			case JsonToken.Undefined:
				WriteUndefined();
				break;
			case JsonToken.EndObject:
				WriteEndObject();
				break;
			case JsonToken.EndArray:
				WriteEndArray();
				break;
			case JsonToken.EndConstructor:
				WriteEndConstructor();
				break;
			case JsonToken.Date:
				ValidationUtils.ArgumentNotNull(value, "value");
				if (value is DateTimeOffset)
				{
					WriteValue((DateTimeOffset)value);
				}
				else
				{
					WriteValue(Convert.ToDateTime(value, CultureInfo.InvariantCulture));
				}
				break;
			case JsonToken.Raw:
				WriteRawValue((value != null) ? value.ToString() : null);
				break;
			case JsonToken.Bytes:
				ValidationUtils.ArgumentNotNull(value, "value");
				if (value is Guid)
				{
					WriteValue((Guid)value);
				}
				else
				{
					WriteValue((byte[])value);
				}
				break;
			default:
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException("token", token, "Unexpected token type.");
			}
		}

		internal virtual void WriteToken(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments)
		{
			int num = (reader.TokenType == JsonToken.None) ? (-1) : (JsonTokenUtils.IsStartToken(reader.TokenType) ? reader.Depth : (reader.Depth + 1));
			do
			{
				if (writeDateConstructorAsDate && reader.TokenType == JsonToken.StartConstructor && string.Equals(reader.Value.ToString(), "Date", StringComparison.Ordinal))
				{
					WriteConstructorDate(reader);
				}
				else if (writeComments || reader.TokenType != JsonToken.Comment)
				{
					WriteToken(reader.TokenType, reader.Value);
				}
			}
			while (num - 1 < reader.Depth - (JsonTokenUtils.IsEndToken(reader.TokenType) ? 1 : 0) && writeChildren && reader.Read());
		}

		private void WriteConstructorDate(JsonReader reader)
		{
			if (!reader.Read())
			{
				throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
			}
			if (reader.TokenType != JsonToken.Integer)
			{
				throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected Integer, got " + reader.TokenType, null);
			}
			DateTime value = DateTimeUtils.ConvertJavaScriptTicksToDateTime((long)reader.Value);
			if (!reader.Read())
			{
				throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
			}
			if (reader.TokenType != JsonToken.EndConstructor)
			{
				throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected EndConstructor, got " + reader.TokenType, null);
			}
			WriteValue(value);
		}

		private void WriteEnd(JsonContainerType type)
		{
			switch (type)
			{
			case JsonContainerType.Object:
				WriteEndObject();
				break;
			case JsonContainerType.Array:
				WriteEndArray();
				break;
			case JsonContainerType.Constructor:
				WriteEndConstructor();
				break;
			default:
				throw JsonWriterException.Create(this, "Unexpected type when writing end: " + type, null);
			}
		}

		private void AutoCompleteAll()
		{
			while (Top > 0)
			{
				WriteEnd();
			}
		}

		private JsonToken GetCloseTokenForType(JsonContainerType type)
		{
			switch (type)
			{
			case JsonContainerType.Object:
				return JsonToken.EndObject;
			case JsonContainerType.Array:
				return JsonToken.EndArray;
			case JsonContainerType.Constructor:
				return JsonToken.EndConstructor;
			default:
				throw JsonWriterException.Create(this, "No close token for type: " + type, null);
			}
		}

		private void AutoCompleteClose(JsonContainerType type)
		{
			int num = 0;
			if (_currentPosition.Type == type)
			{
				num = 1;
			}
			else
			{
				int num2 = Top - 2;
				for (int num3 = num2; num3 >= 0; num3--)
				{
					int index = num2 - num3;
					if (_stack[index].Type == type)
					{
						num = num3 + 2;
						break;
					}
				}
			}
			if (num == 0)
			{
				throw JsonWriterException.Create(this, "No token to close.", null);
			}
			for (int i = 0; i < num; i++)
			{
				JsonToken closeTokenForType = GetCloseTokenForType(Pop());
				if (_currentState == State.Property)
				{
					WriteNull();
				}
				if (_formatting == Formatting.Indented && _currentState != State.ObjectStart && _currentState != State.ArrayStart)
				{
					WriteIndent();
				}
				WriteEnd(closeTokenForType);
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
					_currentState = State.Array;
					break;
				case JsonContainerType.None:
					_currentState = State.Start;
					break;
				default:
					throw JsonWriterException.Create(this, "Unknown JsonType: " + jsonContainerType, null);
				}
			}
		}

		protected virtual void WriteEnd(JsonToken token)
		{
		}

		protected virtual void WriteIndent()
		{
		}

		protected virtual void WriteValueDelimiter()
		{
		}

		protected virtual void WriteIndentSpace()
		{
		}

		internal void AutoComplete(JsonToken tokenBeingWritten)
		{
			State state = StateArray[(int)tokenBeingWritten][(int)_currentState];
			if (state == State.Error)
			{
				throw JsonWriterException.Create(this, "Token {0} in state {1} would result in an invalid JSON object.".FormatWith(CultureInfo.InvariantCulture, tokenBeingWritten.ToString(), _currentState.ToString()), null);
			}
			if ((_currentState == State.Object || _currentState == State.Array || _currentState == State.Constructor) && tokenBeingWritten != JsonToken.Comment)
			{
				WriteValueDelimiter();
			}
			if (_formatting == Formatting.Indented)
			{
				if (_currentState == State.Property)
				{
					WriteIndentSpace();
				}
				if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.Constructor || _currentState == State.ConstructorStart || (tokenBeingWritten == JsonToken.PropertyName && _currentState != 0))
				{
					WriteIndent();
				}
			}
			_currentState = state;
		}

		public virtual void WriteNull()
		{
			InternalWriteValue(JsonToken.Null);
		}

		public virtual void WriteUndefined()
		{
			InternalWriteValue(JsonToken.Undefined);
		}

		public virtual void WriteRaw(string json)
		{
			InternalWriteRaw();
		}

		public virtual void WriteRawValue(string json)
		{
			UpdateScopeWithFinishedValue();
			AutoComplete(JsonToken.Undefined);
			WriteRaw(json);
		}

		public virtual void WriteValue(string value)
		{
			InternalWriteValue(JsonToken.String);
		}

		public virtual void WriteValue(int value)
		{
			InternalWriteValue(JsonToken.Integer);
		}

		[CLSCompliant(false)]
		public virtual void WriteValue(uint value)
		{
			InternalWriteValue(JsonToken.Integer);
		}

		public virtual void WriteValue(long value)
		{
			InternalWriteValue(JsonToken.Integer);
		}

		[CLSCompliant(false)]
		public virtual void WriteValue(ulong value)
		{
			InternalWriteValue(JsonToken.Integer);
		}

		public virtual void WriteValue(float value)
		{
			InternalWriteValue(JsonToken.Float);
		}

		public virtual void WriteValue(double value)
		{
			InternalWriteValue(JsonToken.Float);
		}

		public virtual void WriteValue(bool value)
		{
			InternalWriteValue(JsonToken.Boolean);
		}

		public virtual void WriteValue(short value)
		{
			InternalWriteValue(JsonToken.Integer);
		}

		[CLSCompliant(false)]
		public virtual void WriteValue(ushort value)
		{
			InternalWriteValue(JsonToken.Integer);
		}

		public virtual void WriteValue(char value)
		{
			InternalWriteValue(JsonToken.String);
		}

		public virtual void WriteValue(byte value)
		{
			InternalWriteValue(JsonToken.Integer);
		}

		[CLSCompliant(false)]
		public virtual void WriteValue(sbyte value)
		{
			InternalWriteValue(JsonToken.Integer);
		}

		public virtual void WriteValue(decimal value)
		{
			InternalWriteValue(JsonToken.Float);
		}

		public virtual void WriteValue(DateTime value)
		{
			InternalWriteValue(JsonToken.Date);
		}

		public virtual void WriteValue(DateTimeOffset value)
		{
			InternalWriteValue(JsonToken.Date);
		}

		public virtual void WriteValue(Guid value)
		{
			InternalWriteValue(JsonToken.String);
		}

		public virtual void WriteValue(TimeSpan value)
		{
			InternalWriteValue(JsonToken.String);
		}

		public virtual void WriteValue(int? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		[CLSCompliant(false)]
		public virtual void WriteValue(uint? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(long? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		[CLSCompliant(false)]
		public virtual void WriteValue(ulong? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(float? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(double? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(bool? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(short? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		[CLSCompliant(false)]
		public virtual void WriteValue(ushort? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(char? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(byte? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		[CLSCompliant(false)]
		public virtual void WriteValue(sbyte? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(decimal? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(DateTime? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(DateTimeOffset? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(Guid? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(TimeSpan? value)
		{
			if (!value.HasValue)
			{
				WriteNull();
			}
			else
			{
				WriteValue(value.GetValueOrDefault());
			}
		}

		public virtual void WriteValue(byte[] value)
		{
			if (value == null)
			{
				WriteNull();
			}
			else
			{
				InternalWriteValue(JsonToken.Bytes);
			}
		}

		public virtual void WriteValue(Uri value)
		{
			if (value == null)
			{
				WriteNull();
			}
			else
			{
				InternalWriteValue(JsonToken.String);
			}
		}

		public virtual void WriteComment(string text)
		{
			InternalWriteComment();
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

		internal static void WriteValue(JsonWriter writer, PrimitiveTypeCode typeCode, object value)
		{
			switch (typeCode)
			{
			case PrimitiveTypeCode.Char:
				writer.WriteValue((char)value);
				break;
			case PrimitiveTypeCode.CharNullable:
				writer.WriteValue((value == null) ? null : new char?((char)value));
				break;
			case PrimitiveTypeCode.Boolean:
				writer.WriteValue((bool)value);
				break;
			case PrimitiveTypeCode.BooleanNullable:
				writer.WriteValue((value == null) ? null : new bool?((bool)value));
				break;
			case PrimitiveTypeCode.SByte:
				writer.WriteValue((sbyte)value);
				break;
			case PrimitiveTypeCode.SByteNullable:
				writer.WriteValue((value == null) ? null : new sbyte?((sbyte)value));
				break;
			case PrimitiveTypeCode.Int16:
				writer.WriteValue((short)value);
				break;
			case PrimitiveTypeCode.Int16Nullable:
				writer.WriteValue((value == null) ? null : new short?((short)value));
				break;
			case PrimitiveTypeCode.UInt16:
				writer.WriteValue((ushort)value);
				break;
			case PrimitiveTypeCode.UInt16Nullable:
				writer.WriteValue((value == null) ? null : new ushort?((ushort)value));
				break;
			case PrimitiveTypeCode.Int32:
				writer.WriteValue((int)value);
				break;
			case PrimitiveTypeCode.Int32Nullable:
				writer.WriteValue((value == null) ? null : new int?((int)value));
				break;
			case PrimitiveTypeCode.Byte:
				writer.WriteValue((byte)value);
				break;
			case PrimitiveTypeCode.ByteNullable:
				writer.WriteValue((value == null) ? null : new byte?((byte)value));
				break;
			case PrimitiveTypeCode.UInt32:
				writer.WriteValue((uint)value);
				break;
			case PrimitiveTypeCode.UInt32Nullable:
				writer.WriteValue((value == null) ? null : new uint?((uint)value));
				break;
			case PrimitiveTypeCode.Int64:
				writer.WriteValue((long)value);
				break;
			case PrimitiveTypeCode.Int64Nullable:
				writer.WriteValue((value == null) ? null : new long?((long)value));
				break;
			case PrimitiveTypeCode.UInt64:
				writer.WriteValue((ulong)value);
				break;
			case PrimitiveTypeCode.UInt64Nullable:
				writer.WriteValue((value == null) ? null : new ulong?((ulong)value));
				break;
			case PrimitiveTypeCode.Single:
				writer.WriteValue((float)value);
				break;
			case PrimitiveTypeCode.SingleNullable:
				writer.WriteValue((value == null) ? null : new float?((float)value));
				break;
			case PrimitiveTypeCode.Double:
				writer.WriteValue((double)value);
				break;
			case PrimitiveTypeCode.DoubleNullable:
				writer.WriteValue((value == null) ? null : new double?((double)value));
				break;
			case PrimitiveTypeCode.DateTime:
				writer.WriteValue((DateTime)value);
				break;
			case PrimitiveTypeCode.DateTimeNullable:
				writer.WriteValue((value == null) ? null : new DateTime?((DateTime)value));
				break;
			case PrimitiveTypeCode.DateTimeOffset:
				writer.WriteValue((DateTimeOffset)value);
				break;
			case PrimitiveTypeCode.DateTimeOffsetNullable:
				writer.WriteValue((value == null) ? null : new DateTimeOffset?((DateTimeOffset)value));
				break;
			case PrimitiveTypeCode.Decimal:
				writer.WriteValue((decimal)value);
				break;
			case PrimitiveTypeCode.DecimalNullable:
				writer.WriteValue((value == null) ? null : new decimal?((decimal)value));
				break;
			case PrimitiveTypeCode.Guid:
				writer.WriteValue((Guid)value);
				break;
			case PrimitiveTypeCode.GuidNullable:
				writer.WriteValue((value == null) ? null : new Guid?((Guid)value));
				break;
			case PrimitiveTypeCode.TimeSpan:
				writer.WriteValue((TimeSpan)value);
				break;
			case PrimitiveTypeCode.TimeSpanNullable:
				writer.WriteValue((value == null) ? null : new TimeSpan?((TimeSpan)value));
				break;
			case PrimitiveTypeCode.Uri:
				writer.WriteValue((Uri)value);
				break;
			case PrimitiveTypeCode.String:
				writer.WriteValue((string)value);
				break;
			case PrimitiveTypeCode.Bytes:
				writer.WriteValue((byte[])value);
				break;
			case PrimitiveTypeCode.DBNull:
				writer.WriteNull();
				break;
			default:
			{
				if (!(value is IConvertible))
				{
					throw CreateUnsupportedTypeException(writer, value);
				}
				IConvertible obj = (IConvertible)value;
				TypeInformation typeInformation = ConvertUtils.GetTypeInformation(obj);
				PrimitiveTypeCode typeCode2 = (typeInformation.TypeCode == PrimitiveTypeCode.Object) ? PrimitiveTypeCode.String : typeInformation.TypeCode;
				Type conversionType = (typeInformation.TypeCode == PrimitiveTypeCode.Object) ? typeof(string) : typeInformation.Type;
				object value2 = obj.ToType(conversionType, CultureInfo.InvariantCulture);
				WriteValue(writer, typeCode2, value2);
				break;
			}
			}
		}

		private static JsonWriterException CreateUnsupportedTypeException(JsonWriter writer, object value)
		{
			return JsonWriterException.Create(writer, "Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()), null);
		}

		internal void InternalWriteEnd(JsonContainerType container)
		{
			AutoCompleteClose(container);
		}

		internal void InternalWritePropertyName(string name)
		{
			_currentPosition.PropertyName = name;
			AutoComplete(JsonToken.PropertyName);
		}

		internal void InternalWriteRaw()
		{
		}

		internal void InternalWriteStart(JsonToken token, JsonContainerType container)
		{
			UpdateScopeWithFinishedValue();
			AutoComplete(token);
			Push(container);
		}

		internal void InternalWriteValue(JsonToken token)
		{
			UpdateScopeWithFinishedValue();
			AutoComplete(token);
		}

		internal void InternalWriteComment()
		{
			AutoComplete(JsonToken.Comment);
		}
	}
}
