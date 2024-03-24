using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public class JsonArrayContract : JsonContainerContract
	{
		private readonly Type _genericCollectionDefinitionType;

		private Type _genericWrapperType;

		private ObjectConstructor<object> _genericWrapperCreator;

		private Func<object> _genericTemporaryCollectionCreator;

		private readonly ConstructorInfo _parameterizedConstructor;

		private ObjectConstructor<object> _parameterizedCreator;

		private ObjectConstructor<object> _overrideCreator;

		public Type CollectionItemType
		{
			get;
			private set;
		}

		public bool IsMultidimensionalArray
		{
			get;
			private set;
		}

		internal bool IsArray
		{
			get;
			private set;
		}

		internal bool ShouldCreateWrapper
		{
			get;
			private set;
		}

		internal bool CanDeserialize
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
				CanDeserialize = true;
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

		public JsonArrayContract(Type underlyingType)
			: base(underlyingType)
		{
			ContractType = JsonContractType.Array;
			IsArray = base.CreatedType.IsArray;
			bool canDeserialize;
			Type implementingType;
			if (IsArray)
			{
				CollectionItemType = ReflectionUtils.GetCollectionItemType(base.UnderlyingType);
				IsReadOnlyOrFixedSize = true;
				_genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);
				canDeserialize = true;
				IsMultidimensionalArray = (IsArray && base.UnderlyingType.GetArrayRank() > 1);
			}
			else if (typeof(IList).IsAssignableFrom(underlyingType))
			{
				if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType))
				{
					CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];
				}
				else
				{
					CollectionItemType = ReflectionUtils.GetCollectionItemType(underlyingType);
				}
				if (underlyingType == typeof(IList))
				{
					base.CreatedType = typeof(List<object>);
				}
				if (CollectionItemType != null)
				{
					_parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(underlyingType, CollectionItemType);
				}
				IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(underlyingType, typeof(ReadOnlyCollection<>));
				canDeserialize = true;
			}
			else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType))
			{
				CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];
				if (ReflectionUtils.IsGenericDefinition(underlyingType, typeof(ICollection<>)) || ReflectionUtils.IsGenericDefinition(underlyingType, typeof(IList<>)))
				{
					base.CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);
				}
				_parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(underlyingType, CollectionItemType);
				canDeserialize = true;
				ShouldCreateWrapper = true;
			}
			else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IEnumerable<>), out implementingType))
			{
				CollectionItemType = implementingType.GetGenericArguments()[0];
				if (ReflectionUtils.IsGenericDefinition(base.UnderlyingType, typeof(IEnumerable<>)))
				{
					base.CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);
				}
				_parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(underlyingType, CollectionItemType);
				if (underlyingType.IsGenericType() && underlyingType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					_genericCollectionDefinitionType = implementingType;
					IsReadOnlyOrFixedSize = false;
					ShouldCreateWrapper = false;
					canDeserialize = true;
				}
				else
				{
					_genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);
					IsReadOnlyOrFixedSize = true;
					ShouldCreateWrapper = true;
					canDeserialize = HasParameterizedCreatorInternal;
				}
			}
			else
			{
				canDeserialize = false;
				ShouldCreateWrapper = true;
			}
			CanDeserialize = canDeserialize;
			if (CollectionItemType != null && ReflectionUtils.IsNullableType(CollectionItemType) && (ReflectionUtils.InheritsGenericDefinition(base.CreatedType, typeof(List<>), out implementingType) || (IsArray && !IsMultidimensionalArray)))
			{
				ShouldCreateWrapper = true;
			}
		}

		internal IWrappedCollection CreateWrapper(object list)
		{
			if (_genericWrapperCreator == null)
			{
				_genericWrapperType = typeof(CollectionWrapper<>).MakeGenericType(CollectionItemType);
				Type type = (!ReflectionUtils.InheritsGenericDefinition(_genericCollectionDefinitionType, typeof(List<>)) && _genericCollectionDefinitionType.GetGenericTypeDefinition() != typeof(IEnumerable<>)) ? _genericCollectionDefinitionType : typeof(ICollection<>).MakeGenericType(CollectionItemType);
				ConstructorInfo constructor = _genericWrapperType.GetConstructor(new Type[1]
				{
					type
				});
				_genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
			}
			return (IWrappedCollection)_genericWrapperCreator(list);
		}

		internal IList CreateTemporaryCollection()
		{
			if (_genericTemporaryCollectionCreator == null)
			{
				Type type = (IsMultidimensionalArray || CollectionItemType == null) ? typeof(object) : CollectionItemType;
				Type type2 = typeof(List<>).MakeGenericType(type);
				_genericTemporaryCollectionCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(type2);
			}
			return (IList)_genericTemporaryCollectionCreator();
		}
	}
}
