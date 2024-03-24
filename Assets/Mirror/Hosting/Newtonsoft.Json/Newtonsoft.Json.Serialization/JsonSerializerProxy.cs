using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	internal class JsonSerializerProxy : JsonSerializer
	{
		private readonly JsonSerializerInternalReader _serializerReader;

		private readonly JsonSerializerInternalWriter _serializerWriter;

		private readonly JsonSerializer _serializer;

		public override IReferenceResolver ReferenceResolver
		{
			set
			{
				_serializer.ReferenceResolver = value;
			}
		}

		public override ITraceWriter TraceWriter
		{
			get
			{
				return _serializer.TraceWriter;
			}
			set
			{
				_serializer.TraceWriter = value;
			}
		}

		public override IEqualityComparer EqualityComparer
		{
			set
			{
				_serializer.EqualityComparer = value;
			}
		}

		public override JsonConverterCollection Converters
		{
			get
			{
				return _serializer.Converters;
			}
		}

		public override DefaultValueHandling DefaultValueHandling
		{
			set
			{
				_serializer.DefaultValueHandling = value;
			}
		}

		public override IContractResolver ContractResolver
		{
			get
			{
				return _serializer.ContractResolver;
			}
			set
			{
				_serializer.ContractResolver = value;
			}
		}

		public override MissingMemberHandling MissingMemberHandling
		{
			set
			{
				_serializer.MissingMemberHandling = value;
			}
		}

		public override NullValueHandling NullValueHandling
		{
			set
			{
				_serializer.NullValueHandling = value;
			}
		}

		public override ObjectCreationHandling ObjectCreationHandling
		{
			get
			{
				return _serializer.ObjectCreationHandling;
			}
			set
			{
				_serializer.ObjectCreationHandling = value;
			}
		}

		public override ReferenceLoopHandling ReferenceLoopHandling
		{
			set
			{
				_serializer.ReferenceLoopHandling = value;
			}
		}

		public override PreserveReferencesHandling PreserveReferencesHandling
		{
			set
			{
				_serializer.PreserveReferencesHandling = value;
			}
		}

		public override TypeNameHandling TypeNameHandling
		{
			set
			{
				_serializer.TypeNameHandling = value;
			}
		}

		public override MetadataPropertyHandling MetadataPropertyHandling
		{
			get
			{
				return _serializer.MetadataPropertyHandling;
			}
			set
			{
				_serializer.MetadataPropertyHandling = value;
			}
		}

		public override FormatterAssemblyStyle TypeNameAssemblyFormat
		{
			set
			{
				_serializer.TypeNameAssemblyFormat = value;
			}
		}

		public override ConstructorHandling ConstructorHandling
		{
			set
			{
				_serializer.ConstructorHandling = value;
			}
		}

		public override SerializationBinder Binder
		{
			set
			{
				_serializer.Binder = value;
			}
		}

		public override StreamingContext Context
		{
			get
			{
				return _serializer.Context;
			}
			set
			{
				_serializer.Context = value;
			}
		}

		public override Formatting Formatting
		{
			get
			{
				return _serializer.Formatting;
			}
			set
			{
				_serializer.Formatting = value;
			}
		}

		public override bool CheckAdditionalContent
		{
			get
			{
				return _serializer.CheckAdditionalContent;
			}
			set
			{
				_serializer.CheckAdditionalContent = value;
			}
		}

		public override event EventHandler<ErrorEventArgs> Error
		{
			add
			{
				_serializer.Error += value;
			}
			remove
			{
				_serializer.Error -= value;
			}
		}

		public JsonSerializerProxy(JsonSerializerInternalReader serializerReader)
		{
			ValidationUtils.ArgumentNotNull(serializerReader, "serializerReader");
			_serializerReader = serializerReader;
			_serializer = serializerReader.Serializer;
		}

		public JsonSerializerProxy(JsonSerializerInternalWriter serializerWriter)
		{
			ValidationUtils.ArgumentNotNull(serializerWriter, "serializerWriter");
			_serializerWriter = serializerWriter;
			_serializer = serializerWriter.Serializer;
		}

		internal JsonSerializerInternalBase GetInternalSerializer()
		{
			if (_serializerReader != null)
			{
				return _serializerReader;
			}
			return _serializerWriter;
		}

		internal override object DeserializeInternal(JsonReader reader, Type objectType)
		{
			if (_serializerReader != null)
			{
				return _serializerReader.Deserialize(reader, objectType, false);
			}
			return _serializer.Deserialize(reader, objectType);
		}

		internal override void SerializeInternal(JsonWriter jsonWriter, object value, Type rootType)
		{
			if (_serializerWriter != null)
			{
				_serializerWriter.Serialize(jsonWriter, value, rootType);
			}
			else
			{
				_serializer.Serialize(jsonWriter, value);
			}
		}
	}
}
