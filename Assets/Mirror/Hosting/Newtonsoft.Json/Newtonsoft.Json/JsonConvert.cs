using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Newtonsoft.Json
{
	[Preserve]
	public static class JsonConvert
	{
		public static readonly string True;

		public static readonly string False;

		public static readonly string Null;

		public static readonly string Undefined;

		public static readonly string PositiveInfinity;

		public static readonly string NegativeInfinity;

		public static readonly string NaN;

		private static readonly JsonSerializerSettings InitialSerializerSettings;

		public static Func<JsonSerializerSettings> DefaultSettings
		{
			get;
			set;
		}

		static JsonConvert()
		{
			True = "true";
			False = "false";
			Null = "null";
			Undefined = "undefined";
			PositiveInfinity = "Infinity";
			NegativeInfinity = "-Infinity";
			NaN = "NaN";
			InitialSerializerSettings = new JsonSerializerSettings();
			DefaultSettings = GetDefaultSettings;
		}

		internal static JsonSerializerSettings GetDefaultSettings()
		{
			return InitialSerializerSettings;
		}

		public static string ToString(bool value)
		{
			if (!value)
			{
				return False;
			}
			return True;
		}

		public static string ToString(char value)
		{
			return ToString(char.ToString(value));
		}

		internal static string ToString(float value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
		{
			return EnsureFloatFormat((double)value, EnsureDecimalPlace((double)value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
		}

		private static string EnsureFloatFormat(double value, string text, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
		{
			if (floatFormatHandling == FloatFormatHandling.Symbol || (!double.IsInfinity(value) && !double.IsNaN(value)))
			{
				return text;
			}
			if (floatFormatHandling == FloatFormatHandling.DefaultValue)
			{
				if (nullable)
				{
					return Null;
				}
				return "0.0";
			}
			return quoteChar.ToString() + text + quoteChar.ToString();
		}

		internal static string ToString(double value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
		{
			return EnsureFloatFormat(value, EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
		}

		private static string EnsureDecimalPlace(double value, string text)
		{
			if (double.IsNaN(value) || double.IsInfinity(value) || text.IndexOf('.') != -1 || text.IndexOf('E') != -1 || text.IndexOf('e') != -1)
			{
				return text;
			}
			return text + ".0";
		}

		private static string EnsureDecimalPlace(string text)
		{
			if (text.IndexOf('.') != -1)
			{
				return text;
			}
			return text + ".0";
		}

		public static string ToString(decimal value)
		{
			return EnsureDecimalPlace(value.ToString(null, CultureInfo.InvariantCulture));
		}

		public static string ToString(string value)
		{
			return ToString(value, '"');
		}

		public static string ToString(string value, char delimiter)
		{
			return ToString(value, delimiter, StringEscapeHandling.Default);
		}

		public static string ToString(string value, char delimiter, StringEscapeHandling stringEscapeHandling)
		{
			if (delimiter != '"' && delimiter != '\'')
			{
				throw new ArgumentException("Delimiter must be a single or double quote.", "delimiter");
			}
			return JavaScriptUtils.ToEscapedJavaScriptString(value, delimiter, true, stringEscapeHandling);
		}

		public static string SerializeObject(object value)
		{
			return SerializeObject(value, null, null);
		}

		public static string SerializeObject(object value, Formatting formatting)
		{
			return SerializeObject(value, formatting, null);
		}

		public static string SerializeObject(object value, Type type, JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
			return SerializeObjectInternal(value, type, jsonSerializer);
		}

		public static string SerializeObject(object value, Formatting formatting, JsonSerializerSettings settings)
		{
			return SerializeObject(value, null, formatting, settings);
		}

		public static string SerializeObject(object value, Type type, Formatting formatting, JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
			jsonSerializer.Formatting = formatting;
			return SerializeObjectInternal(value, type, jsonSerializer);
		}

		private static string SerializeObjectInternal(object value, Type type, JsonSerializer jsonSerializer)
		{
			StringWriter stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
			using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
			{
				jsonTextWriter.Formatting = jsonSerializer.Formatting;
				jsonSerializer.Serialize(jsonTextWriter, value, type);
			}
			return stringWriter.ToString();
		}

		public static T DeserializeObject<T>(string value)
		{
			return DeserializeObject<T>(value, null);
		}

		public static T DeserializeObject<T>(string value, JsonSerializerSettings settings)
		{
			return (T)DeserializeObject(value, typeof(T), settings);
		}

		public static object DeserializeObject(string value, Type type, JsonSerializerSettings settings)
		{
			ValidationUtils.ArgumentNotNull(value, "value");
			JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
			if (!jsonSerializer.IsCheckAdditionalContentSet())
			{
				jsonSerializer.CheckAdditionalContent = true;
			}
			using (JsonTextReader reader = new JsonTextReader(new StringReader(value)))
			{
				return jsonSerializer.Deserialize(reader, type);
			}
		}
	}
}
