using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using UnityEngine;
namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	internal class JsonFormatterConverter : MonoBehaviour
	{
		private readonly JsonSerializerInternalReader _reader;

		private readonly JsonISerializableContract _contract;

		private readonly JsonProperty _member;

		public JsonFormatterConverter(JsonSerializerInternalReader reader, JsonISerializableContract contract, JsonProperty member)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			ValidationUtils.ArgumentNotNull(contract, "contract");
			_reader = reader;
			_contract = contract;
			_member = member;
		}

		private T GetTokenValue<T>(object value)
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			return (T)System.Convert.ChangeType(((JValue)value).Value, typeof(T), CultureInfo.InvariantCulture);
		}

		public object Convert(object value, Type type)
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			JToken jToken = value as JToken;
			if (jToken == null)
			{
				throw new ArgumentException("Value is not a JToken.", "value");
			}
			return _reader.CreateISerializableItem(jToken, type, _contract, _member);
		}

		public bool ToBoolean(object value)
		{
			return GetTokenValue<bool>(value);
		}

		public short ToInt16(object value)
		{
			return GetTokenValue<short>(value);
		}

		public int ToInt32(object value)
		{
			return GetTokenValue<int>(value);
		}

		public long ToInt64(object value)
		{
			return GetTokenValue<long>(value);
		}

		public string ToString(object value)
		{
			return GetTokenValue<string>(value);
		}

		public uint ToUInt32(object value)
		{
			return GetTokenValue<uint>(value);
		}
	}
}
