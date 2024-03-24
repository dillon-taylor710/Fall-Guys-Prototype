using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Newtonsoft.Json.Linq
{
	[Preserve]
	public abstract class JToken : IEnumerable<JToken>, IJEnumerable<JToken>, IEnumerable, IJsonLineInfo, ICloneable
	{
		private class LineInfoAnnotation
		{
			internal readonly int LineNumber;

			internal readonly int LinePosition;

			public LineInfoAnnotation(int lineNumber, int linePosition)
			{
				LineNumber = lineNumber;
				LinePosition = linePosition;
			}
		}

		private static JTokenEqualityComparer _equalityComparer;

		private JContainer _parent;

		private JToken _previous;

		private JToken _next;

		private object _annotations;

		private static readonly JTokenType[] BooleanTypes = new JTokenType[6]
		{
			JTokenType.Integer,
			JTokenType.Float,
			JTokenType.String,
			JTokenType.Comment,
			JTokenType.Raw,
			JTokenType.Boolean
		};

		private static readonly JTokenType[] NumberTypes = new JTokenType[6]
		{
			JTokenType.Integer,
			JTokenType.Float,
			JTokenType.String,
			JTokenType.Comment,
			JTokenType.Raw,
			JTokenType.Boolean
		};

		private static readonly JTokenType[] StringTypes = new JTokenType[11]
		{
			JTokenType.Date,
			JTokenType.Integer,
			JTokenType.Float,
			JTokenType.String,
			JTokenType.Comment,
			JTokenType.Raw,
			JTokenType.Boolean,
			JTokenType.Bytes,
			JTokenType.Guid,
			JTokenType.TimeSpan,
			JTokenType.Uri
		};

		private static readonly JTokenType[] GuidTypes = new JTokenType[5]
		{
			JTokenType.String,
			JTokenType.Comment,
			JTokenType.Raw,
			JTokenType.Guid,
			JTokenType.Bytes
		};

		private static readonly JTokenType[] TimeSpanTypes = new JTokenType[4]
		{
			JTokenType.String,
			JTokenType.Comment,
			JTokenType.Raw,
			JTokenType.TimeSpan
		};

		private static readonly JTokenType[] UriTypes = new JTokenType[4]
		{
			JTokenType.String,
			JTokenType.Comment,
			JTokenType.Raw,
			JTokenType.Uri
		};

		private static readonly JTokenType[] CharTypes = new JTokenType[5]
		{
			JTokenType.Integer,
			JTokenType.Float,
			JTokenType.String,
			JTokenType.Comment,
			JTokenType.Raw
		};

		private static readonly JTokenType[] DateTimeTypes = new JTokenType[4]
		{
			JTokenType.Date,
			JTokenType.String,
			JTokenType.Comment,
			JTokenType.Raw
		};

		private static readonly JTokenType[] BytesTypes = new JTokenType[5]
		{
			JTokenType.Bytes,
			JTokenType.String,
			JTokenType.Comment,
			JTokenType.Raw,
			JTokenType.Integer
		};

		public JContainer Parent
		{
			[DebuggerStepThrough]
			get
			{
				return _parent;
			}
			internal set
			{
				_parent = value;
			}
		}

		public JToken Root
		{
			get
			{
				JContainer parent = Parent;
				if (parent == null)
				{
					return this;
				}
				while (parent.Parent != null)
				{
					parent = parent.Parent;
				}
				return parent;
			}
		}

		public abstract JTokenType Type
		{
			get;
		}

		public abstract bool HasValues
		{
			get;
		}

		public JToken Next
		{
			get
			{
				return _next;
			}
			internal set
			{
				_next = value;
			}
		}

		public JToken Previous
		{
			get
			{
				return _previous;
			}
			internal set
			{
				_previous = value;
			}
		}

		public string Path
		{
			get
			{
				if (Parent == null)
				{
					return string.Empty;
				}
				List<JsonPosition> list = new List<JsonPosition>();
				JToken jToken = null;
				for (JToken jToken2 = this; jToken2 != null; jToken2 = jToken2.Parent)
				{
					JsonPosition item;
					switch (jToken2.Type)
					{
					case JTokenType.Property:
					{
						JProperty jProperty = (JProperty)jToken2;
						List<JsonPosition> list3 = list;
						item = new JsonPosition(JsonContainerType.Object)
						{
							PropertyName = jProperty.Name
						};
						list3.Add(item);
						break;
					}
					case JTokenType.Array:
					case JTokenType.Constructor:
						if (jToken != null)
						{
							int position = ((IList<JToken>)jToken2).IndexOf(jToken);
							List<JsonPosition> list2 = list;
							item = new JsonPosition(JsonContainerType.Array)
							{
								Position = position
							};
							list2.Add(item);
						}
						break;
					}
					jToken = jToken2;
				}
				list.Reverse();
				return JsonPosition.BuildPath(list, null);
			}
		}

		public virtual JToken this[object key]
		{
			get
			{
				throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
			set
			{
				throw new InvalidOperationException("Cannot set child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
		}

		public virtual JToken First
		{
			get
			{
				throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
		}

		public virtual JToken Last
		{
			get
			{
				throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
		}

		int IJsonLineInfo.LineNumber
		{
			get
			{
				LineInfoAnnotation lineInfoAnnotation = Annotation<LineInfoAnnotation>();
				if (lineInfoAnnotation != null)
				{
					return lineInfoAnnotation.LineNumber;
				}
				return 0;
			}
		}

		int IJsonLineInfo.LinePosition
		{
			get
			{
				LineInfoAnnotation lineInfoAnnotation = Annotation<LineInfoAnnotation>();
				if (lineInfoAnnotation != null)
				{
					return lineInfoAnnotation.LinePosition;
				}
				return 0;
			}
		}

		internal JToken()
		{
		}

		internal abstract JToken CloneToken();

		internal abstract bool DeepEquals(JToken node);

		public static bool DeepEquals(JToken t1, JToken t2)
		{
			if (t1 != t2)
			{
				if (t1 != null && t2 != null)
				{
					return t1.DeepEquals(t2);
				}
				return false;
			}
			return true;
		}

		public virtual T Value<T>(object key)
		{
			JToken jToken = this[key];
			if (jToken != null)
			{
				return jToken.Convert<JToken, T>();
			}
			return default(T);
		}

		public virtual JEnumerable<JToken> Children()
		{
			return JEnumerable<JToken>.Empty;
		}

		public JEnumerable<T> Children<T>() where T : JToken
		{
			return new JEnumerable<T>(Children().OfType<T>());
		}

		public void Remove()
		{
			if (_parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			_parent.RemoveItem(this);
		}

		public void Replace(JToken value)
		{
			if (_parent == null)
			{
				throw new InvalidOperationException("The parent is missing.");
			}
			_parent.ReplaceItem(this, value);
		}

		public abstract void WriteTo(JsonWriter writer, params JsonConverter[] converters);

		public override string ToString()
		{
			return ToString(Formatting.Indented);
		}

		public string ToString(Formatting formatting, params JsonConverter[] converters)
		{
			using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
			{
				JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter);
				jsonTextWriter.Formatting = formatting;
				WriteTo(jsonTextWriter, converters);
				return stringWriter.ToString();
			}
		}

		private static JValue EnsureValue(JToken value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value is JProperty)
			{
				value = ((JProperty)value).Value;
			}
			return value as JValue;
		}

		private static string GetType(JToken token)
		{
			ValidationUtils.ArgumentNotNull(token, "token");
			if (token is JProperty)
			{
				token = ((JProperty)token).Value;
			}
			return token.Type.ToString();
		}

		private static bool ValidateToken(JToken o, JTokenType[] validTypes, bool nullable)
		{
			if (Array.IndexOf(validTypes, o.Type) == -1)
			{
				if (nullable)
				{
					if (o.Type != JTokenType.Null)
					{
						return o.Type == JTokenType.Undefined;
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public static explicit operator bool(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, BooleanTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToBoolean(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator DateTimeOffset(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, DateTimeTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value is DateTimeOffset)
			{
				return (DateTimeOffset)jValue.Value;
			}
			if (jValue.Value is string)
			{
				return DateTimeOffset.Parse((string)jValue.Value, CultureInfo.InvariantCulture);
			}
			return new DateTimeOffset(Convert.ToDateTime(jValue.Value, CultureInfo.InvariantCulture));
		}

		public static explicit operator bool?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, BooleanTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToBoolean(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator long(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToInt64(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator DateTime?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, DateTimeTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value is DateTimeOffset)
			{
				return ((DateTimeOffset)jValue.Value).DateTime;
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToDateTime(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator DateTimeOffset?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, DateTimeTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			if (jValue.Value is DateTimeOffset)
			{
				return (DateTimeOffset?)jValue.Value;
			}
			if (jValue.Value is string)
			{
				return DateTimeOffset.Parse((string)jValue.Value, CultureInfo.InvariantCulture);
			}
			return new DateTimeOffset(Convert.ToDateTime(jValue.Value, CultureInfo.InvariantCulture));
		}

		public static explicit operator decimal?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToDecimal(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator double?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToDouble(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator char?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, CharTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Char.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToChar(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator int(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToInt32(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator short(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToInt16(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator ushort(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToUInt16(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator char(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, CharTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Char.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToChar(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator byte(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Byte.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToByte(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator sbyte(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to SByte.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToSByte(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator int?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToInt32(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator short?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToInt16(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator ushort?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToUInt16(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator byte?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Byte.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToByte(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator sbyte?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to SByte.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToSByte(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator DateTime(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, DateTimeTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value is DateTimeOffset)
			{
				return ((DateTimeOffset)jValue.Value).DateTime;
			}
			return Convert.ToDateTime(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator long?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToInt64(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator float?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToSingle(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator decimal(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToDecimal(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator uint?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToUInt32(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator ulong?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return Convert.ToUInt64(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator double(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToDouble(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator float(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToSingle(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator string(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, StringTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to String.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			if (jValue.Value is byte[])
			{
				return Convert.ToBase64String((byte[])jValue.Value);
			}
			return Convert.ToString(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator uint(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToUInt32(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator ulong(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, NumberTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			return Convert.ToUInt64(jValue.Value, CultureInfo.InvariantCulture);
		}

		public static explicit operator Guid(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, GuidTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to Guid.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value is byte[])
			{
				return new Guid((byte[])jValue.Value);
			}
			if (!(jValue.Value is Guid))
			{
				return new Guid(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
			}
			return (Guid)jValue.Value;
		}

		public static explicit operator Guid?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, GuidTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Guid.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			if (jValue.Value is byte[])
			{
				return new Guid((byte[])jValue.Value);
			}
			return (jValue.Value is Guid) ? ((Guid)jValue.Value) : new Guid(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
		}

		public static explicit operator TimeSpan(JToken value)
		{
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, TimeSpanTypes, false))
			{
				throw new ArgumentException("Can not convert {0} to TimeSpan.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (!(jValue.Value is TimeSpan))
			{
				return ConvertUtils.ParseTimeSpan(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
			}
			return (TimeSpan)jValue.Value;
		}

		public static explicit operator TimeSpan?(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, TimeSpanTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to TimeSpan.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			return (jValue.Value is TimeSpan) ? ((TimeSpan)jValue.Value) : ConvertUtils.ParseTimeSpan(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
		}

		public static explicit operator Uri(JToken value)
		{
			if (value == null)
			{
				return null;
			}
			JValue jValue = EnsureValue(value);
			if (jValue == null || !ValidateToken(jValue, UriTypes, true))
			{
				throw new ArgumentException("Can not convert {0} to Uri.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
			}
			if (jValue.Value == null)
			{
				return null;
			}
			if (!(jValue.Value is Uri))
			{
				return new Uri(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
			}
			return (Uri)jValue.Value;
		}

		public static implicit operator JToken(bool value)
		{
			return new JValue(value);
		}

		public static implicit operator JToken(long value)
		{
			return new JValue(value);
		}

		public static implicit operator JToken(int value)
		{
			return new JValue(value);
		}

		public static implicit operator JToken(DateTime value)
		{
			return new JValue(value);
		}

		public static implicit operator JToken(double value)
		{
			return new JValue(value);
		}

		public static implicit operator JToken(float value)
		{
			return new JValue(value);
		}

		public static implicit operator JToken(string value)
		{
			return new JValue(value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<JToken>)this).GetEnumerator();
		}

		IEnumerator<JToken> IEnumerable<JToken>.GetEnumerator()
		{
			return Children().GetEnumerator();
		}

		internal abstract int GetDeepHashCode();

		public JsonReader CreateReader()
		{
			return new JTokenReader(this);
		}

		internal static JToken FromObjectInternal(object o, JsonSerializer jsonSerializer)
		{
			ValidationUtils.ArgumentNotNull(o, "o");
			ValidationUtils.ArgumentNotNull(jsonSerializer, "jsonSerializer");
			using (JTokenWriter jTokenWriter = new JTokenWriter())
			{
				jsonSerializer.Serialize(jTokenWriter, o);
				return jTokenWriter.Token;
			}
		}

		public static JToken FromObject(object o)
		{
			return FromObjectInternal(o, JsonSerializer.CreateDefault());
		}

		public object ToObject(Type objectType)
		{
			if (JsonConvert.DefaultSettings == null)
			{
				bool isEnum;
				PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(objectType, out isEnum);
				if (isEnum)
				{
					if (Type == JTokenType.String)
					{
						try
						{
							return ToObject(objectType, JsonSerializer.CreateDefault());
						}
						catch (Exception innerException)
						{
							Type type = objectType.IsEnum() ? objectType : Nullable.GetUnderlyingType(objectType);
							throw new ArgumentException("Could not convert '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, (string)this, type.Name), innerException);
						}
					}
					if (Type == JTokenType.Integer)
					{
						return Enum.ToObject(objectType.IsEnum() ? objectType : Nullable.GetUnderlyingType(objectType), ((JValue)this).Value);
					}
				}
				switch (typeCode)
				{
				case PrimitiveTypeCode.BooleanNullable:
					return (bool?)this;
				case PrimitiveTypeCode.Boolean:
					return (bool)this;
				case PrimitiveTypeCode.CharNullable:
					return (char?)this;
				case PrimitiveTypeCode.Char:
					return (char)this;
				case PrimitiveTypeCode.SByte:
					return (sbyte?)this;
				case PrimitiveTypeCode.SByteNullable:
					return (sbyte)this;
				case PrimitiveTypeCode.ByteNullable:
					return (byte?)this;
				case PrimitiveTypeCode.Byte:
					return (byte)this;
				case PrimitiveTypeCode.Int16Nullable:
					return (short?)this;
				case PrimitiveTypeCode.Int16:
					return (short)this;
				case PrimitiveTypeCode.UInt16Nullable:
					return (ushort?)this;
				case PrimitiveTypeCode.UInt16:
					return (ushort)this;
				case PrimitiveTypeCode.Int32Nullable:
					return (int?)this;
				case PrimitiveTypeCode.Int32:
					return (int)this;
				case PrimitiveTypeCode.UInt32Nullable:
					return (uint?)this;
				case PrimitiveTypeCode.UInt32:
					return (uint)this;
				case PrimitiveTypeCode.Int64Nullable:
					return (long?)this;
				case PrimitiveTypeCode.Int64:
					return (long)this;
				case PrimitiveTypeCode.UInt64Nullable:
					return (ulong?)this;
				case PrimitiveTypeCode.UInt64:
					return (ulong)this;
				case PrimitiveTypeCode.SingleNullable:
					return (float?)this;
				case PrimitiveTypeCode.Single:
					return (float)this;
				case PrimitiveTypeCode.DoubleNullable:
					return (double?)this;
				case PrimitiveTypeCode.Double:
					return (double)this;
				case PrimitiveTypeCode.DecimalNullable:
					return (decimal?)this;
				case PrimitiveTypeCode.Decimal:
					return (decimal)this;
				case PrimitiveTypeCode.DateTimeNullable:
					return (DateTime?)this;
				case PrimitiveTypeCode.DateTime:
					return (DateTime)this;
				case PrimitiveTypeCode.DateTimeOffsetNullable:
					return (DateTimeOffset?)this;
				case PrimitiveTypeCode.DateTimeOffset:
					return (DateTimeOffset)this;
				case PrimitiveTypeCode.String:
					return (string)this;
				case PrimitiveTypeCode.GuidNullable:
					return (Guid?)this;
				case PrimitiveTypeCode.Guid:
					return (Guid)this;
				case PrimitiveTypeCode.Uri:
					return (Uri)this;
				case PrimitiveTypeCode.TimeSpanNullable:
					return (TimeSpan?)this;
				case PrimitiveTypeCode.TimeSpan:
					return (TimeSpan)this;
				}
			}
			return ToObject(objectType, JsonSerializer.CreateDefault());
		}

		public object ToObject(Type objectType, JsonSerializer jsonSerializer)
		{
			ValidationUtils.ArgumentNotNull(jsonSerializer, "jsonSerializer");
			using (JTokenReader reader = new JTokenReader(this))
			{
				return jsonSerializer.Deserialize(reader, objectType);
			}
		}

		public static JToken ReadFrom(JsonReader reader)
		{
			return ReadFrom(reader, null);
		}

		public static JToken ReadFrom(JsonReader reader, JsonLoadSettings settings)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			if (reader.TokenType != 0 || ((settings != null && settings.CommentHandling == CommentHandling.Ignore) ? reader.ReadAndMoveToContent() : reader.Read()))
			{
				IJsonLineInfo lineInfo = reader as IJsonLineInfo;
				switch (reader.TokenType)
				{
				case JsonToken.StartObject:
					return JObject.Load(reader, settings);
				case JsonToken.StartArray:
					return JArray.Load(reader, settings);
				case JsonToken.StartConstructor:
					return JConstructor.Load(reader, settings);
				case JsonToken.PropertyName:
					return JProperty.Load(reader, settings);
				case JsonToken.Integer:
				case JsonToken.Float:
				case JsonToken.String:
				case JsonToken.Boolean:
				case JsonToken.Date:
				case JsonToken.Bytes:
				{
					JValue jValue4 = new JValue(reader.Value);
					jValue4.SetLineInfo(lineInfo, settings);
					return jValue4;
				}
				case JsonToken.Comment:
				{
					JValue jValue3 = JValue.CreateComment(reader.Value.ToString());
					jValue3.SetLineInfo(lineInfo, settings);
					return jValue3;
				}
				case JsonToken.Null:
				{
					JValue jValue2 = JValue.CreateNull();
					jValue2.SetLineInfo(lineInfo, settings);
					return jValue2;
				}
				case JsonToken.Undefined:
				{
					JValue jValue = JValue.CreateUndefined();
					jValue.SetLineInfo(lineInfo, settings);
					return jValue;
				}
				default:
					throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader. Unexpected token: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
				}
			}
			throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader.");
		}

		internal void SetLineInfo(IJsonLineInfo lineInfo, JsonLoadSettings settings)
		{
			if ((settings == null || settings.LineInfoHandling != LineInfoHandling.Load) && lineInfo != null && lineInfo.HasLineInfo())
			{
				SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
			}
		}

		internal void SetLineInfo(int lineNumber, int linePosition)
		{
			AddAnnotation(new LineInfoAnnotation(lineNumber, linePosition));
		}

		bool IJsonLineInfo.HasLineInfo()
		{
			return Annotation<LineInfoAnnotation>() != null;
		}

		object ICloneable.Clone()
		{
			return DeepClone();
		}

		public JToken DeepClone()
		{
			return CloneToken();
		}

		public void AddAnnotation(object annotation)
		{
			if (annotation == null)
			{
				throw new ArgumentNullException("annotation");
			}
			if (_annotations == null)
			{
				_annotations = ((!(annotation is object[])) ? annotation : new object[1]
				{
					annotation
				});
			}
			else
			{
				object[] array = _annotations as object[];
				if (array == null)
				{
					_annotations = new object[2]
					{
						_annotations,
						annotation
					};
				}
				else
				{
					int i;
					for (i = 0; i < array.Length && array[i] != null; i++)
					{
					}
					if (i == array.Length)
					{
						Array.Resize(ref array, i * 2);
						_annotations = array;
					}
					array[i] = annotation;
				}
			}
		}

		public T Annotation<T>() where T : class
		{
			if (_annotations != null)
			{
				object[] array = _annotations as object[];
				if (array == null)
				{
					return _annotations as T;
				}
				foreach (object obj in array)
				{
					if (obj == null)
					{
						break;
					}
					T val = obj as T;
					if (val != null)
					{
						return val;
					}
				}
			}
			return null;
		}
	}
}
