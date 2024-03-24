using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
	[Preserve]
	public class JValue : JToken, IEquatable<JValue>, IComparable<JValue>, IComparable, IConvertible, IFormattable
	{
		private JTokenType _valueType;

		private object _value;

		public override bool HasValues
		{
			get
			{
				return false;
			}
		}

		public override JTokenType Type
		{
			get
			{
				return _valueType;
			}
		}

		public new object Value
		{
			get
			{
				return _value;
			}
		}

		internal JValue(object value, JTokenType type)
		{
			_value = value;
			_valueType = type;
		}

		public JValue(JValue other)
			: this(other.Value, other.Type)
		{
		}

		public JValue(long value)
			: this(value, JTokenType.Integer)
		{
		}

		public JValue(double value)
			: this(value, JTokenType.Float)
		{
		}

		public JValue(float value)
			: this(value, JTokenType.Float)
		{
		}

		public JValue(DateTime value)
			: this(value, JTokenType.Date)
		{
		}

		public JValue(bool value)
			: this(value, JTokenType.Boolean)
		{
		}

		public JValue(string value)
			: this(value, JTokenType.String)
		{
		}

		public JValue(object value)
			: this(value, GetValueType(null, value))
		{
		}

		internal override bool DeepEquals(JToken node)
		{
			JValue jValue = node as JValue;
			if (jValue == null)
			{
				return false;
			}
			if (jValue == this)
			{
				return true;
			}
			return ValuesEquals(this, jValue);
		}

		internal static int Compare(JTokenType valueType, object objA, object objB)
		{
			if (objA == null && objB == null)
			{
				return 0;
			}
			if (objA != null && objB == null)
			{
				return 1;
			}
			if (objA != null || objB == null)
			{
				switch (valueType)
				{
				case JTokenType.Integer:
					if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
					{
						return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
					}
					if (objA is float || objB is float || objA is double || objB is double)
					{
						return CompareFloat(objA, objB);
					}
					return Convert.ToInt64(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
				case JTokenType.Float:
					return CompareFloat(objA, objB);
				case JTokenType.Comment:
				case JTokenType.String:
				case JTokenType.Raw:
				{
					string strA = Convert.ToString(objA, CultureInfo.InvariantCulture);
					string strB = Convert.ToString(objB, CultureInfo.InvariantCulture);
					return string.CompareOrdinal(strA, strB);
				}
				case JTokenType.Boolean:
				{
					bool flag = Convert.ToBoolean(objA, CultureInfo.InvariantCulture);
					bool value4 = Convert.ToBoolean(objB, CultureInfo.InvariantCulture);
					return flag.CompareTo(value4);
				}
				case JTokenType.Date:
				{
					if (objA is DateTime)
					{
						DateTime dateTime = (DateTime)objA;
						DateTime value2 = (!(objB is DateTimeOffset)) ? Convert.ToDateTime(objB, CultureInfo.InvariantCulture) : ((DateTimeOffset)objB).DateTime;
						return dateTime.CompareTo(value2);
					}
					DateTimeOffset dateTimeOffset = (DateTimeOffset)objA;
					DateTimeOffset other = (objB is DateTimeOffset) ? ((DateTimeOffset)objB) : new DateTimeOffset(Convert.ToDateTime(objB, CultureInfo.InvariantCulture));
					return dateTimeOffset.CompareTo(other);
				}
				case JTokenType.Bytes:
				{
					if (!(objB is byte[]))
					{
						throw new ArgumentException("Object must be of type byte[].");
					}
					byte[] array = objA as byte[];
					byte[] array2 = objB as byte[];
					if (array == null)
					{
						return -1;
					}
					if (array2 == null)
					{
						return 1;
					}
					return MiscellaneousUtils.ByteArrayCompare(array, array2);
				}
				case JTokenType.Guid:
				{
					if (!(objB is Guid))
					{
						throw new ArgumentException("Object must be of type Guid.");
					}
					Guid guid = (Guid)objA;
					Guid value3 = (Guid)objB;
					return guid.CompareTo(value3);
				}
				case JTokenType.Uri:
				{
					if (!(objB is Uri))
					{
						throw new ArgumentException("Object must be of type Uri.");
					}
					Uri uri = (Uri)objA;
					Uri uri2 = (Uri)objB;
					return Comparer<string>.Default.Compare(uri.ToString(), uri2.ToString());
				}
				case JTokenType.TimeSpan:
				{
					if (!(objB is TimeSpan))
					{
						throw new ArgumentException("Object must be of type TimeSpan.");
					}
					TimeSpan timeSpan = (TimeSpan)objA;
					TimeSpan value = (TimeSpan)objB;
					return timeSpan.CompareTo(value);
				}
				default:
					throw MiscellaneousUtils.CreateArgumentOutOfRangeException("valueType", valueType, "Unexpected value type: {0}".FormatWith(CultureInfo.InvariantCulture, valueType));
				}
			}
			return -1;
		}

		private static int CompareFloat(object objA, object objB)
		{
			double d = Convert.ToDouble(objA, CultureInfo.InvariantCulture);
			double num = Convert.ToDouble(objB, CultureInfo.InvariantCulture);
			if (MathUtils.ApproxEquals(d, num))
			{
				return 0;
			}
			return d.CompareTo(num);
		}

		internal override JToken CloneToken()
		{
			return new JValue(this);
		}

		public static JValue CreateComment(string value)
		{
			return new JValue(value, JTokenType.Comment);
		}

		public static JValue CreateNull()
		{
			return new JValue(null, JTokenType.Null);
		}

		public static JValue CreateUndefined()
		{
			return new JValue(null, JTokenType.Undefined);
		}

		private static JTokenType GetValueType(JTokenType? current, object value)
		{
			if (value == null)
			{
				return JTokenType.Null;
			}
			if (value == DBNull.Value)
			{
				return JTokenType.Null;
			}
			if (value is string)
			{
				return GetStringValueType(current);
			}
			if (value is long || value is int || value is short || value is sbyte || value is ulong || value is uint || value is ushort || value is byte)
			{
				return JTokenType.Integer;
			}
			if (value is Enum)
			{
				return JTokenType.Integer;
			}
			if (value is double || value is float || value is decimal)
			{
				return JTokenType.Float;
			}
			if (value is DateTime)
			{
				return JTokenType.Date;
			}
			if (value is DateTimeOffset)
			{
				return JTokenType.Date;
			}
			if (value is byte[])
			{
				return JTokenType.Bytes;
			}
			if (value is bool)
			{
				return JTokenType.Boolean;
			}
			if (value is Guid)
			{
				return JTokenType.Guid;
			}
			if (value is Uri)
			{
				return JTokenType.Uri;
			}
			if (value is TimeSpan)
			{
				return JTokenType.TimeSpan;
			}
			throw new ArgumentException("Could not determine JSON object type for type {0}.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
		}

		private static JTokenType GetStringValueType(JTokenType? current)
		{
			if (!current.HasValue)
			{
				return JTokenType.String;
			}
			JTokenType valueOrDefault = current.GetValueOrDefault();
			if (valueOrDefault == JTokenType.Comment || valueOrDefault == JTokenType.String || valueOrDefault == JTokenType.Raw)
			{
				return current.GetValueOrDefault();
			}
			return JTokenType.String;
		}

		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			if (converters != null && converters.Length != 0 && _value != null)
			{
				JsonConverter matchingConverter = JsonSerializer.GetMatchingConverter(converters, _value.GetType());
				if (matchingConverter != null && matchingConverter.CanWrite)
				{
					matchingConverter.WriteJson(writer, _value, JsonSerializer.CreateDefault());
					return;
				}
			}
			switch (_valueType)
			{
			case JTokenType.Comment:
				writer.WriteComment((_value != null) ? _value.ToString() : null);
				break;
			case JTokenType.Raw:
				writer.WriteRawValue((_value != null) ? _value.ToString() : null);
				break;
			case JTokenType.Null:
				writer.WriteNull();
				break;
			case JTokenType.Undefined:
				writer.WriteUndefined();
				break;
			case JTokenType.Integer:
				if (_value is int)
				{
					writer.WriteValue((int)_value);
				}
				else if (_value is long)
				{
					writer.WriteValue((long)_value);
				}
				else if (_value is ulong)
				{
					writer.WriteValue((ulong)_value);
				}
				else
				{
					writer.WriteValue(Convert.ToInt64(_value, CultureInfo.InvariantCulture));
				}
				break;
			case JTokenType.Float:
				if (_value is decimal)
				{
					writer.WriteValue((decimal)_value);
				}
				else if (_value is double)
				{
					writer.WriteValue((double)_value);
				}
				else if (_value is float)
				{
					writer.WriteValue((float)_value);
				}
				else
				{
					writer.WriteValue(Convert.ToDouble(_value, CultureInfo.InvariantCulture));
				}
				break;
			case JTokenType.String:
				writer.WriteValue((_value != null) ? _value.ToString() : null);
				break;
			case JTokenType.Boolean:
				writer.WriteValue(Convert.ToBoolean(_value, CultureInfo.InvariantCulture));
				break;
			case JTokenType.Date:
				if (_value is DateTimeOffset)
				{
					writer.WriteValue((DateTimeOffset)_value);
				}
				else
				{
					writer.WriteValue(Convert.ToDateTime(_value, CultureInfo.InvariantCulture));
				}
				break;
			case JTokenType.Bytes:
				writer.WriteValue((byte[])_value);
				break;
			case JTokenType.Guid:
				writer.WriteValue((_value != null) ? ((Guid?)_value) : null);
				break;
			case JTokenType.TimeSpan:
				writer.WriteValue((_value != null) ? ((TimeSpan?)_value) : null);
				break;
			case JTokenType.Uri:
				writer.WriteValue((Uri)_value);
				break;
			default:
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException("TokenType", _valueType, "Unexpected token type.");
			}
		}

		internal override int GetDeepHashCode()
		{
			int num = (_value != null) ? _value.GetHashCode() : 0;
			int valueType = (int)_valueType;
			return valueType.GetHashCode() ^ num;
		}

		private static bool ValuesEquals(JValue v1, JValue v2)
		{
			if (v1 != v2)
			{
				if (v1._valueType == v2._valueType)
				{
					return Compare(v1._valueType, v1._value, v2._value) == 0;
				}
				return false;
			}
			return true;
		}

		public bool Equals(JValue other)
		{
			if (other == null)
			{
				return false;
			}
			return ValuesEquals(this, other);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			JValue jValue = obj as JValue;
			if (jValue != null)
			{
				return Equals(jValue);
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			if (_value == null)
			{
				return 0;
			}
			return _value.GetHashCode();
		}

		public override string ToString()
		{
			if (_value == null)
			{
				return string.Empty;
			}
			return _value.ToString();
		}

		public string ToString(IFormatProvider formatProvider)
		{
			return ToString(null, formatProvider);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (_value == null)
			{
				return string.Empty;
			}
			IFormattable formattable = _value as IFormattable;
			if (formattable != null)
			{
				return formattable.ToString(format, formatProvider);
			}
			return _value.ToString();
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			object objB = (obj is JValue) ? ((JValue)obj).Value : obj;
			return Compare(_valueType, _value, objB);
		}

		public int CompareTo(JValue obj)
		{
			if (obj == null)
			{
				return 1;
			}
			return Compare(_valueType, _value, obj._value);
		}

		TypeCode IConvertible.GetTypeCode()
		{
			if (_value == null)
			{
				return TypeCode.Empty;
			}
			IConvertible convertible = _value as IConvertible;
			if (convertible == null)
			{
				return TypeCode.Object;
			}
			return convertible.GetTypeCode();
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return (bool)(JToken)this;
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return (char)(JToken)this;
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return (sbyte)(JToken)this;
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return (byte)(JToken)this;
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return (short)(JToken)this;
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return (ushort)(JToken)this;
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return (int)(JToken)this;
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return (uint)(JToken)this;
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return (long)(JToken)this;
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return (ulong)(JToken)this;
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return (float)(JToken)this;
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return (double)(JToken)this;
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return (decimal)(JToken)this;
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return (DateTime)(JToken)this;
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			return ToObject(conversionType);
		}
	}
}
