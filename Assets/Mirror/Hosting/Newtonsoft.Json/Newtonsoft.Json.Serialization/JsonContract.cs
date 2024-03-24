using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public abstract class JsonContract
	{
		internal bool IsNullable;

		internal bool IsConvertable;

		internal bool IsEnum;

		internal Type NonNullableUnderlyingType;

		internal ReadType InternalReadType;

		internal JsonContractType ContractType;

		internal bool IsReadOnlyOrFixedSize;

		internal bool IsSealed;

		internal bool IsInstantiable;

		private List<SerializationCallback> _onDeserializedCallbacks;

		private IList<SerializationCallback> _onDeserializingCallbacks;

		private IList<SerializationCallback> _onSerializedCallbacks;

		private IList<SerializationCallback> _onSerializingCallbacks;

		private IList<SerializationErrorCallback> _onErrorCallbacks;

		private Type _createdType;

		public Type UnderlyingType
		{
			get;
			private set;
		}

		public Type CreatedType
		{
			get
			{
				return _createdType;
			}
			set
			{
				_createdType = value;
				IsSealed = _createdType.IsSealed();
				IsInstantiable = (!_createdType.IsInterface() && !_createdType.IsAbstract());
			}
		}

		public bool? IsReference
		{
			get;
			set;
		}

		public JsonConverter Converter
		{
			get;
			set;
		}

		internal JsonConverter InternalConverter
		{
			get;
			set;
		}

		public IList<SerializationCallback> OnDeserializedCallbacks
		{
			get
			{
				if (_onDeserializedCallbacks == null)
				{
					_onDeserializedCallbacks = new List<SerializationCallback>();
				}
				return _onDeserializedCallbacks;
			}
		}

		public IList<SerializationCallback> OnDeserializingCallbacks
		{
			get
			{
				if (_onDeserializingCallbacks == null)
				{
					_onDeserializingCallbacks = new List<SerializationCallback>();
				}
				return _onDeserializingCallbacks;
			}
		}

		public IList<SerializationCallback> OnSerializedCallbacks
		{
			get
			{
				if (_onSerializedCallbacks == null)
				{
					_onSerializedCallbacks = new List<SerializationCallback>();
				}
				return _onSerializedCallbacks;
			}
		}

		public IList<SerializationCallback> OnSerializingCallbacks
		{
			get
			{
				if (_onSerializingCallbacks == null)
				{
					_onSerializingCallbacks = new List<SerializationCallback>();
				}
				return _onSerializingCallbacks;
			}
		}

		public IList<SerializationErrorCallback> OnErrorCallbacks
		{
			get
			{
				if (_onErrorCallbacks == null)
				{
					_onErrorCallbacks = new List<SerializationErrorCallback>();
				}
				return _onErrorCallbacks;
			}
		}

		public Func<object> DefaultCreator
		{
			get;
			set;
		}

		public bool DefaultCreatorNonPublic
		{
			get;
			set;
		}

		internal JsonContract(Type underlyingType)
		{
			ValidationUtils.ArgumentNotNull(underlyingType, "underlyingType");
			UnderlyingType = underlyingType;
			IsNullable = ReflectionUtils.IsNullable(underlyingType);
			NonNullableUnderlyingType = ((IsNullable && ReflectionUtils.IsNullableType(underlyingType)) ? Nullable.GetUnderlyingType(underlyingType) : underlyingType);
			CreatedType = NonNullableUnderlyingType;
			IsConvertable = ConvertUtils.IsConvertible(NonNullableUnderlyingType);
			IsEnum = NonNullableUnderlyingType.IsEnum();
			InternalReadType = ReadType.Read;
		}

		internal void InvokeOnSerializing(object o, StreamingContext context)
		{
			if (_onSerializingCallbacks != null)
			{
				foreach (SerializationCallback onSerializingCallback in _onSerializingCallbacks)
				{
					onSerializingCallback(o, context);
				}
			}
		}

		internal void InvokeOnSerialized(object o, StreamingContext context)
		{
			if (_onSerializedCallbacks != null)
			{
				foreach (SerializationCallback onSerializedCallback in _onSerializedCallbacks)
				{
					onSerializedCallback(o, context);
				}
			}
		}

		internal void InvokeOnDeserializing(object o, StreamingContext context)
		{
			if (_onDeserializingCallbacks != null)
			{
				foreach (SerializationCallback onDeserializingCallback in _onDeserializingCallbacks)
				{
					onDeserializingCallback(o, context);
				}
			}
		}

		internal void InvokeOnDeserialized(object o, StreamingContext context)
		{
			if (_onDeserializedCallbacks != null)
			{
				foreach (SerializationCallback onDeserializedCallback in _onDeserializedCallbacks)
				{
					onDeserializedCallback(o, context);
				}
			}
		}

		internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
		{
			if (_onErrorCallbacks != null)
			{
				foreach (SerializationErrorCallback onErrorCallback in _onErrorCallbacks)
				{
					onErrorCallback(o, context, errorContext);
				}
			}
		}

		internal static SerializationCallback CreateSerializationCallback(MethodInfo callbackMethodInfo)
		{
			return delegate(object o, StreamingContext context)
			{
				callbackMethodInfo.Invoke(o, new object[1]
				{
					context
				});
			};
		}

		internal static SerializationErrorCallback CreateSerializationErrorCallback(MethodInfo callbackMethodInfo)
		{
			return delegate(object o, StreamingContext context, ErrorContext econtext)
			{
				callbackMethodInfo.Invoke(o, new object[2]
				{
					context,
					econtext
				});
			};
		}
	}
}
