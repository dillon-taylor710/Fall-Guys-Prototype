using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public class JsonObjectContract : JsonContainerContract
	{
		internal bool ExtensionDataIsJToken;

		private bool? _hasRequiredOrDefaultValueProperties;

		private ConstructorInfo _parametrizedConstructor;

		private ConstructorInfo _overrideConstructor;

		private ObjectConstructor<object> _overrideCreator;

		private ObjectConstructor<object> _parameterizedCreator;

		private JsonPropertyCollection _creatorParameters;

		private Type _extensionDataValueType;

		public MemberSerialization MemberSerialization
		{
			get;
			set;
		}

		public Required? ItemRequired
		{
			get;
			set;
		}

		public JsonPropertyCollection Properties
		{
			get;
			private set;
		}

		public JsonPropertyCollection CreatorParameters
		{
			get
			{
				if (_creatorParameters == null)
				{
					_creatorParameters = new JsonPropertyCollection(base.UnderlyingType);
				}
				return _creatorParameters;
			}
		}

		[Obsolete("OverrideConstructor is obsolete. Use OverrideCreator instead.")]
		public ConstructorInfo OverrideConstructor
		{
			set
			{
				_overrideConstructor = value;
				_overrideCreator = ((value != null) ? JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(value) : null);
			}
		}

		[Obsolete("ParametrizedConstructor is obsolete. Use OverrideCreator instead.")]
		public ConstructorInfo ParametrizedConstructor
		{
			set
			{
				_parametrizedConstructor = value;
				_parameterizedCreator = ((value != null) ? JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(value) : null);
			}
		}

		public ObjectConstructor<object> OverrideCreator
		{
			get
			{
				return _overrideCreator;
			}
		}

		internal ObjectConstructor<object> ParameterizedCreator
		{
			get
			{
				return _parameterizedCreator;
			}
		}

		public ExtensionDataSetter ExtensionDataSetter
		{
			get;
			set;
		}

		public ExtensionDataGetter ExtensionDataGetter
		{
			get;
			set;
		}

		public Type ExtensionDataValueType
		{
			set
			{
				_extensionDataValueType = value;
				ExtensionDataIsJToken = (value != null && typeof(JToken).IsAssignableFrom(value));
			}
		}

		internal bool HasRequiredOrDefaultValueProperties
		{
			get
			{
				if (!_hasRequiredOrDefaultValueProperties.HasValue)
				{
					_hasRequiredOrDefaultValueProperties = false;
					if (ItemRequired.GetValueOrDefault(Required.Default) != 0)
					{
						_hasRequiredOrDefaultValueProperties = true;
					}
					else
					{
						foreach (JsonProperty property in Properties)
						{
							if (property.Required != 0 || ((int?)property.DefaultValueHandling & 2) == 2)
							{
								_hasRequiredOrDefaultValueProperties = true;
								break;
							}
						}
					}
				}
				return _hasRequiredOrDefaultValueProperties.GetValueOrDefault();
			}
		}

		public JsonObjectContract(Type underlyingType)
			: base(underlyingType)
		{
			ContractType = JsonContractType.Object;
			Properties = new JsonPropertyCollection(base.UnderlyingType);
		}

		internal object GetUninitializedObject()
		{
			if (!JsonTypeReflector.FullyTrusted)
			{
				throw new JsonException("Insufficient permissions. Creating an uninitialized '{0}' type requires full trust.".FormatWith(CultureInfo.InvariantCulture, NonNullableUnderlyingType));
			}
			return FormatterServices.GetUninitializedObject(NonNullableUnderlyingType);
		}
	}
}
