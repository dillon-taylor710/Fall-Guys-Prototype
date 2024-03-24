using Newtonsoft.Json.Shims;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Newtonsoft.Json
{
	[Serializable]
	[Preserve]
	public class JsonReaderException : JsonException
	{
		private int LineNumber
		{
			[CompilerGenerated]
			set
			{
				LineNumber = value;
			}
		}

		private int LinePosition
		{
			[CompilerGenerated]
			set
			{
				LinePosition = value;
			}
		}

		private string Path
		{
			[CompilerGenerated]
			set
			{
				Path = value;
			}
		}

		public JsonReaderException()
		{
		}

		public JsonReaderException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		internal JsonReaderException(string message, Exception innerException, string path, int lineNumber, int linePosition)
			: base(message, innerException)
		{
			Path = path;
			LineNumber = lineNumber;
			LinePosition = linePosition;
		}

		internal static JsonReaderException Create(JsonReader reader, string message)
		{
			return Create(reader, message, null);
		}

		internal static JsonReaderException Create(JsonReader reader, string message, Exception ex)
		{
			return Create(reader as IJsonLineInfo, reader.Path, message, ex);
		}

		internal static JsonReaderException Create(IJsonLineInfo lineInfo, string path, string message, Exception ex)
		{
			message = JsonPosition.FormatMessage(lineInfo, path, message);
			int lineNumber;
			int linePosition;
			if (lineInfo != null && lineInfo.HasLineInfo())
			{
				lineNumber = lineInfo.LineNumber;
				linePosition = lineInfo.LinePosition;
			}
			else
			{
				lineNumber = 0;
				linePosition = 0;
			}
			return new JsonReaderException(message, ex, path, lineNumber, linePosition);
		}
	}
}
