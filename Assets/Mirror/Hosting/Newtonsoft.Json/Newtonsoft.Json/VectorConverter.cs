using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Shims;
using System;
using UnityEngine;

namespace Newtonsoft.Json.Converters
{
	[Preserve]
	public class VectorConverter : JsonConverter
	{
		private static readonly Type V2 = typeof(Vector2);

		private static readonly Type V3 = typeof(Vector3);

		private static readonly Type V4 = typeof(Vector4);

		public bool EnableVector2
		{
			get;
			set;
		}

		public bool EnableVector3
		{
			get;
			set;
		}

		public bool EnableVector4
		{
			get;
			set;
		}

		public VectorConverter()
		{
			EnableVector2 = true;
			EnableVector3 = true;
			EnableVector4 = true;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else
			{
				Type type = value.GetType();
				if (type == V2)
				{
					Vector2 vector = (Vector2)value;
					WriteVector(writer, vector.x, vector.y, null, null);
				}
				else if (type == V3)
				{
					Vector3 vector2 = (Vector3)value;
					WriteVector(writer, vector2.x, vector2.y, vector2.z, null);
				}
				else if (type == V4)
				{
					Vector4 vector3 = (Vector4)value;
					WriteVector(writer, vector3.x, vector3.y, vector3.z, vector3.w);
				}
				else
				{
					writer.WriteNull();
				}
			}
		}

		private static void WriteVector(JsonWriter writer, float x, float y, float? z, float? w)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(x);
			writer.WritePropertyName("y");
			writer.WriteValue(y);
			if (z.HasValue)
			{
				writer.WritePropertyName("z");
				writer.WriteValue(z.Value);
				if (w.HasValue)
				{
					writer.WritePropertyName("w");
					writer.WriteValue(w.Value);
				}
			}
			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (objectType == V2)
			{
				return PopulateVector2(reader);
			}
			if (objectType == V3)
			{
				return PopulateVector3(reader);
			}
			return PopulateVector4(reader);
		}

		public override bool CanConvert(Type objectType)
		{
			if ((!EnableVector2 || objectType != V2) && (!EnableVector3 || objectType != V3))
			{
				if (EnableVector4)
				{
					return objectType == V4;
				}
				return false;
			}
			return true;
		}

		private static Vector2 PopulateVector2(JsonReader reader)
		{
			Vector2 result = default(Vector2);
			if (reader.TokenType != JsonToken.Null)
			{
				JObject jObject = JObject.Load(reader);
				result.x = jObject["x"].Value<float>();
				result.y = jObject["y"].Value<float>();
			}
			return result;
		}

		private static Vector3 PopulateVector3(JsonReader reader)
		{
			Vector3 result = default(Vector3);
			if (reader.TokenType != JsonToken.Null)
			{
				JObject jObject = JObject.Load(reader);
				result.x = jObject["x"].Value<float>();
				result.y = jObject["y"].Value<float>();
				result.z = jObject["z"].Value<float>();
			}
			return result;
		}

		private static Vector4 PopulateVector4(JsonReader reader)
		{
			Vector4 result = default(Vector4);
			if (reader.TokenType != JsonToken.Null)
			{
				JObject jObject = JObject.Load(reader);
				result.x = jObject["x"].Value<float>();
				result.y = jObject["y"].Value<float>();
				result.z = jObject["z"].Value<float>();
				result.w = jObject["w"].Value<float>();
			}
			return result;
		}
	}
}
