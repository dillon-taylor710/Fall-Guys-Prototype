using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public class JsonDictionaryContract : JsonContainerContract
	{
		private readonly Type _genericCollectionDefinitionType;

		private Type _genericWrapperType;

		private ObjectConstructor<object> _genericWrapperCreator;

		private Func<object> _genericTemporaryDictionaryCreator;

		private readonly ConstructorInfo _parameterizedConstructor;

		private ObjectConstructor<object> _overrideCreator;

		private ObjectConstructor<object> _parameterizedCreator;

		public Func<string, string> DictionaryKeyResolver
		{
			get;
			set;
		}

		public Type DictionaryKeyType
		{
			get;
			private set;
		}

		public Type DictionaryValueType
		{
			get;
			private set;
		}

		internal JsonContract KeyContract
		{
			get;
			set;
		}

		internal bool ShouldCreateWrapper
		{
			get;
			private set;
		}

		internal ObjectConstructor<object> ParameterizedCreator
		{
			get
			{
				if (_parameterizedCreator == null)
				{
					_parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(_parameterizedConstructor);
				}
				return _parameterizedCreator;
			}
		}

		public ObjectConstructor<object> OverrideCreator
		{
			get
			{
				return _overrideCreator;
			}
			set
			{
				_overrideCreator = value;
			}
		}

		public bool HasParameterizedCreator
		{
			get;
			set;
		}

		internal bool HasParameterizedCreatorInternal
		{
			get
			{
				if (!HasParameterizedCreator && _parameterizedCreator == null)
				{
					return _parameterizedConstructor != null;
				}
				return true;
			}
		}

		public JsonDictionaryContract(Type underlyingType)
			: base(underlyingType)
		{
			ContractType = JsonContractType.Dictionary;
			Type keyType;
			Type valueType;
			if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IDictionary<, >), out _genericCollectionDefinitionType))
			{
				keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
				valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];
				if (ReflectionUtils.IsGenericDefinition(base.UnderlyingType, typeof(IDictionary<, >)))
				{
					base.CreatedType = typeof(Dictionary<, >).MakeGenericType(keyType, valueType);
				}
			}
			else
			{
				ReflectionUtils.GetDictionaryKeyValueTypes(base.UnderlyingType, out keyType, out valueType);
				if (base.UnderlyingType == typeof(IDictionary))
				{
					base.CreatedType = typeof(Dictionary<object, object>);
				}
			}
			if (keyType != null && valueType != null)
			{
				_parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(base.CreatedType, typeof(KeyValuePair<, >).MakeGenericType(keyType, valueType), typeof(IDictionary<, >).MakeGenericType(keyType, valueType));
			}
			ShouldCreateWrapper = !typeof(IDictionary).IsAssignableFrom(base.CreatedType);
			DictionaryKeyType = keyType;
			DictionaryValueType = valueType;
			Type implementingType;
			if (DictionaryValueType != null && ReflectionUtils.IsNullableType(DictionaryValueType) && ReflectionUtils.InheritsGenericDefinition(base.CreatedType, typeof(Dictionary<, >), out implementingType))
			{
				ShouldCreateWrapper = true;
			}
		}

		internal IWrappedDictionary CreateWrapper(object dictionary)
		{
			if (_genericWrapperCreator == null)
			{
				_genericWrapperType = typeof(DictionaryWrapper<, >).MakeGenericType(DictionaryKeyType, DictionaryValueType);
				ConstructorInfo constructor = _genericWrapperType.GetConstructor(new Type[1]
				{
					_genericCollectionDefinitionType
				});
				_genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
			}
			return (IWrappedDictionary)_genericWrapperCreator(dictionary);
		}

		internal IDictionary CreateTemporaryDictionary()
		{
			if (_genericTemporaryDictionaryCreator == null)
			{
				Type type = typeof(Dictionary<, >).MakeGenericType(DictionaryKeyType ?? typeof(object), DictionaryValueType ?? typeof(object));
				_genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(type);
			}
			return (IDictionary)_genericTemporaryDictionaryCreator();
		}
	}
}
