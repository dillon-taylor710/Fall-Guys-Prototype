using Newtonsoft.Json.Shims;
using System;

namespace Newtonsoft.Json
{
	[Preserve]
	public abstract class JsonConverter
	{
		public virtual bool CanRead
		{
			get
			{
				return true;
			}
		}

		public virtual bool CanWrite
		{
			get
			{
				return true;
			}
		}

		public abstract void WriteJson(JsonWriter writer, object value, JsonSerializer serializer);

		public abstract object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer);

		public abstract bool CanConvert(Type objectType);
	}
}
