using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	internal class JsonSerializerInternalReader : JsonSerializerInternalBase
	{
		internal class CreatorPropertyContext
		{
			public string Name;

			public JsonProperty Property;

			public JsonProperty ConstructorProperty;

			public PropertyPresence? Presence;

			public object Value;

			public bool Used;
		}

		internal enum PropertyPresence
		{
			None,
			Null,
			Value
		}

		public JsonSerializerInternalReader(JsonSerializer serializer)
			: base(serializer)
		{
		}

		private JsonContract GetContractSafe(Type type)
		{
			if (type == null)
			{
				return null;
			}
			return Serializer._contractResolver.ResolveContract(type);
		}

		public object Deserialize(JsonReader reader, Type objectType, bool checkAdditionalContent)
		{
			if (reader != null)
			{
				JsonContract contractSafe = GetContractSafe(objectType);
				try
				{
					JsonConverter converter = GetConverter(contractSafe, null, null, null);
					if (reader.TokenType == JsonToken.None && !ReadForType(reader, contractSafe, converter != null))
					{
						if (contractSafe != null && !contractSafe.IsNullable)
						{
							throw JsonSerializationException.Create(reader, "No JSON content found and type '{0}' is not nullable.".FormatWith(CultureInfo.InvariantCulture, contractSafe.UnderlyingType));
						}
						return null;
					}
					object result = (converter == null || !converter.CanRead) ? CreateValueInternal(reader, objectType, contractSafe, null, null, null, null) : DeserializeConvertable(converter, reader, objectType, null);
					if (checkAdditionalContent && reader.Read() && reader.TokenType != JsonToken.Comment)
					{
						throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
					}
					return result;
				}
				catch (Exception ex)
				{
					if (!IsErrorHandled(null, contractSafe, null, reader as IJsonLineInfo, reader.Path, ex))
					{
						ClearErrorContext();
						throw;
					}
					HandleError(reader, false, 0);
					return null;
				}
			}
			throw new ArgumentNullException("reader");
		}

		private JsonSerializerProxy GetInternalSerializer()
		{
			if (InternalSerializer == null)
			{
				InternalSerializer = new JsonSerializerProxy(this);
			}
			return InternalSerializer;
		}

		private JToken CreateJToken(JsonReader reader, JsonContract contract)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			if (contract != null)
			{
				if (contract.UnderlyingType == typeof(JRaw))
				{
					return JRaw.Create(reader);
				}
				if (reader.TokenType == JsonToken.Null && contract.UnderlyingType != typeof(JValue) && contract.UnderlyingType != typeof(JToken))
				{
					return null;
				}
			}
			using (JTokenWriter jTokenWriter = new JTokenWriter())
			{
				jTokenWriter.WriteToken(reader);
				return jTokenWriter.Token;
			}
		}

		private JToken CreateJObject(JsonReader reader)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");
			using (JTokenWriter jTokenWriter = new JTokenWriter())
			{
				jTokenWriter.WriteStartObject();
				do
				{
					if (reader.TokenType == JsonToken.PropertyName)
					{
						string text = (string)reader.Value;
						if (!reader.ReadAndMoveToContent())
						{
							break;
						}
						if (!CheckPropertyName(reader, text))
						{
							jTokenWriter.WritePropertyName(text);
							jTokenWriter.WriteToken(reader, true, true, false);
						}
					}
					else if (reader.TokenType != JsonToken.Comment)
					{
						jTokenWriter.WriteEndObject();
						return jTokenWriter.Token;
					}
				}
				while (reader.Read());
				throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
			}
		}

		private object CreateValueInternal(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue)
		{
			if (contract != null && contract.ContractType == JsonContractType.Linq)
			{
				return CreateJToken(reader, contract);
			}
			do
			{
				switch (reader.TokenType)
				{
				case JsonToken.StartObject:
					return CreateObject(reader, objectType, contract, member, containerContract, containerMember, existingValue);
				case JsonToken.StartArray:
					return CreateList(reader, objectType, contract, member, existingValue, null);
				case JsonToken.Integer:
				case JsonToken.Float:
				case JsonToken.Boolean:
				case JsonToken.Date:
				case JsonToken.Bytes:
					return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
				case JsonToken.String:
				{
					string text = (string)reader.Value;
					if (CoerceEmptyStringToNull(objectType, contract, text))
					{
						return null;
					}
					if (objectType == typeof(byte[]))
					{
						return Convert.FromBase64String(text);
					}
					return EnsureType(reader, text, CultureInfo.InvariantCulture, contract, objectType);
				}
				case JsonToken.StartConstructor:
				{
					string value = reader.Value.ToString();
					return EnsureType(reader, value, CultureInfo.InvariantCulture, contract, objectType);
				}
				case JsonToken.Null:
				case JsonToken.Undefined:
					if (objectType == typeof(DBNull))
					{
						return DBNull.Value;
					}
					return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
				case JsonToken.Raw:
					return new JRaw((string)reader.Value);
				default:
					throw JsonSerializationException.Create(reader, "Unexpected token while deserializing object: " + reader.TokenType);
				case JsonToken.Comment:
					break;
				}
			}
			while (reader.Read());
			throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
		}

		private static bool CoerceEmptyStringToNull(Type objectType, JsonContract contract, string s)
		{
			if (string.IsNullOrEmpty(s) && objectType != null && objectType != typeof(string) && objectType != typeof(object) && contract != null)
			{
				return contract.IsNullable;
			}
			return false;
		}

		internal string GetExpectedDescription(JsonContract contract)
		{
			switch (contract.ContractType)
			{
			case JsonContractType.Object:
			case JsonContractType.Dictionary:
			case JsonContractType.Serializable:
				return "JSON object (e.g. {\"name\":\"value\"})";
			case JsonContractType.Array:
				return "JSON array (e.g. [1,2,3])";
			case JsonContractType.Primitive:
				return "JSON primitive value (e.g. string, number, boolean, null)";
			case JsonContractType.String:
				return "JSON string value";
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private JsonConverter GetConverter(JsonContract contract, JsonConverter memberConverter, JsonContainerContract containerContract, JsonProperty containerProperty)
		{
			JsonConverter result = null;
			if (memberConverter != null)
			{
				result = memberConverter;
			}
			else if (containerProperty != null && containerProperty.ItemConverter != null)
			{
				result = containerProperty.ItemConverter;
			}
			else if (containerContract != null && containerContract.ItemConverter != null)
			{
				result = containerContract.ItemConverter;
			}
			else if (contract != null)
			{
				JsonConverter matchingConverter;
				if (contract.Converter != null)
				{
					result = contract.Converter;
				}
				else if ((matchingConverter = Serializer.GetMatchingConverter(contract.UnderlyingType)) != null)
				{
					result = matchingConverter;
				}
				else if (contract.InternalConverter != null)
				{
					result = contract.InternalConverter;
				}
			}
			return result;
		}

		private object CreateObject(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue)
		{
			Type objectType2 = objectType;
			string id;
			if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.Ignore)
			{
				reader.ReadAndAssert();
				id = null;
			}
			else if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
			{
				JTokenReader jTokenReader = reader as JTokenReader;
				if (jTokenReader == null)
				{
					jTokenReader = (JTokenReader)JToken.ReadFrom(reader).CreateReader();
					jTokenReader.Culture = reader.Culture;
					jTokenReader.DateFormatString = reader.DateFormatString;
					jTokenReader.DateParseHandling = reader.DateParseHandling;
					jTokenReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
					jTokenReader.FloatParseHandling = reader.FloatParseHandling;
					jTokenReader.SupportMultipleContent = reader.SupportMultipleContent;
					jTokenReader.ReadAndAssert();
					reader = jTokenReader;
				}
				object newValue;
				if (ReadMetadataPropertiesToken(jTokenReader, ref objectType2, ref contract, member, containerContract, containerMember, existingValue, out newValue, out id))
				{
					return newValue;
				}
			}
			else
			{
				reader.ReadAndAssert();
				object newValue2;
				if (ReadMetadataProperties(reader, ref objectType2, ref contract, member, containerContract, containerMember, existingValue, out newValue2, out id))
				{
					return newValue2;
				}
			}
			if (HasNoDefinedType(contract))
			{
				return CreateJObject(reader);
			}
			switch (contract.ContractType)
			{
			case JsonContractType.Object:
			{
				bool createdFromNonDefaultCreator2 = false;
				JsonObjectContract jsonObjectContract = (JsonObjectContract)contract;
				object obj = (existingValue == null || (objectType2 != objectType && !objectType2.IsAssignableFrom(existingValue.GetType()))) ? CreateNewObject(reader, jsonObjectContract, member, containerMember, id, out createdFromNonDefaultCreator2) : existingValue;
				if (createdFromNonDefaultCreator2)
				{
					return obj;
				}
				return PopulateObject(obj, reader, jsonObjectContract, member, id);
			}
			case JsonContractType.Primitive:
			{
				JsonPrimitiveContract contract3 = (JsonPrimitiveContract)contract;
				if (Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore && reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value.ToString(), "$value", StringComparison.Ordinal))
				{
					reader.ReadAndAssert();
					if (reader.TokenType == JsonToken.StartObject)
					{
						throw JsonSerializationException.Create(reader, "Unexpected token when deserializing primitive value: " + reader.TokenType);
					}
					object result = CreateValueInternal(reader, objectType2, contract3, member, null, null, existingValue);
					reader.ReadAndAssert();
					return result;
				}
				break;
			}
			case JsonContractType.Dictionary:
			{
				JsonDictionaryContract jsonDictionaryContract = (JsonDictionaryContract)contract;
				if (existingValue == null)
				{
					bool createdFromNonDefaultCreator;
					IDictionary dictionary = CreateNewDictionary(reader, jsonDictionaryContract, out createdFromNonDefaultCreator);
					if (createdFromNonDefaultCreator)
					{
						if (id != null)
						{
							throw JsonSerializationException.Create(reader, "Cannot preserve reference to readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
						}
						if (contract.OnSerializingCallbacks.Count > 0)
						{
							throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
						}
						if (contract.OnErrorCallbacks.Count > 0)
						{
							throw JsonSerializationException.Create(reader, "Cannot call OnError on readonly list, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
						}
						if (!jsonDictionaryContract.HasParameterizedCreatorInternal)
						{
							throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size dictionary: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
						}
					}
					PopulateDictionary(dictionary, reader, jsonDictionaryContract, member, id);
					if (createdFromNonDefaultCreator)
					{
						return (jsonDictionaryContract.OverrideCreator ?? jsonDictionaryContract.ParameterizedCreator)(dictionary);
					}
					if (dictionary is IWrappedDictionary)
					{
						return ((IWrappedDictionary)dictionary).UnderlyingDictionary;
					}
					return dictionary;
				}
				object dictionary2;
				if (!jsonDictionaryContract.ShouldCreateWrapper)
				{
					dictionary2 = (IDictionary)existingValue;
				}
				else
				{
					IDictionary dictionary3 = jsonDictionaryContract.CreateWrapper(existingValue);
					dictionary2 = dictionary3;
				}
				return PopulateDictionary((IDictionary)dictionary2, reader, jsonDictionaryContract, member, id);
			}
			case JsonContractType.Serializable:
			{
				JsonISerializableContract contract2 = (JsonISerializableContract)contract;
				return CreateISerializable(reader, contract2, member, id);
			}
			}
			string format = "Cannot deserialize the current JSON object (e.g. {{\"name\":\"value\"}}) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine + "To fix this error either change the JSON to a {1} or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object." + Environment.NewLine;
			format = format.FormatWith(CultureInfo.InvariantCulture, objectType2, GetExpectedDescription(contract));
			throw JsonSerializationException.Create(reader, format);
		}

		private bool ReadMetadataPropertiesToken(JTokenReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue, out object newValue, out string id)
		{
			id = null;
			newValue = null;
			if (reader.TokenType == JsonToken.StartObject)
			{
				JObject jObject = (JObject)reader.CurrentToken;
				JToken jToken = jObject["$ref"];
				if (jToken != null)
				{
					if (jToken.Type != JTokenType.String && jToken.Type != JTokenType.Null)
					{
						throw JsonSerializationException.Create(jToken, jToken.Path, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, "$ref"), null);
					}
					JToken parent = jToken.Parent;
					JToken jToken2 = null;
					if (parent.Next != null)
					{
						jToken2 = parent.Next;
					}
					else if (parent.Previous != null)
					{
						jToken2 = parent.Previous;
					}
					string text = (string)jToken;
					if (text != null)
					{
						if (jToken2 != null)
						{
							throw JsonSerializationException.Create(jToken2, jToken2.Path, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, "$ref"), null);
						}
						newValue = Serializer.GetReferenceResolver().ResolveReference(this, text);
						if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
						{
							TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader, reader.Path, "Resolved object reference '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, text, newValue.GetType())), null);
						}
						reader.Skip();
						return true;
					}
				}
				JToken jToken3 = jObject["$type"];
				if (jToken3 != null)
				{
					string qualifiedTypeName = (string)jToken3;
					JsonReader jsonReader = jToken3.CreateReader();
					jsonReader.ReadAndAssert();
					ResolveTypeName(jsonReader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName);
					if (jObject["$value"] != null)
					{
						while (true)
						{
							reader.ReadAndAssert();
							if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "$value")
							{
								break;
							}
							reader.ReadAndAssert();
							reader.Skip();
						}
						return false;
					}
				}
				JToken jToken4 = jObject["$id"];
				if (jToken4 != null)
				{
					id = (string)jToken4;
				}
				JToken jToken5 = jObject["$values"];
				if (jToken5 != null)
				{
					JsonReader jsonReader2 = jToken5.CreateReader();
					jsonReader2.ReadAndAssert();
					newValue = CreateList(jsonReader2, objectType, contract, member, existingValue, id);
					reader.Skip();
					return true;
				}
			}
			reader.ReadAndAssert();
			return false;
		}

		private bool ReadMetadataProperties(JsonReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue, out object newValue, out string id)
		{
			id = null;
			newValue = null;
			if (reader.TokenType == JsonToken.PropertyName)
			{
				string text = reader.Value.ToString();
				if (text.Length > 0 && text[0] == '$')
				{
					bool flag;
					do
					{
						text = reader.Value.ToString();
						if (string.Equals(text, "$ref", StringComparison.Ordinal))
						{
							reader.ReadAndAssert();
							if (reader.TokenType != JsonToken.String && reader.TokenType != JsonToken.Null)
							{
								throw JsonSerializationException.Create(reader, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, "$ref"));
							}
							string text2 = (reader.Value != null) ? reader.Value.ToString() : null;
							reader.ReadAndAssert();
							if (text2 != null)
							{
								if (reader.TokenType == JsonToken.PropertyName)
								{
									throw JsonSerializationException.Create(reader, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, "$ref"));
								}
								newValue = Serializer.GetReferenceResolver().ResolveReference(this, text2);
								if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
								{
									TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved object reference '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, text2, newValue.GetType())), null);
								}
								return true;
							}
							flag = true;
						}
						else if (string.Equals(text, "$type", StringComparison.Ordinal))
						{
							reader.ReadAndAssert();
							string qualifiedTypeName = reader.Value.ToString();
							ResolveTypeName(reader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName);
							reader.ReadAndAssert();
							flag = true;
						}
						else if (string.Equals(text, "$id", StringComparison.Ordinal))
						{
							reader.ReadAndAssert();
							id = ((reader.Value != null) ? reader.Value.ToString() : null);
							reader.ReadAndAssert();
							flag = true;
						}
						else
						{
							if (string.Equals(text, "$values", StringComparison.Ordinal))
							{
								reader.ReadAndAssert();
								object obj = CreateList(reader, objectType, contract, member, existingValue, id);
								reader.ReadAndAssert();
								newValue = obj;
								return true;
							}
							flag = false;
						}
					}
					while (flag && reader.TokenType == JsonToken.PropertyName);
				}
			}
			return false;
		}

		private void ResolveTypeName(JsonReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, string qualifiedTypeName)
		{
			if ((((member != null) ? member.TypeNameHandling : null) ?? ((containerContract != null) ? containerContract.ItemTypeNameHandling : null) ?? ((containerMember != null) ? containerMember.ItemTypeNameHandling : null) ?? Serializer._typeNameHandling) != 0)
			{
				string typeName;
				string assemblyName;
				ReflectionUtils.SplitFullyQualifiedTypeName(qualifiedTypeName, out typeName, out assemblyName);
				Type type;
				try
				{
					type = Serializer._binder.BindToType(assemblyName, typeName);
				}
				catch (Exception ex)
				{
					throw JsonSerializationException.Create(reader, "Error resolving type specified in JSON '{0}'.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName), ex);
				}
				if (type == null)
				{
					throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' was not resolved.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName));
				}
				if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
				{
					TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved type '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName, type)), null);
				}
				if (objectType != null && !objectType.IsAssignableFrom(type))
				{
					throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' is not compatible with '{1}'.".FormatWith(CultureInfo.InvariantCulture, type.AssemblyQualifiedName, objectType.AssemblyQualifiedName));
				}
				objectType = type;
				contract = GetContractSafe(type);
			}
		}

		private JsonArrayContract EnsureArrayContract(JsonReader reader, Type objectType, JsonContract contract)
		{
			if (contract == null)
			{
				throw JsonSerializationException.Create(reader, "Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			JsonArrayContract obj = contract as JsonArrayContract;
			if (obj == null)
			{
				string format = "Cannot deserialize the current JSON array (e.g. [1,2,3]) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine + "To fix this error either change the JSON to a {1} or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array." + Environment.NewLine;
				format = format.FormatWith(CultureInfo.InvariantCulture, objectType, GetExpectedDescription(contract));
				throw JsonSerializationException.Create(reader, format);
			}
			return obj;
		}

		private object CreateList(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue, string id)
		{
			if (HasNoDefinedType(contract))
			{
				return CreateJToken(reader, contract);
			}
			JsonArrayContract jsonArrayContract = EnsureArrayContract(reader, objectType, contract);
			if (existingValue == null)
			{
				bool createdFromNonDefaultCreator;
				IList list = CreateNewList(reader, jsonArrayContract, out createdFromNonDefaultCreator);
				if (createdFromNonDefaultCreator)
				{
					if (id != null)
					{
						throw JsonSerializationException.Create(reader, "Cannot preserve reference to array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
					}
					if (contract.OnSerializingCallbacks.Count > 0)
					{
						throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
					}
					if (contract.OnErrorCallbacks.Count > 0)
					{
						throw JsonSerializationException.Create(reader, "Cannot call OnError on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
					}
					if (!jsonArrayContract.HasParameterizedCreatorInternal && !jsonArrayContract.IsArray)
					{
						throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
					}
				}
				if (!jsonArrayContract.IsMultidimensionalArray)
				{
					PopulateList(list, reader, jsonArrayContract, member, id);
				}
				else
				{
					PopulateMultidimensionalArray(list, reader, jsonArrayContract, member, id);
				}
				if (createdFromNonDefaultCreator)
				{
					if (jsonArrayContract.IsMultidimensionalArray)
					{
						list = CollectionUtils.ToMultidimensionalArray(list, jsonArrayContract.CollectionItemType, contract.CreatedType.GetArrayRank());
					}
					else
					{
						if (!jsonArrayContract.IsArray)
						{
							return (jsonArrayContract.OverrideCreator ?? jsonArrayContract.ParameterizedCreator)(list);
						}
						Array array = Array.CreateInstance(jsonArrayContract.CollectionItemType, list.Count);
						list.CopyTo(array, 0);
						list = array;
					}
				}
				else if (list is IWrappedCollection)
				{
					return ((IWrappedCollection)list).UnderlyingCollection;
				}
				return list;
			}
			if (!jsonArrayContract.CanDeserialize)
			{
				throw JsonSerializationException.Create(reader, "Cannot populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.CreatedType));
			}
			object list2;
			if (!jsonArrayContract.ShouldCreateWrapper)
			{
				list2 = (IList)existingValue;
			}
			else
			{
				IList list3 = jsonArrayContract.CreateWrapper(existingValue);
				list2 = list3;
			}
			return PopulateList((IList)list2, reader, jsonArrayContract, member, id);
		}

		private bool HasNoDefinedType(JsonContract contract)
		{
			if (contract != null && contract.UnderlyingType != typeof(object))
			{
				return contract.ContractType == JsonContractType.Linq;
			}
			return true;
		}

		private object EnsureType(JsonReader reader, object value, CultureInfo culture, JsonContract contract, Type targetType)
		{
			if (targetType == null)
			{
				return value;
			}
			if (ReflectionUtils.GetObjectType(value) != targetType)
			{
				if (value != null || !contract.IsNullable)
				{
					try
					{
						if (contract.IsConvertable)
						{
							JsonPrimitiveContract jsonPrimitiveContract = (JsonPrimitiveContract)contract;
							if (contract.IsEnum)
							{
								if (value is string)
								{
									return Enum.Parse(contract.NonNullableUnderlyingType, value.ToString(), true);
								}
								if (ConvertUtils.IsInteger(jsonPrimitiveContract.TypeCode))
								{
									return Enum.ToObject(contract.NonNullableUnderlyingType, value);
								}
							}
							return Convert.ChangeType(value, contract.NonNullableUnderlyingType, culture);
						}
						return ConvertUtils.ConvertOrCast(value, culture, contract.NonNullableUnderlyingType);
					}
					catch (Exception ex)
					{
						throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.FormatValueForPrint(value), targetType), ex);
					}
				}
				return null;
			}
			return value;
		}

		private bool SetPropertyValue(JsonProperty property, JsonConverter propertyConverter, JsonContainerContract containerContract, JsonProperty containerProperty, JsonReader reader, object target)
		{
			bool useExistingValue;
			object currentValue;
			JsonContract propertyContract;
			bool gottenCurrentValue;
			if (CalculatePropertyDetails(property, ref propertyConverter, containerContract, containerProperty, reader, target, out useExistingValue, out currentValue, out propertyContract, out gottenCurrentValue))
			{
				return false;
			}
			object obj;
			if (propertyConverter != null && propertyConverter.CanRead)
			{
				if (!gottenCurrentValue && target != null && property.Readable)
				{
					currentValue = property.ValueProvider.GetValue(target);
				}
				obj = DeserializeConvertable(propertyConverter, reader, property.PropertyType, currentValue);
			}
			else
			{
				obj = CreateValueInternal(reader, property.PropertyType, propertyContract, property, containerContract, containerProperty, useExistingValue ? currentValue : null);
			}
			if ((!useExistingValue || obj != currentValue) && ShouldSetPropertyValue(property, obj))
			{
				property.ValueProvider.SetValue(target, obj);
				if (property.SetIsSpecified != null)
				{
					if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
					{
						TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "IsSpecified for property '{0}' on {1} set to true.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType)), null);
					}
					property.SetIsSpecified(target, true);
				}
				return true;
			}
			return useExistingValue;
		}

		private bool CalculatePropertyDetails(JsonProperty property, ref JsonConverter propertyConverter, JsonContainerContract containerContract, JsonProperty containerProperty, JsonReader reader, object target, out bool useExistingValue, out object currentValue, out JsonContract propertyContract, out bool gottenCurrentValue)
		{
			currentValue = null;
			useExistingValue = false;
			propertyContract = null;
			gottenCurrentValue = false;
			if (property.Ignored)
			{
				return true;
			}
			JsonToken tokenType = reader.TokenType;
			if (property.PropertyContract == null)
			{
				property.PropertyContract = GetContractSafe(property.PropertyType);
			}
			if (property.ObjectCreationHandling.GetValueOrDefault(Serializer._objectCreationHandling) != ObjectCreationHandling.Replace && (tokenType == JsonToken.StartArray || tokenType == JsonToken.StartObject) && property.Readable)
			{
				currentValue = property.ValueProvider.GetValue(target);
				gottenCurrentValue = true;
				if (currentValue != null)
				{
					propertyContract = GetContractSafe(currentValue.GetType());
					useExistingValue = (!propertyContract.IsReadOnlyOrFixedSize && !propertyContract.UnderlyingType.IsValueType());
				}
			}
			if (!property.Writable && !useExistingValue)
			{
				return true;
			}
			if (property.NullValueHandling.GetValueOrDefault(Serializer._nullValueHandling) == NullValueHandling.Ignore && tokenType == JsonToken.Null)
			{
				return true;
			}
			if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Ignore) && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate) && JsonTokenUtils.IsPrimitiveToken(tokenType) && MiscellaneousUtils.ValueEquals(reader.Value, property.GetResolvedDefaultValue()))
			{
				return true;
			}
			if (currentValue == null)
			{
				propertyContract = property.PropertyContract;
			}
			else
			{
				propertyContract = GetContractSafe(currentValue.GetType());
				if (propertyContract != property.PropertyContract)
				{
					propertyConverter = GetConverter(propertyContract, property.MemberConverter, containerContract, containerProperty);
				}
			}
			return false;
		}

		private void AddReference(JsonReader reader, string id, object value)
		{
			try
			{
				if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
				{
					TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Read object reference Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, id, value.GetType())), null);
				}
				Serializer.GetReferenceResolver().AddReference(this, id, value);
			}
			catch (Exception ex)
			{
				throw JsonSerializationException.Create(reader, "Error reading object reference '{0}'.".FormatWith(CultureInfo.InvariantCulture, id), ex);
			}
		}

		private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
		{
			return (value & flag) == flag;
		}

		private bool ShouldSetPropertyValue(JsonProperty property, object value)
		{
			if (property.NullValueHandling.GetValueOrDefault(Serializer._nullValueHandling) == NullValueHandling.Ignore && value == null)
			{
				return false;
			}
			if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Ignore) && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate) && MiscellaneousUtils.ValueEquals(value, property.GetResolvedDefaultValue()))
			{
				return false;
			}
			if (!property.Writable)
			{
				return false;
			}
			return true;
		}

		private IList CreateNewList(JsonReader reader, JsonArrayContract contract, out bool createdFromNonDefaultCreator)
		{
			if (!contract.CanDeserialize)
			{
				throw JsonSerializationException.Create(reader, "Cannot create and populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.CreatedType));
			}
			if (contract.OverrideCreator != null)
			{
				if (contract.HasParameterizedCreator)
				{
					createdFromNonDefaultCreator = true;
					return contract.CreateTemporaryCollection();
				}
				createdFromNonDefaultCreator = false;
				return (IList)contract.OverrideCreator();
			}
			if (contract.IsReadOnlyOrFixedSize)
			{
				createdFromNonDefaultCreator = true;
				IList list = contract.CreateTemporaryCollection();
				if (contract.ShouldCreateWrapper)
				{
					list = contract.CreateWrapper(list);
				}
				return list;
			}
			if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
			{
				object obj = contract.DefaultCreator();
				if (contract.ShouldCreateWrapper)
				{
					obj = contract.CreateWrapper(obj);
				}
				createdFromNonDefaultCreator = false;
				return (IList)obj;
			}
			if (contract.HasParameterizedCreatorInternal)
			{
				createdFromNonDefaultCreator = true;
				return contract.CreateTemporaryCollection();
			}
			if (!contract.IsInstantiable)
			{
				throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
			}
			throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
		}

		private IDictionary CreateNewDictionary(JsonReader reader, JsonDictionaryContract contract, out bool createdFromNonDefaultCreator)
		{
			if (contract.OverrideCreator != null)
			{
				if (contract.HasParameterizedCreator)
				{
					createdFromNonDefaultCreator = true;
					return contract.CreateTemporaryDictionary();
				}
				createdFromNonDefaultCreator = false;
				return (IDictionary)contract.OverrideCreator();
			}
			if (contract.IsReadOnlyOrFixedSize)
			{
				createdFromNonDefaultCreator = true;
				return contract.CreateTemporaryDictionary();
			}
			if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
			{
				object obj = contract.DefaultCreator();
				if (contract.ShouldCreateWrapper)
				{
					obj = contract.CreateWrapper(obj);
				}
				createdFromNonDefaultCreator = false;
				return (IDictionary)obj;
			}
			if (contract.HasParameterizedCreatorInternal)
			{
				createdFromNonDefaultCreator = true;
				return contract.CreateTemporaryDictionary();
			}
			if (!contract.IsInstantiable)
			{
				throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
			}
			throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
		}

		private void OnDeserializing(JsonReader reader, JsonContract contract, object value)
		{
			if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
			{
				TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
			}
			contract.InvokeOnDeserializing(value, Serializer._context);
		}

		private void OnDeserialized(JsonReader reader, JsonContract contract, object value)
		{
			if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
			{
				TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
			}
			contract.InvokeOnDeserialized(value, Serializer._context);
		}

		private object PopulateDictionary(IDictionary dictionary, JsonReader reader, JsonDictionaryContract contract, JsonProperty containerProperty, string id)
		{
			IWrappedDictionary wrappedDictionary = dictionary as IWrappedDictionary;
			object obj = (wrappedDictionary != null) ? wrappedDictionary.UnderlyingDictionary : dictionary;
			if (id != null)
			{
				AddReference(reader, id, obj);
			}
			OnDeserializing(reader, contract, obj);
			int depth = reader.Depth;
			if (contract.KeyContract == null)
			{
				contract.KeyContract = GetContractSafe(contract.DictionaryKeyType);
			}
			if (contract.ItemContract == null)
			{
				contract.ItemContract = GetContractSafe(contract.DictionaryValueType);
			}
			JsonConverter jsonConverter = contract.ItemConverter ?? GetConverter(contract.ItemContract, null, contract, containerProperty);
			PrimitiveTypeCode primitiveTypeCode = (contract.KeyContract is JsonPrimitiveContract) ? ((JsonPrimitiveContract)contract.KeyContract).TypeCode : PrimitiveTypeCode.Empty;
			bool flag = false;
			do
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					object obj2 = reader.Value;
					if (!CheckPropertyName(reader, obj2.ToString()))
					{
						try
						{
							try
							{
								switch (primitiveTypeCode)
								{
								case PrimitiveTypeCode.DateTime:
								case PrimitiveTypeCode.DateTimeNullable:
								{
									DateTime dt2;
									obj2 = ((!DateTimeUtils.TryParseDateTime(obj2.ToString(), reader.DateTimeZoneHandling, reader.DateFormatString, reader.Culture, out dt2)) ? EnsureType(reader, obj2, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType) : ((object)dt2));
									break;
								}
								case PrimitiveTypeCode.DateTimeOffset:
								case PrimitiveTypeCode.DateTimeOffsetNullable:
								{
									DateTimeOffset dt;
									obj2 = ((!DateTimeUtils.TryParseDateTimeOffset(obj2.ToString(), reader.DateFormatString, reader.Culture, out dt)) ? EnsureType(reader, obj2, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType) : ((object)dt));
									break;
								}
								default:
									obj2 = EnsureType(reader, obj2, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
									break;
								}
							}
							catch (Exception ex)
							{
								throw JsonSerializationException.Create(reader, "Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, reader.Value, contract.DictionaryKeyType), ex);
							}
							if (!ReadForType(reader, contract.ItemContract, jsonConverter != null))
							{
								throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
							}
							object obj4 = dictionary[obj2] = ((jsonConverter == null || !jsonConverter.CanRead) ? CreateValueInternal(reader, contract.DictionaryValueType, contract.ItemContract, null, contract, containerProperty, null) : DeserializeConvertable(jsonConverter, reader, contract.DictionaryValueType, null));
						}
						catch (Exception ex2)
						{
							if (!IsErrorHandled(obj, contract, obj2, reader as IJsonLineInfo, reader.Path, ex2))
							{
								throw;
							}
							HandleError(reader, true, depth);
						}
					}
					break;
				}
				case JsonToken.EndObject:
					flag = true;
					break;
				default:
					throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
				case JsonToken.Comment:
					break;
				}
			}
			while (!flag && reader.Read());
			if (!flag)
			{
				ThrowUnexpectedEndException(reader, contract, obj, "Unexpected end when deserializing object.");
			}
			OnDeserialized(reader, contract, obj);
			return obj;
		}

		private object PopulateMultidimensionalArray(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id)
		{
			int arrayRank = contract.UnderlyingType.GetArrayRank();
			if (id != null)
			{
				AddReference(reader, id, list);
			}
			OnDeserializing(reader, contract, list);
			JsonContract contractSafe = GetContractSafe(contract.CollectionItemType);
			JsonConverter converter = GetConverter(contractSafe, null, contract, containerProperty);
			int? num = null;
			Stack<IList> stack = new Stack<IList>();
			stack.Push(list);
			IList list2 = list;
			bool flag = false;
			do
			{
				int depth = reader.Depth;
				if (stack.Count == arrayRank)
				{
					try
					{
						if (ReadForType(reader, contractSafe, converter != null))
						{
							JsonToken tokenType = reader.TokenType;
							if (tokenType == JsonToken.EndArray)
							{
								stack.Pop();
								list2 = stack.Peek();
								num = null;
							}
							else
							{
								object value = (converter == null || !converter.CanRead) ? CreateValueInternal(reader, contract.CollectionItemType, contractSafe, null, contract, containerProperty, null) : DeserializeConvertable(converter, reader, contract.CollectionItemType, null);
								list2.Add(value);
							}
							continue;
						}
					}
					catch (Exception ex)
					{
						JsonPosition position = reader.GetPosition(depth);
						if (!IsErrorHandled(list, contract, position.Position, reader as IJsonLineInfo, reader.Path, ex))
						{
							throw;
						}
						HandleError(reader, true, depth);
						if (num.HasValue && num == position.Position)
						{
							throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
						}
						num = position.Position;
						continue;
					}
					break;
				}
				if (!reader.Read())
				{
					break;
				}
				switch (reader.TokenType)
				{
				case JsonToken.StartArray:
				{
					IList list3 = new List<object>();
					list2.Add(list3);
					stack.Push(list3);
					list2 = list3;
					break;
				}
				case JsonToken.EndArray:
					stack.Pop();
					if (stack.Count > 0)
					{
						list2 = stack.Peek();
					}
					else
					{
						flag = true;
					}
					break;
				default:
					throw JsonSerializationException.Create(reader, "Unexpected token when deserializing multidimensional array: " + reader.TokenType);
				case JsonToken.Comment:
					break;
				}
			}
			while (!flag);
			if (!flag)
			{
				ThrowUnexpectedEndException(reader, contract, list, "Unexpected end when deserializing array.");
			}
			OnDeserialized(reader, contract, list);
			return list;
		}

		private void ThrowUnexpectedEndException(JsonReader reader, JsonContract contract, object currentObject, string message)
		{
			try
			{
				throw JsonSerializationException.Create(reader, message);
			}
			catch (Exception ex)
			{
				if (!IsErrorHandled(currentObject, contract, null, reader as IJsonLineInfo, reader.Path, ex))
				{
					throw;
				}
				HandleError(reader, false, 0);
			}
		}

		private object PopulateList(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id)
		{
			IWrappedCollection wrappedCollection = list as IWrappedCollection;
			object obj = (wrappedCollection != null) ? wrappedCollection.UnderlyingCollection : list;
			if (id != null)
			{
				AddReference(reader, id, obj);
			}
			if (list.IsFixedSize)
			{
				reader.Skip();
				return obj;
			}
			OnDeserializing(reader, contract, obj);
			int depth = reader.Depth;
			if (contract.ItemContract == null)
			{
				contract.ItemContract = GetContractSafe(contract.CollectionItemType);
			}
			JsonConverter converter = GetConverter(contract.ItemContract, null, contract, containerProperty);
			int? num = null;
			bool flag = false;
			do
			{
				try
				{
					if (ReadForType(reader, contract.ItemContract, converter != null))
					{
						JsonToken tokenType = reader.TokenType;
						if (tokenType == JsonToken.EndArray)
						{
							flag = true;
						}
						else
						{
							object value = (converter == null || !converter.CanRead) ? CreateValueInternal(reader, contract.CollectionItemType, contract.ItemContract, null, contract, containerProperty, null) : DeserializeConvertable(converter, reader, contract.CollectionItemType, null);
							list.Add(value);
						}
						continue;
					}
				}
				catch (Exception ex)
				{
					JsonPosition position = reader.GetPosition(depth);
					if (!IsErrorHandled(obj, contract, position.Position, reader as IJsonLineInfo, reader.Path, ex))
					{
						throw;
					}
					HandleError(reader, true, depth);
					if (num.HasValue && num == position.Position)
					{
						throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
					}
					num = position.Position;
					continue;
				}
				break;
			}
			while (!flag);
			if (!flag)
			{
				ThrowUnexpectedEndException(reader, contract, obj, "Unexpected end when deserializing array.");
			}
			OnDeserialized(reader, contract, obj);
			return obj;
		}

		private object CreateISerializable(JsonReader reader, JsonISerializableContract contract, JsonProperty member, string id)
		{
			Type underlyingType = contract.UnderlyingType;
			if (!JsonTypeReflector.FullyTrusted)
			{
				string format = "Type '{0}' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine + "To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine;
				format = format.FormatWith(CultureInfo.InvariantCulture, underlyingType);
				throw JsonSerializationException.Create(reader, format);
			}
			if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
			{
				TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using ISerializable constructor.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
			}
//			SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new JsonFormatterConverter(this, contract, member));
			bool flag = false;
			do
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string text = reader.Value.ToString();
					if (!reader.Read())
					{
						throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, text));
					}
//					serializationInfo.AddValue(text, JToken.ReadFrom(reader));
					break;
				}
				case JsonToken.EndObject:
					flag = true;
					break;
				default:
					throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
				case JsonToken.Comment:
					break;
				}
			}
			while (!flag && reader.Read());
			if (!flag)
			{
//				ThrowUnexpectedEndException(reader, contract, serializationInfo, "Unexpected end when deserializing object.");
			}
			if (contract.ISerializableCreator == null)
			{
				throw JsonSerializationException.Create(reader, "ISerializable type '{0}' does not have a valid constructor. To correctly implement ISerializable a constructor that takes SerializationInfo and StreamingContext parameters should be present.".FormatWith(CultureInfo.InvariantCulture, underlyingType));
			}
//			object obj = contract.ISerializableCreator(serializationInfo, Serializer._context);
//			if (id != null)
//			{
//				AddReference(reader, id, obj);
//			}
//			OnDeserializing(reader, contract, obj);
//			OnDeserialized(reader, contract, obj);
			return null;
		}

		internal object CreateISerializableItem(JToken token, Type type, JsonISerializableContract contract, JsonProperty member)
		{
			JsonContract contractSafe = GetContractSafe(type);
			JsonConverter converter = GetConverter(contractSafe, null, contract, member);
			JsonReader jsonReader = token.CreateReader();
			jsonReader.ReadAndAssert();
			if (converter != null && converter.CanRead)
			{
				return DeserializeConvertable(converter, jsonReader, type, null);
			}
			return CreateValueInternal(jsonReader, type, contractSafe, null, contract, member, null);
		}

		private object CreateObjectUsingCreatorWithParameters(JsonReader reader, JsonObjectContract contract, JsonProperty containerProperty, ObjectConstructor<object> creator, string id)
		{
			ValidationUtils.ArgumentNotNull(creator, "creator");
			bool flag = contract.HasRequiredOrDefaultValueProperties || HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Populate);
			Type underlyingType = contract.UnderlyingType;
			if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
			{
				string arg = string.Join(", ", (from p in contract.CreatorParameters
				select p.PropertyName).ToArray());
				TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using creator with parameters: {1}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType, arg)), null);
			}
			List<CreatorPropertyContext> list = ResolvePropertyAndCreatorValues(contract, containerProperty, reader, underlyingType);
			if (flag)
			{
				foreach (JsonProperty property3 in contract.Properties)
				{
					if (list.All((CreatorPropertyContext p) => p.Property != property3))
					{
						list.Add(new CreatorPropertyContext
						{
							Property = property3,
							Name = property3.PropertyName,
							Presence = PropertyPresence.None
						});
					}
				}
			}
			object[] array = new object[contract.CreatorParameters.Count];
			foreach (CreatorPropertyContext item in list)
			{
				if (flag && item.Property != null && !item.Presence.HasValue)
				{
					object value = item.Value;
					PropertyPresence value2 = (value == null) ? PropertyPresence.Null : ((!(value is string)) ? PropertyPresence.Value : (CoerceEmptyStringToNull(item.Property.PropertyType, item.Property.PropertyContract, (string)value) ? PropertyPresence.Null : PropertyPresence.Value));
					item.Presence = value2;
				}
				JsonProperty jsonProperty = item.ConstructorProperty;
				if (jsonProperty == null && item.Property != null)
				{
					jsonProperty = contract.CreatorParameters.ForgivingCaseSensitiveFind((JsonProperty p) => p.PropertyName, item.Property.UnderlyingName);
				}
				if (jsonProperty != null && !jsonProperty.Ignored)
				{
					if (flag && (item.Presence == PropertyPresence.None || item.Presence == PropertyPresence.Null))
					{
						if (jsonProperty.PropertyContract == null)
						{
							jsonProperty.PropertyContract = GetContractSafe(jsonProperty.PropertyType);
						}
						if (HasFlag(jsonProperty.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate))
						{
							item.Value = EnsureType(reader, jsonProperty.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, jsonProperty.PropertyContract, jsonProperty.PropertyType);
						}
					}
					int num = contract.CreatorParameters.IndexOf(jsonProperty);
					array[num] = item.Value;
					item.Used = true;
				}
			}
			object obj = creator(array);
			if (id != null)
			{
				AddReference(reader, id, obj);
			}
			OnDeserializing(reader, contract, obj);
			foreach (CreatorPropertyContext item2 in list)
			{
				if (!item2.Used && item2.Property != null && !item2.Property.Ignored && item2.Presence != PropertyPresence.None)
				{
					JsonProperty property2 = item2.Property;
					object value3 = item2.Value;
					if (ShouldSetPropertyValue(property2, value3))
					{
						property2.ValueProvider.SetValue(obj, value3);
						item2.Used = true;
					}
					else if (!property2.Writable && value3 != null)
					{
						JsonContract jsonContract = Serializer._contractResolver.ResolveContract(property2.PropertyType);
						if (jsonContract.ContractType == JsonContractType.Array)
						{
							JsonArrayContract jsonArrayContract = (JsonArrayContract)jsonContract;
							object value4 = property2.ValueProvider.GetValue(obj);
							if (value4 != null)
							{
								IWrappedCollection wrappedCollection = jsonArrayContract.CreateWrapper(value4);
								foreach (object item3 in jsonArrayContract.CreateWrapper(value3))
								{
									wrappedCollection.Add(item3);
								}
							}
						}
						else if (jsonContract.ContractType == JsonContractType.Dictionary)
						{
							JsonDictionaryContract jsonDictionaryContract = (JsonDictionaryContract)jsonContract;
							object value5 = property2.ValueProvider.GetValue(obj);
							if (value5 != null)
							{
								object obj2;
								if (!jsonDictionaryContract.ShouldCreateWrapper)
								{
									obj2 = (IDictionary)value5;
								}
								else
								{
									IDictionary dictionary = jsonDictionaryContract.CreateWrapper(value5);
									obj2 = dictionary;
								}
								IDictionary dictionary2 = (IDictionary)obj2;
								object obj3;
								if (!jsonDictionaryContract.ShouldCreateWrapper)
								{
									obj3 = (IDictionary)value3;
								}
								else
								{
									IDictionary dictionary = jsonDictionaryContract.CreateWrapper(value3);
									obj3 = dictionary;
								}
								IDictionaryEnumerator enumerator4 = ((IDictionary)obj3).GetEnumerator();
								try
								{
									while (enumerator4.MoveNext())
									{
										DictionaryEntry entry = enumerator4.Entry;
										dictionary2.Add(entry.Key, entry.Value);
									}
								}
								finally
								{
									IDisposable obj4 = enumerator4 as IDisposable;
									if (obj4 != null)
									{
										obj4.Dispose();
									}
								}
							}
						}
						item2.Used = true;
					}
				}
			}
			if (contract.ExtensionDataSetter != null)
			{
				foreach (CreatorPropertyContext item4 in list)
				{
					if (!item4.Used)
					{
						contract.ExtensionDataSetter(obj, item4.Name, item4.Value);
					}
				}
			}
			if (flag)
			{
				foreach (CreatorPropertyContext item5 in list)
				{
					if (item5.Property != null)
					{
						EndProcessProperty(obj, reader, contract, reader.Depth, item5.Property, item5.Presence.GetValueOrDefault(), !item5.Used);
					}
				}
			}
			OnDeserialized(reader, contract, obj);
			return obj;
		}

		private object DeserializeConvertable(JsonConverter converter, JsonReader reader, Type objectType, object existingValue)
		{
			if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
			{
				TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
			}
			object result = converter.ReadJson(reader, objectType, existingValue, GetInternalSerializer());
			if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
			{
				TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
			}
			return result;
		}

		private List<CreatorPropertyContext> ResolvePropertyAndCreatorValues(JsonObjectContract contract, JsonProperty containerProperty, JsonReader reader, Type objectType)
		{
			List<CreatorPropertyContext> list = new List<CreatorPropertyContext>();
			bool flag = false;
			do
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string text = reader.Value.ToString();
					CreatorPropertyContext creatorPropertyContext = new CreatorPropertyContext
					{
						Name = reader.Value.ToString(),
						ConstructorProperty = contract.CreatorParameters.GetClosestMatchProperty(text),
						Property = contract.Properties.GetClosestMatchProperty(text)
					};
					list.Add(creatorPropertyContext);
					JsonProperty jsonProperty = creatorPropertyContext.ConstructorProperty ?? creatorPropertyContext.Property;
					if (jsonProperty != null && !jsonProperty.Ignored)
					{
						if (jsonProperty.PropertyContract == null)
						{
							jsonProperty.PropertyContract = GetContractSafe(jsonProperty.PropertyType);
						}
						JsonConverter converter = GetConverter(jsonProperty.PropertyContract, jsonProperty.MemberConverter, contract, containerProperty);
						if (!ReadForType(reader, jsonProperty.PropertyContract, converter != null))
						{
							throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, text));
						}
						if (converter != null && converter.CanRead)
						{
							creatorPropertyContext.Value = DeserializeConvertable(converter, reader, jsonProperty.PropertyType, null);
						}
						else
						{
							creatorPropertyContext.Value = CreateValueInternal(reader, jsonProperty.PropertyType, jsonProperty.PropertyContract, jsonProperty, contract, containerProperty, null);
						}
					}
					else
					{
						if (!reader.Read())
						{
							throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, text));
						}
						if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
						{
							TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}.".FormatWith(CultureInfo.InvariantCulture, text, contract.UnderlyingType)), null);
						}
						if (Serializer._missingMemberHandling == MissingMemberHandling.Error)
						{
							throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, text, objectType.Name));
						}
						if (contract.ExtensionDataSetter != null)
						{
							creatorPropertyContext.Value = ReadExtensionDataValue(contract, containerProperty, reader);
						}
						else
						{
							reader.Skip();
						}
					}
					break;
				}
				case JsonToken.EndObject:
					flag = true;
					break;
				default:
					throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
				case JsonToken.Comment:
					break;
				}
			}
			while (!flag && reader.Read());
			return list;
		}

		private bool ReadForType(JsonReader reader, JsonContract contract, bool hasConverter)
		{
			if (hasConverter)
			{
				return reader.Read();
			}
			switch ((contract != null) ? contract.InternalReadType : ReadType.Read)
			{
			case ReadType.Read:
				return reader.ReadAndMoveToContent();
			case ReadType.ReadAsInt32:
				reader.ReadAsInt32();
				break;
			case ReadType.ReadAsDecimal:
				reader.ReadAsDecimal();
				break;
			case ReadType.ReadAsDouble:
				reader.ReadAsDouble();
				break;
			case ReadType.ReadAsBytes:
				reader.ReadAsBytes();
				break;
			case ReadType.ReadAsBoolean:
				reader.ReadAsBoolean();
				break;
			case ReadType.ReadAsString:
				reader.ReadAsString();
				break;
			case ReadType.ReadAsDateTime:
				reader.ReadAsDateTime();
				break;
			case ReadType.ReadAsDateTimeOffset:
				reader.ReadAsDateTimeOffset();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return reader.TokenType != JsonToken.None;
		}

		public object CreateNewObject(JsonReader reader, JsonObjectContract objectContract, JsonProperty containerMember, JsonProperty containerProperty, string id, out bool createdFromNonDefaultCreator)
		{
			object obj = null;
			if (objectContract.OverrideCreator != null)
			{
				if (objectContract.CreatorParameters.Count > 0)
				{
					createdFromNonDefaultCreator = true;
					return CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.OverrideCreator, id);
				}
				obj = objectContract.OverrideCreator();
			}
			else if (objectContract.DefaultCreator != null && (!objectContract.DefaultCreatorNonPublic || Serializer._constructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor || objectContract.ParameterizedCreator == null))
			{
				obj = objectContract.DefaultCreator();
			}
			else if (objectContract.ParameterizedCreator != null)
			{
				createdFromNonDefaultCreator = true;
				return CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.ParameterizedCreator, id);
			}
			if (obj == null)
			{
				if (!objectContract.IsInstantiable)
				{
					throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
				}
				throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
			}
			createdFromNonDefaultCreator = false;
			return obj;
		}

		private object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, JsonProperty member, string id)
		{
			OnDeserializing(reader, contract, newObject);
			Dictionary<JsonProperty, PropertyPresence> dictionary = (contract.HasRequiredOrDefaultValueProperties || HasFlag(Serializer._defaultValueHandling, DefaultValueHandling.Populate)) ? contract.Properties.ToDictionary((JsonProperty m) => m, (JsonProperty m) => PropertyPresence.None) : null;
			if (id != null)
			{
				AddReference(reader, id, newObject);
			}
			int depth = reader.Depth;
			bool flag = false;
			do
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string text = reader.Value.ToString();
					if (!CheckPropertyName(reader, text))
					{
						try
						{
							JsonProperty closestMatchProperty = contract.Properties.GetClosestMatchProperty(text);
							if (closestMatchProperty == null)
							{
								if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
								{
									TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}".FormatWith(CultureInfo.InvariantCulture, text, contract.UnderlyingType)), null);
								}
								if (Serializer._missingMemberHandling == MissingMemberHandling.Error)
								{
									throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, text, contract.UnderlyingType.Name));
								}
								if (reader.Read())
								{
									SetExtensionData(contract, member, reader, text, newObject);
								}
							}
							else if (closestMatchProperty.Ignored || !ShouldDeserialize(reader, closestMatchProperty, newObject))
							{
								if (reader.Read())
								{
									SetPropertyPresence(reader, closestMatchProperty, dictionary);
									SetExtensionData(contract, member, reader, text, newObject);
								}
							}
							else
							{
								if (closestMatchProperty.PropertyContract == null)
								{
									closestMatchProperty.PropertyContract = GetContractSafe(closestMatchProperty.PropertyType);
								}
								JsonConverter converter = GetConverter(closestMatchProperty.PropertyContract, closestMatchProperty.MemberConverter, contract, member);
								if (!ReadForType(reader, closestMatchProperty.PropertyContract, converter != null))
								{
									throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, text));
								}
								SetPropertyPresence(reader, closestMatchProperty, dictionary);
								if (!SetPropertyValue(closestMatchProperty, converter, contract, member, reader, newObject))
								{
									SetExtensionData(contract, member, reader, text, newObject);
								}
							}
						}
						catch (Exception ex)
						{
							if (!IsErrorHandled(newObject, contract, text, reader as IJsonLineInfo, reader.Path, ex))
							{
								throw;
							}
							HandleError(reader, true, depth);
						}
					}
					break;
				}
				case JsonToken.EndObject:
					flag = true;
					break;
				default:
					throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
				case JsonToken.Comment:
					break;
				}
			}
			while (!flag && reader.Read());
			if (!flag)
			{
				ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
			}
			if (dictionary != null)
			{
				foreach (KeyValuePair<JsonProperty, PropertyPresence> item in dictionary)
				{
					JsonProperty key = item.Key;
					PropertyPresence value = item.Value;
					EndProcessProperty(newObject, reader, contract, depth, key, value, true);
				}
			}
			OnDeserialized(reader, contract, newObject);
			return newObject;
		}

		private bool ShouldDeserialize(JsonReader reader, JsonProperty property, object target)
		{
			if (property.ShouldDeserialize == null)
			{
				return true;
			}
			bool flag = property.ShouldDeserialize(target);
			if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
			{
				TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, reader.Path, "ShouldDeserialize result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, flag)), null);
			}
			return flag;
		}

		private bool CheckPropertyName(JsonReader reader, string memberName)
		{
			if (Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead && (memberName == "$id" || memberName == "$ref" || memberName == "$type" || memberName == "$values"))
			{
				reader.Skip();
				return true;
			}
			return false;
		}

		private void SetExtensionData(JsonObjectContract contract, JsonProperty member, JsonReader reader, string memberName, object o)
		{
			if (contract.ExtensionDataSetter != null)
			{
				try
				{
					object value = ReadExtensionDataValue(contract, member, reader);
					contract.ExtensionDataSetter(o, memberName, value);
				}
				catch (Exception ex)
				{
					throw JsonSerializationException.Create(reader, "Error setting value in extension data for type '{0}'.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType), ex);
				}
			}
			else
			{
				reader.Skip();
			}
		}

		private object ReadExtensionDataValue(JsonObjectContract contract, JsonProperty member, JsonReader reader)
		{
			if (contract.ExtensionDataIsJToken)
			{
				return JToken.ReadFrom(reader);
			}
			return CreateValueInternal(reader, null, null, null, contract, member, null);
		}

		private void EndProcessProperty(object newObject, JsonReader reader, JsonObjectContract contract, int initialDepth, JsonProperty property, PropertyPresence presence, bool setDefaultValue)
		{
			if (presence == PropertyPresence.None || presence == PropertyPresence.Null)
			{
				try
				{
					Required required = property._required ?? contract.ItemRequired ?? Required.Default;
					switch (presence)
					{
					case PropertyPresence.None:
						if (required == Required.AllowNull || required == Required.Always)
						{
							throw JsonSerializationException.Create(reader, "Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
						}
						if (setDefaultValue && !property.Ignored)
						{
							if (property.PropertyContract == null)
							{
								property.PropertyContract = GetContractSafe(property.PropertyType);
							}
							if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer._defaultValueHandling), DefaultValueHandling.Populate) && property.Writable)
							{
								property.ValueProvider.SetValue(newObject, EnsureType(reader, property.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, property.PropertyContract, property.PropertyType));
							}
						}
						break;
					case PropertyPresence.Null:
						switch (required)
						{
						case Required.Always:
							throw JsonSerializationException.Create(reader, "Required property '{0}' expects a value but got null.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
						case Required.DisallowNull:
							throw JsonSerializationException.Create(reader, "Required property '{0}' expects a non-null value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
						}
						break;
					}
				}
				catch (Exception ex)
				{
					if (!IsErrorHandled(newObject, contract, property.PropertyName, reader as IJsonLineInfo, reader.Path, ex))
					{
						throw;
					}
					HandleError(reader, true, initialDepth);
				}
			}
		}

		private void SetPropertyPresence(JsonReader reader, JsonProperty property, Dictionary<JsonProperty, PropertyPresence> requiredProperties)
		{
			if (property != null && requiredProperties != null)
			{
				PropertyPresence value;
				switch (reader.TokenType)
				{
				case JsonToken.String:
					value = (CoerceEmptyStringToNull(property.PropertyType, property.PropertyContract, (string)reader.Value) ? PropertyPresence.Null : PropertyPresence.Value);
					break;
				case JsonToken.Null:
				case JsonToken.Undefined:
					value = PropertyPresence.Null;
					break;
				default:
					value = PropertyPresence.Value;
					break;
				}
				requiredProperties[property] = value;
			}
		}

		private void HandleError(JsonReader reader, bool readPastError, int initialDepth)
		{
			ClearErrorContext();
			if (readPastError)
			{
				reader.Skip();
				while (reader.Depth > initialDepth + 1 && reader.Read())
				{
				}
			}
		}
	}
}
