using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Newtonsoft.Json.Converters
{
	public class HashSetConverter : JsonConverter
	{
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			bool flag = serializer.ObjectCreationHandling == ObjectCreationHandling.Replace;
			if (reader.TokenType == JsonToken.Null)
			{
				if (!flag)
				{
					return existingValue;
				}
				return null;
			}
			object obj = (!flag && existingValue != null) ? existingValue : Activator.CreateInstance(objectType);
			Type objectType2 = objectType.GetGenericArguments()[0];
			MethodInfo method = objectType.GetMethod("Add");
			JArray jArray = JArray.Load(reader);
			for (int i = 0; i < jArray.Count; i++)
			{
				object obj2 = serializer.Deserialize(jArray[i].CreateReader(), objectType2);
				method.Invoke(obj, new object[1]
				{
					obj2
				});
			}
			return obj;
		}

		public override bool CanConvert(Type objectType)
		{
			if (objectType.IsGenericType())
			{
				return objectType.GetGenericTypeDefinition() == typeof(HashSet<>);
			}
			return false;
		}
	}
}
