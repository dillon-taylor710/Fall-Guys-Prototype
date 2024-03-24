using Newtonsoft.Json.Shims;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Newtonsoft.Json
{
	[Serializable]
	[Preserve]
	public class JsonWriterException : JsonException
	{
		private string Path
		{
			[CompilerGenerated]
			set
			{
				Path = value;
			}
		}

		public JsonWriterException()
		{
		}

		public JsonWriterException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		internal JsonWriterException(string message, Exception innerException, string path)
			: base(message, innerException)
		{
			Path = path;
		}

		internal static JsonWriterException Create(JsonWriter writer, string message, Exception ex)
		{
			return Create(writer.ContainerPath, message, ex);
		}

		internal static JsonWriterException Create(string path, string message, Exception ex)
		{
			message = JsonPosition.FormatMessage(null, path, message);
			return new JsonWriterException(message, ex, path);
		}
	}
}
