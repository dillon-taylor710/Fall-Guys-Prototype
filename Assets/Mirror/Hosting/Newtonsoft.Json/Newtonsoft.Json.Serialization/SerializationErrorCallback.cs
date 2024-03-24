using Newtonsoft.Json.Shims;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public delegate void SerializationErrorCallback(object o, StreamingContext context, ErrorContext errorContext);
}
