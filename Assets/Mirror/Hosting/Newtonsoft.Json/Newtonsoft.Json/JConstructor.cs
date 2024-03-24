using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
	[Preserve]
	public class JConstructor : JContainer
	{
		private string _name;

		private readonly List<JToken> _values = new List<JToken>();

		protected override IList<JToken> ChildrenTokens
		{
			get
			{
				return _values;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public override JTokenType Type
		{
			get
			{
				return JTokenType.Constructor;
			}
		}

		public override JToken this[object key]
		{
			get
			{
				ValidationUtils.ArgumentNotNull(key, "key");
				if (!(key is int))
				{
					throw new ArgumentException("Accessed JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
				}
				return GetItem((int)key);
			}
			set
			{
				ValidationUtils.ArgumentNotNull(key, "key");
				if (!(key is int))
				{
					throw new ArgumentException("Set JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
				}
				SetItem((int)key, value);
			}
		}

		public JConstructor(JConstructor other)
			: base(other)
		{
			_name = other.Name;
		}

		public JConstructor(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException("Constructor name cannot be empty.", "name");
			}
			_name = name;
		}

		internal override int IndexOfItem(JToken item)
		{
			return _values.IndexOfReference(item);
		}

		internal override bool DeepEquals(JToken node)
		{
			JConstructor jConstructor = node as JConstructor;
			if (jConstructor != null && _name == jConstructor.Name)
			{
				return ContentsEqual(jConstructor);
			}
			return false;
		}

		internal override JToken CloneToken()
		{
			return new JConstructor(this);
		}

		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			writer.WriteStartConstructor(_name);
			foreach (JToken item in Children())
			{
				item.WriteTo(writer, converters);
			}
			writer.WriteEndConstructor();
		}

		internal override int GetDeepHashCode()
		{
			return _name.GetHashCode() ^ ContentsHashCode();
		}

		public static JConstructor Load(JsonReader reader, JsonLoadSettings settings)
		{
			if (reader.TokenType == JsonToken.None && !reader.Read())
			{
				throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
			}
			reader.MoveToContent();
			if (reader.TokenType != JsonToken.StartConstructor)
			{
				throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			JConstructor jConstructor = new JConstructor((string)reader.Value);
			jConstructor.SetLineInfo(reader as IJsonLineInfo, settings);
			jConstructor.ReadTokenFrom(reader, settings);
			return jConstructor;
		}
	}
}
