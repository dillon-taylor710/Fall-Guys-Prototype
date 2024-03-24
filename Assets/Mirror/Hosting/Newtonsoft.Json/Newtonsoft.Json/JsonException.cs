using Newtonsoft.Json.Shims;
using System;
using System.Runtime.Serialization;

namespace Newtonsoft.Json
{
	[Serializable]
	[Preserve]
	public class JsonException : Exception
	{
		public JsonException()
		{
		}

		public JsonException(string message)
			: base(message)
		{
		}

		public JsonException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public JsonException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
