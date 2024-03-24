using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Shims;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace Newtonsoft.Json
{
	[Preserve]
	public class JsonSerializerSettings
	{
		internal const ReferenceLoopHandling DefaultReferenceLoopHandling = ReferenceLoopHandling.Error;

		internal const MissingMemberHandling DefaultMissingMemberHandling = MissingMemberHandling.Ignore;

		internal const NullValueHandling DefaultNullValueHandling = NullValueHandling.Include;

		internal const DefaultValueHandling DefaultDefaultValueHandling = DefaultValueHandling.Include;

		internal const ObjectCreationHandling DefaultObjectCreationHandling = ObjectCreationHandling.Auto;

		internal const PreserveReferencesHandling DefaultPreserveReferencesHandling = PreserveReferencesHandling.None;

		internal const ConstructorHandling DefaultConstructorHandling = ConstructorHandling.Default;

		internal const TypeNameHandling DefaultTypeNameHandling = TypeNameHandling.None;

		internal const MetadataPropertyHandling DefaultMetadataPropertyHandling = MetadataPropertyHandling.Default;

		internal const FormatterAssemblyStyle DefaultTypeNameAssemblyFormat = default(FormatterAssemblyStyle);

		internal static readonly StreamingContext DefaultContext;

		internal const Formatting DefaultFormatting = Formatting.None;

		internal const DateFormatHandling DefaultDateFormatHandling = DateFormatHandling.IsoDateFormat;

		internal const DateTimeZoneHandling DefaultDateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;

		internal const DateParseHandling DefaultDateParseHandling = DateParseHandling.DateTime;

		internal const FloatParseHandling DefaultFloatParseHandling = FloatParseHandling.Double;

		internal const FloatFormatHandling DefaultFloatFormatHandling = FloatFormatHandling.String;

		internal const StringEscapeHandling DefaultStringEscapeHandling = StringEscapeHandling.Default;

		internal const FormatterAssemblyStyle DefaultFormatterAssemblyStyle = default(FormatterAssemblyStyle);

		internal static readonly CultureInfo DefaultCulture;

		internal const bool DefaultCheckAdditionalContent = false;

		internal const string DefaultDateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

		internal Formatting? _formatting;

		internal DateFormatHandling? _dateFormatHandling;

		internal DateTimeZoneHandling? _dateTimeZoneHandling;

		internal DateParseHandling? _dateParseHandling;

		internal FloatFormatHandling? _floatFormatHandling;

		internal FloatParseHandling? _floatParseHandling;

		internal StringEscapeHandling? _stringEscapeHandling;

		internal CultureInfo _culture;

		internal bool? _checkAdditionalContent;

		internal int? _maxDepth;

		internal bool _maxDepthSet;

		internal string _dateFormatString;

		internal bool _dateFormatStringSet;

		internal FormatterAssemblyStyle? _typeNameAssemblyFormat;

		internal DefaultValueHandling? _defaultValueHandling;

		internal PreserveReferencesHandling? _preserveReferencesHandling;

		internal NullValueHandling? _nullValueHandling;

		internal ObjectCreationHandling? _objectCreationHandling;

		internal MissingMemberHandling? _missingMemberHandling;

		internal ReferenceLoopHandling? _referenceLoopHandling;

		internal StreamingContext? _context;

		internal ConstructorHandling? _constructorHandling;

		internal TypeNameHandling? _typeNameHandling;

		internal MetadataPropertyHandling? _metadataPropertyHandling;

		public ReferenceLoopHandling ReferenceLoopHandling
		{
			get
			{
				return _referenceLoopHandling ?? ReferenceLoopHandling.Error;
			}
		}

		public MissingMemberHandling MissingMemberHandling
		{
			get
			{
				return _missingMemberHandling ?? MissingMemberHandling.Ignore;
			}
		}

		public ObjectCreationHandling ObjectCreationHandling
		{
			get
			{
				return _objectCreationHandling ?? ObjectCreationHandling.Auto;
			}
		}

		public NullValueHandling NullValueHandling
		{
			get
			{
				return _nullValueHandling ?? NullValueHandling.Include;
			}
		}

		public DefaultValueHandling DefaultValueHandling
		{
			get
			{
				return _defaultValueHandling ?? DefaultValueHandling.Include;
			}
		}

		public IList<JsonConverter> Converters
		{
			get;
			set;
		}

		public PreserveReferencesHandling PreserveReferencesHandling
		{
			get
			{
				return _preserveReferencesHandling ?? PreserveReferencesHandling.None;
			}
		}

		public TypeNameHandling TypeNameHandling
		{
			get
			{
				return _typeNameHandling ?? TypeNameHandling.None;
			}
		}

		public MetadataPropertyHandling MetadataPropertyHandling
		{
			get
			{
				return _metadataPropertyHandling ?? MetadataPropertyHandling.Default;
			}
		}

		public FormatterAssemblyStyle TypeNameAssemblyFormat
		{
			get
			{
				return _typeNameAssemblyFormat ?? FormatterAssemblyStyle.Simple;
			}
		}

		public ConstructorHandling ConstructorHandling
		{
			get
			{
				return _constructorHandling ?? ConstructorHandling.Default;
			}
		}

		public IContractResolver ContractResolver
		{
			get;
		}

		public IEqualityComparer EqualityComparer
		{
			get;
		}

		public Func<IReferenceResolver> ReferenceResolverProvider
		{
			get;
		}

		public ITraceWriter TraceWriter
		{
			get;
		}

		public SerializationBinder Binder
		{
			get;
		}

		public EventHandler<ErrorEventArgs> Error
		{
			get;
		}

		public StreamingContext Context
		{
			get
			{
				return _context ?? DefaultContext;
			}
		}

		static JsonSerializerSettings()
		{
			DefaultContext = default(StreamingContext);
			DefaultCulture = CultureInfo.InvariantCulture;
		}

		public JsonSerializerSettings()
		{
			Converters = new List<JsonConverter>
			{
				new VectorConverter()
			};
			Converters.Add(new HashSetConverter());
		}
	}
}
