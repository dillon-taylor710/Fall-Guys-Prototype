using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace Newtonsoft.Json
{
	[Preserve]
	public class JsonSerializer
	{
		internal TypeNameHandling _typeNameHandling;

		internal FormatterAssemblyStyle _typeNameAssemblyFormat;

		internal PreserveReferencesHandling _preserveReferencesHandling;

		internal ReferenceLoopHandling _referenceLoopHandling;

		internal MissingMemberHandling _missingMemberHandling;

		internal ObjectCreationHandling _objectCreationHandling;

		internal NullValueHandling _nullValueHandling;

		internal DefaultValueHandling _defaultValueHandling;

		internal ConstructorHandling _constructorHandling;

		internal MetadataPropertyHandling _metadataPropertyHandling;

		internal JsonConverterCollection _converters;

		internal IContractResolver _contractResolver;

		internal ITraceWriter _traceWriter;

		internal IEqualityComparer _equalityComparer;

		internal SerializationBinder _binder;

		internal StreamingContext _context;

		private IReferenceResolver _referenceResolver;

		private Formatting? _formatting;

		private DateFormatHandling? _dateFormatHandling;

		private DateTimeZoneHandling? _dateTimeZoneHandling;

		private DateParseHandling? _dateParseHandling;

		private FloatFormatHandling? _floatFormatHandling;

		private FloatParseHandling? _floatParseHandling;

		private StringEscapeHandling? _stringEscapeHandling;

		private CultureInfo _culture;

		private int? _maxDepth;

		private bool _maxDepthSet;

		private bool? _checkAdditionalContent;

		private string _dateFormatString;

		private bool _dateFormatStringSet;

		public virtual IReferenceResolver ReferenceResolver
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "Reference resolver cannot be null.");
				}
				_referenceResolver = value;
			}
		}

		public virtual SerializationBinder Binder
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "Serialization binder cannot be null.");
				}
				_binder = value;
			}
		}

		public virtual ITraceWriter TraceWriter
		{
			get
			{
				return _traceWriter;
			}
			set
			{
				_traceWriter = value;
			}
		}

		public virtual IEqualityComparer EqualityComparer
		{
			set
			{
				_equalityComparer = value;
			}
		}

		public virtual TypeNameHandling TypeNameHandling
		{
			set
			{
				if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_typeNameHandling = value;
			}
		}

		public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
		{
			set
			{
				if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_typeNameAssemblyFormat = value;
			}
		}

		public virtual PreserveReferencesHandling PreserveReferencesHandling
		{
			set
			{
				if (value < PreserveReferencesHandling.None || value > PreserveReferencesHandling.All)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_preserveReferencesHandling = value;
			}
		}

		public virtual ReferenceLoopHandling ReferenceLoopHandling
		{
			set
			{
				if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_referenceLoopHandling = value;
			}
		}

		public virtual MissingMemberHandling MissingMemberHandling
		{
			set
			{
				if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_missingMemberHandling = value;
			}
		}

		public virtual NullValueHandling NullValueHandling
		{
			set
			{
				if (value < NullValueHandling.Include || value > NullValueHandling.Ignore)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_nullValueHandling = value;
			}
		}

		public virtual DefaultValueHandling DefaultValueHandling
		{
			set
			{
				if (value < DefaultValueHandling.Include || value > DefaultValueHandling.IgnoreAndPopulate)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_defaultValueHandling = value;
			}
		}

		public virtual ObjectCreationHandling ObjectCreationHandling
		{
			get
			{
				return _objectCreationHandling;
			}
			set
			{
				if (value < ObjectCreationHandling.Auto || value > ObjectCreationHandling.Replace)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_objectCreationHandling = value;
			}
		}

		public virtual ConstructorHandling ConstructorHandling
		{
			set
			{
				if (value < ConstructorHandling.Default || value > ConstructorHandling.AllowNonPublicDefaultConstructor)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_constructorHandling = value;
			}
		}

		public virtual MetadataPropertyHandling MetadataPropertyHandling
		{
			get
			{
				return _metadataPropertyHandling;
			}
			set
			{
				if (value < MetadataPropertyHandling.Default || value > MetadataPropertyHandling.Ignore)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_metadataPropertyHandling = value;
			}
		}

		public virtual JsonConverterCollection Converters
		{
			get
			{
				if (_converters == null)
				{
					_converters = new JsonConverterCollection();
				}
				return _converters;
			}
		}

		public virtual IContractResolver ContractResolver
		{
			get
			{
				return _contractResolver;
			}
			set
			{
				_contractResolver = (value ?? DefaultContractResolver.Instance);
			}
		}

		public virtual StreamingContext Context
		{
			get
			{
				return _context;
			}
			set
			{
				_context = value;
			}
		}

		public virtual Formatting Formatting
		{
			get
			{
				return _formatting ?? Formatting.None;
			}
			set
			{
				_formatting = value;
			}
		}

		public virtual bool CheckAdditionalContent
		{
			get
			{
				return _checkAdditionalContent ?? false;
			}
			set
			{
				_checkAdditionalContent = value;
			}
		}

		public virtual event EventHandler<ErrorEventArgs> Error;

		public JsonSerializer()
		{
			_referenceLoopHandling = ReferenceLoopHandling.Error;
			_missingMemberHandling = MissingMemberHandling.Ignore;
			_nullValueHandling = NullValueHandling.Include;
			_defaultValueHandling = DefaultValueHandling.Include;
			_objectCreationHandling = ObjectCreationHandling.Auto;
			_preserveReferencesHandling = PreserveReferencesHandling.None;
			_constructorHandling = ConstructorHandling.Default;
			_typeNameHandling = TypeNameHandling.None;
			_metadataPropertyHandling = MetadataPropertyHandling.Default;
			_context = JsonSerializerSettings.DefaultContext;
			_binder = DefaultSerializationBinder.Instance;
			_culture = JsonSerializerSettings.DefaultCulture;
			_contractResolver = DefaultContractResolver.Instance;
		}

		internal bool IsCheckAdditionalContentSet()
		{
			return _checkAdditionalContent.HasValue;
		}

		public static JsonSerializer Create()
		{
			return new JsonSerializer();
		}

		public static JsonSerializer Create(JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = Create();
			if (settings != null)
			{
				ApplySerializerSettings(jsonSerializer, settings);
			}
			return jsonSerializer;
		}

		public static JsonSerializer CreateDefault()
		{
			Func<JsonSerializerSettings> defaultSettings = JsonConvert.DefaultSettings;
			return Create((defaultSettings != null) ? defaultSettings() : null);
		}

		public static JsonSerializer CreateDefault(JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = CreateDefault();
			if (settings != null)
			{
				ApplySerializerSettings(jsonSerializer, settings);
			}
			return jsonSerializer;
		}

		private static void ApplySerializerSettings(JsonSerializer serializer, JsonSerializerSettings settings)
		{
			if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
			{
				for (int i = 0; i < settings.Converters.Count; i++)
				{
					serializer.Converters.Insert(i, settings.Converters[i]);
				}
			}
			if (settings._typeNameHandling.HasValue)
			{
				serializer.TypeNameHandling = settings.TypeNameHandling;
			}
			if (settings._metadataPropertyHandling.HasValue)
			{
				serializer.MetadataPropertyHandling = settings.MetadataPropertyHandling;
			}
			if (settings._typeNameAssemblyFormat.HasValue)
			{
				serializer.TypeNameAssemblyFormat = settings.TypeNameAssemblyFormat;
			}
			if (settings._preserveReferencesHandling.HasValue)
			{
				serializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
			}
			if (settings._referenceLoopHandling.HasValue)
			{
				serializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
			}
			if (settings._missingMemberHandling.HasValue)
			{
				serializer.MissingMemberHandling = settings.MissingMemberHandling;
			}
			if (settings._objectCreationHandling.HasValue)
			{
				serializer.ObjectCreationHandling = settings.ObjectCreationHandling;
			}
			if (settings._nullValueHandling.HasValue)
			{
				serializer.NullValueHandling = settings.NullValueHandling;
			}
			if (settings._defaultValueHandling.HasValue)
			{
				serializer.DefaultValueHandling = settings.DefaultValueHandling;
			}
			if (settings._constructorHandling.HasValue)
			{
				serializer.ConstructorHandling = settings.ConstructorHandling;
			}
			if (settings._context.HasValue)
			{
				serializer.Context = settings.Context;
			}
			if (settings._checkAdditionalContent.HasValue)
			{
				serializer._checkAdditionalContent = settings._checkAdditionalContent;
			}
			if (settings.Error != null)
			{
				serializer.Error += settings.Error;
			}
			if (settings.ContractResolver != null)
			{
				serializer.ContractResolver = settings.ContractResolver;
			}
			if (settings.ReferenceResolverProvider != null)
			{
				serializer.ReferenceResolver = settings.ReferenceResolverProvider();
			}
			if (settings.TraceWriter != null)
			{
				serializer.TraceWriter = settings.TraceWriter;
			}
			if (settings.EqualityComparer != null)
			{
				serializer.EqualityComparer = settings.EqualityComparer;
			}
			if (settings.Binder != null)
			{
				serializer.Binder = settings.Binder;
			}
			if (settings._formatting.HasValue)
			{
				serializer._formatting = settings._formatting;
			}
			if (settings._dateFormatHandling.HasValue)
			{
				serializer._dateFormatHandling = settings._dateFormatHandling;
			}
			if (settings._dateTimeZoneHandling.HasValue)
			{
				serializer._dateTimeZoneHandling = settings._dateTimeZoneHandling;
			}
			if (settings._dateParseHandling.HasValue)
			{
				serializer._dateParseHandling = settings._dateParseHandling;
			}
			if (settings._dateFormatStringSet)
			{
				serializer._dateFormatString = settings._dateFormatString;
				serializer._dateFormatStringSet = settings._dateFormatStringSet;
			}
			if (settings._floatFormatHandling.HasValue)
			{
				serializer._floatFormatHandling = settings._floatFormatHandling;
			}
			if (settings._floatParseHandling.HasValue)
			{
				serializer._floatParseHandling = settings._floatParseHandling;
			}
			if (settings._stringEscapeHandling.HasValue)
			{
				serializer._stringEscapeHandling = settings._stringEscapeHandling;
			}
			if (settings._culture != null)
			{
				serializer._culture = settings._culture;
			}
			if (settings._maxDepthSet)
			{
				serializer._maxDepth = settings._maxDepth;
				serializer._maxDepthSet = settings._maxDepthSet;
			}
		}

		public T Deserialize<T>(JsonReader reader)
		{
			return (T)Deserialize(reader, typeof(T));
		}

		public object Deserialize(JsonReader reader, Type objectType)
		{
			return DeserializeInternal(reader, objectType);
		}

		internal virtual object DeserializeInternal(JsonReader reader, Type objectType)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			CultureInfo previousCulture;
			DateTimeZoneHandling? previousDateTimeZoneHandling;
			DateParseHandling? previousDateParseHandling;
			FloatParseHandling? previousFloatParseHandling;
			int? previousMaxDepth;
			string previousDateFormatString;
			SetupReader(reader, out previousCulture, out previousDateTimeZoneHandling, out previousDateParseHandling, out previousFloatParseHandling, out previousMaxDepth, out previousDateFormatString);
			TraceJsonReader traceJsonReader = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose) ? new TraceJsonReader(reader) : null;
			object result = new JsonSerializerInternalReader(this).Deserialize(traceJsonReader ?? reader, objectType, CheckAdditionalContent);
			if (traceJsonReader != null)
			{
				TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
			}
			ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
			return result;
		}

		private void SetupReader(JsonReader reader, out CultureInfo previousCulture, out DateTimeZoneHandling? previousDateTimeZoneHandling, out DateParseHandling? previousDateParseHandling, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string previousDateFormatString)
		{
			if (_culture != null && !_culture.Equals(reader.Culture))
			{
				previousCulture = reader.Culture;
				reader.Culture = _culture;
			}
			else
			{
				previousCulture = null;
			}
			if (_dateTimeZoneHandling.HasValue && reader.DateTimeZoneHandling != _dateTimeZoneHandling)
			{
				previousDateTimeZoneHandling = reader.DateTimeZoneHandling;
				reader.DateTimeZoneHandling = _dateTimeZoneHandling.GetValueOrDefault();
			}
			else
			{
				previousDateTimeZoneHandling = null;
			}
			if (_dateParseHandling.HasValue && reader.DateParseHandling != _dateParseHandling)
			{
				previousDateParseHandling = reader.DateParseHandling;
				reader.DateParseHandling = _dateParseHandling.GetValueOrDefault();
			}
			else
			{
				previousDateParseHandling = null;
			}
			if (_floatParseHandling.HasValue && reader.FloatParseHandling != _floatParseHandling)
			{
				previousFloatParseHandling = reader.FloatParseHandling;
				reader.FloatParseHandling = _floatParseHandling.GetValueOrDefault();
			}
			else
			{
				previousFloatParseHandling = null;
			}
			if (_maxDepthSet && reader.MaxDepth != _maxDepth)
			{
				previousMaxDepth = reader.MaxDepth;
				reader.MaxDepth = _maxDepth;
			}
			else
			{
				previousMaxDepth = null;
			}
			if (_dateFormatStringSet && reader.DateFormatString != _dateFormatString)
			{
				previousDateFormatString = reader.DateFormatString;
				reader.DateFormatString = _dateFormatString;
			}
			else
			{
				previousDateFormatString = null;
			}
			JsonTextReader jsonTextReader = reader as JsonTextReader;
			if (jsonTextReader != null)
			{
				DefaultContractResolver defaultContractResolver = _contractResolver as DefaultContractResolver;
				if (defaultContractResolver != null)
				{
					jsonTextReader.NameTable = defaultContractResolver.GetState().NameTable;
				}
			}
		}

		private void ResetReader(JsonReader reader, CultureInfo previousCulture, DateTimeZoneHandling? previousDateTimeZoneHandling, DateParseHandling? previousDateParseHandling, FloatParseHandling? previousFloatParseHandling, int? previousMaxDepth, string previousDateFormatString)
		{
			if (previousCulture != null)
			{
				reader.Culture = previousCulture;
			}
			if (previousDateTimeZoneHandling.HasValue)
			{
				reader.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
			}
			if (previousDateParseHandling.HasValue)
			{
				reader.DateParseHandling = previousDateParseHandling.GetValueOrDefault();
			}
			if (previousFloatParseHandling.HasValue)
			{
				reader.FloatParseHandling = previousFloatParseHandling.GetValueOrDefault();
			}
			if (_maxDepthSet)
			{
				reader.MaxDepth = previousMaxDepth;
			}
			if (_dateFormatStringSet)
			{
				reader.DateFormatString = previousDateFormatString;
			}
			JsonTextReader jsonTextReader = reader as JsonTextReader;
			if (jsonTextReader != null)
			{
				jsonTextReader.NameTable = null;
			}
		}

		public void Serialize(JsonWriter jsonWriter, object value, Type objectType)
		{
			SerializeInternal(jsonWriter, value, objectType);
		}

		public void Serialize(JsonWriter jsonWriter, object value)
		{
			SerializeInternal(jsonWriter, value, null);
		}

		internal virtual void SerializeInternal(JsonWriter jsonWriter, object value, Type objectType)
		{
			ValidationUtils.ArgumentNotNull(jsonWriter, "jsonWriter");
			Formatting? formatting = null;
			if (_formatting.HasValue && jsonWriter.Formatting != _formatting)
			{
				formatting = jsonWriter.Formatting;
				jsonWriter.Formatting = _formatting.GetValueOrDefault();
			}
			DateFormatHandling? dateFormatHandling = null;
			if (_dateFormatHandling.HasValue && jsonWriter.DateFormatHandling != _dateFormatHandling)
			{
				dateFormatHandling = jsonWriter.DateFormatHandling;
				jsonWriter.DateFormatHandling = _dateFormatHandling.GetValueOrDefault();
			}
			DateTimeZoneHandling? dateTimeZoneHandling = null;
			if (_dateTimeZoneHandling.HasValue && jsonWriter.DateTimeZoneHandling != _dateTimeZoneHandling)
			{
				dateTimeZoneHandling = jsonWriter.DateTimeZoneHandling;
				jsonWriter.DateTimeZoneHandling = _dateTimeZoneHandling.GetValueOrDefault();
			}
			FloatFormatHandling? floatFormatHandling = null;
			if (_floatFormatHandling.HasValue && jsonWriter.FloatFormatHandling != _floatFormatHandling)
			{
				floatFormatHandling = jsonWriter.FloatFormatHandling;
				jsonWriter.FloatFormatHandling = _floatFormatHandling.GetValueOrDefault();
			}
			StringEscapeHandling? stringEscapeHandling = null;
			if (_stringEscapeHandling.HasValue && jsonWriter.StringEscapeHandling != _stringEscapeHandling)
			{
				stringEscapeHandling = jsonWriter.StringEscapeHandling;
				jsonWriter.StringEscapeHandling = _stringEscapeHandling.GetValueOrDefault();
			}
			CultureInfo cultureInfo = null;
			if (_culture != null && !_culture.Equals(jsonWriter.Culture))
			{
				cultureInfo = jsonWriter.Culture;
				jsonWriter.Culture = _culture;
			}
			string dateFormatString = null;
			if (_dateFormatStringSet && jsonWriter.DateFormatString != _dateFormatString)
			{
				dateFormatString = jsonWriter.DateFormatString;
				jsonWriter.DateFormatString = _dateFormatString;
			}
			TraceJsonWriter traceJsonWriter = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose) ? new TraceJsonWriter(jsonWriter) : null;
			new JsonSerializerInternalWriter(this).Serialize(traceJsonWriter ?? jsonWriter, value, objectType);
			if (traceJsonWriter != null)
			{
				TraceWriter.Trace(TraceLevel.Verbose, traceJsonWriter.GetSerializedJsonMessage(), null);
			}
			if (formatting.HasValue)
			{
				jsonWriter.Formatting = formatting.GetValueOrDefault();
			}
			if (dateFormatHandling.HasValue)
			{
				jsonWriter.DateFormatHandling = dateFormatHandling.GetValueOrDefault();
			}
			if (dateTimeZoneHandling.HasValue)
			{
				jsonWriter.DateTimeZoneHandling = dateTimeZoneHandling.GetValueOrDefault();
			}
			if (floatFormatHandling.HasValue)
			{
				jsonWriter.FloatFormatHandling = floatFormatHandling.GetValueOrDefault();
			}
			if (stringEscapeHandling.HasValue)
			{
				jsonWriter.StringEscapeHandling = stringEscapeHandling.GetValueOrDefault();
			}
			if (_dateFormatStringSet)
			{
				jsonWriter.DateFormatString = dateFormatString;
			}
			if (cultureInfo != null)
			{
				jsonWriter.Culture = cultureInfo;
			}
		}

		internal IReferenceResolver GetReferenceResolver()
		{
			if (_referenceResolver == null)
			{
				_referenceResolver = new DefaultReferenceResolver();
			}
			return _referenceResolver;
		}

		internal JsonConverter GetMatchingConverter(Type type)
		{
			return GetMatchingConverter(_converters, type);
		}

		internal static JsonConverter GetMatchingConverter(IList<JsonConverter> converters, Type objectType)
		{
			if (converters != null)
			{
				for (int i = 0; i < converters.Count; i++)
				{
					JsonConverter jsonConverter = converters[i];
					if (jsonConverter.CanConvert(objectType))
					{
						return jsonConverter;
					}
				}
			}
			return null;
		}

		internal void OnError(ErrorEventArgs e)
		{
			EventHandler<ErrorEventArgs> error = this.Error;
			if (error != null)
			{
				error(this, e);
			}
		}
	}
}
