using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
	[Preserve]
	public class JTokenWriter : JsonWriter
	{
		private JContainer _token;

		private JContainer _parent;

		private JValue _value;

		private JToken _current;

		public JToken Token
		{
			get
			{
				if (_token != null)
				{
					return _token;
				}
				return _value;
			}
		}

		public override void Close()
		{
			base.Close();
		}

		public override void WriteStartObject()
		{
			base.WriteStartObject();
			AddParent(new JObject());
		}

		private void AddParent(JContainer container)
		{
			if (_parent == null)
			{
				_token = container;
			}
			else
			{
				_parent.AddAndSkipParentCheck(container);
			}
			_parent = container;
			_current = container;
		}

		private void RemoveParent()
		{
			_current = _parent;
			_parent = _parent.Parent;
			if (_parent != null && _parent.Type == JTokenType.Property)
			{
				_parent = _parent.Parent;
			}
		}

		public override void WriteStartArray()
		{
			base.WriteStartArray();
			AddParent(new JArray());
		}

		public override void WriteStartConstructor(string name)
		{
			base.WriteStartConstructor(name);
			AddParent(new JConstructor(name));
		}

		protected override void WriteEnd(JsonToken token)
		{
			RemoveParent();
		}

		public override void WritePropertyName(string name)
		{
			JObject jObject = _parent as JObject;
			if (jObject != null)
			{
				jObject.Remove(name);
			}
			AddParent(new JProperty(name));
			base.WritePropertyName(name);
		}

		private void AddValue(object value, JsonToken token)
		{
			AddValue(new JValue(value), token);
		}

		internal void AddValue(JValue value, JsonToken token)
		{
			if (_parent != null)
			{
				_parent.Add(value);
				_current = _parent.Last;
				if (_parent.Type == JTokenType.Property)
				{
					_parent = _parent.Parent;
				}
			}
			else
			{
				_value = (value ?? JValue.CreateNull());
				_current = _value;
			}
		}

		public override void WriteNull()
		{
			base.WriteNull();
			AddValue(null, JsonToken.Null);
		}

		public override void WriteUndefined()
		{
			base.WriteUndefined();
			AddValue(null, JsonToken.Undefined);
		}

		public override void WriteRaw(string json)
		{
			base.WriteRaw(json);
			AddValue(new JRaw(json), JsonToken.Raw);
		}

		public override void WriteComment(string text)
		{
			base.WriteComment(text);
			AddValue(JValue.CreateComment(text), JsonToken.Comment);
		}

		public override void WriteValue(string value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.String);
		}

		public override void WriteValue(int value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		[CLSCompliant(false)]
		public override void WriteValue(uint value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		public override void WriteValue(long value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		[CLSCompliant(false)]
		public override void WriteValue(ulong value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		public override void WriteValue(float value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Float);
		}

		public override void WriteValue(double value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Float);
		}

		public override void WriteValue(bool value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Boolean);
		}

		public override void WriteValue(short value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		[CLSCompliant(false)]
		public override void WriteValue(ushort value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		public override void WriteValue(char value)
		{
			base.WriteValue(value);
			string text = null;
			text = value.ToString(CultureInfo.InvariantCulture);
			AddValue(text, JsonToken.String);
		}

		public override void WriteValue(byte value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		[CLSCompliant(false)]
		public override void WriteValue(sbyte value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Integer);
		}

		public override void WriteValue(decimal value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Float);
		}

		public override void WriteValue(DateTime value)
		{
			base.WriteValue(value);
			value = DateTimeUtils.EnsureDateTime(value, base.DateTimeZoneHandling);
			AddValue(value, JsonToken.Date);
		}

		public override void WriteValue(DateTimeOffset value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Date);
		}

		public override void WriteValue(byte[] value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.Bytes);
		}

		public override void WriteValue(TimeSpan value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.String);
		}

		public override void WriteValue(Guid value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.String);
		}

		public override void WriteValue(Uri value)
		{
			base.WriteValue(value);
			AddValue(value, JsonToken.String);
		}

		internal override void WriteToken(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments)
		{
			JTokenReader jTokenReader = reader as JTokenReader;
			if ((jTokenReader != null && writeChildren) & writeDateConstructorAsDate & writeComments)
			{
				if (jTokenReader.TokenType != 0 || jTokenReader.Read())
				{
					JToken jToken = jTokenReader.CurrentToken.CloneToken();
					if (_parent != null)
					{
						_parent.Add(jToken);
						_current = _parent.Last;
						if (_parent.Type == JTokenType.Property)
						{
							_parent = _parent.Parent;
							InternalWriteValue(JsonToken.Null);
						}
					}
					else
					{
						_current = jToken;
						if (_token == null && _value == null)
						{
							_token = (jToken as JContainer);
							_value = (jToken as JValue);
						}
					}
					jTokenReader.Skip();
				}
			}
			else
			{
				base.WriteToken(reader, writeChildren, writeDateConstructorAsDate, writeComments);
			}
		}
	}
}