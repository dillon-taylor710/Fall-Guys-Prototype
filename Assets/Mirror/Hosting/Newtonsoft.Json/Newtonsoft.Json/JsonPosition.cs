using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Newtonsoft.Json
{
	[Preserve]
	internal struct JsonPosition
	{
		private static readonly char[] SpecialCharacters = new char[6]
		{
			'.',
			' ',
			'[',
			']',
			'(',
			')'
		};

		internal JsonContainerType Type;

		internal int Position;

		internal string PropertyName;

		internal bool HasIndex;

		public JsonPosition(JsonContainerType type)
		{
			Type = type;
			HasIndex = TypeHasIndex(type);
			Position = -1;
			PropertyName = null;
		}

		internal int CalculateLength()
		{
			switch (Type)
			{
			case JsonContainerType.Object:
				return PropertyName.Length + 5;
			case JsonContainerType.Array:
			case JsonContainerType.Constructor:
				return MathUtils.IntLength((ulong)Position) + 2;
			default:
				throw new ArgumentOutOfRangeException("Type");
			}
		}

		internal void WriteTo(StringBuilder sb)
		{
			switch (Type)
			{
			case JsonContainerType.Object:
			{
				string propertyName = PropertyName;
				if (propertyName.IndexOfAny(SpecialCharacters) != -1)
				{
					sb.Append("['");
					sb.Append(propertyName);
					sb.Append("']");
				}
				else
				{
					if (sb.Length > 0)
					{
						sb.Append('.');
					}
					sb.Append(propertyName);
				}
				break;
			}
			case JsonContainerType.Array:
			case JsonContainerType.Constructor:
				sb.Append('[');
				sb.Append(Position);
				sb.Append(']');
				break;
			}
		}

		internal static bool TypeHasIndex(JsonContainerType type)
		{
			if (type != JsonContainerType.Array)
			{
				return type == JsonContainerType.Constructor;
			}
			return true;
		}

		internal static string BuildPath(List<JsonPosition> positions, JsonPosition? currentPosition)
		{
			int num = 0;
			if (positions != null)
			{
				for (int i = 0; i < positions.Count; i++)
				{
					num += positions[i].CalculateLength();
				}
			}
			if (currentPosition.HasValue)
			{
				num += currentPosition.GetValueOrDefault().CalculateLength();
			}
			StringBuilder stringBuilder = new StringBuilder(num);
			if (positions != null)
			{
				foreach (JsonPosition position in positions)
				{
					position.WriteTo(stringBuilder);
				}
			}
			if (currentPosition.HasValue)
			{
				currentPosition.GetValueOrDefault().WriteTo(stringBuilder);
			}
			return stringBuilder.ToString();
		}

		internal static string FormatMessage(IJsonLineInfo lineInfo, string path, string message)
		{
			if (!message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
			{
				message = message.Trim();
				if (!message.EndsWith('.'))
				{
					message += ".";
				}
				message += " ";
			}
			message += "Path '{0}'".FormatWith(CultureInfo.InvariantCulture, path);
			if (lineInfo != null && lineInfo.HasLineInfo())
			{
				message += ", line {0}, position {1}".FormatWith(CultureInfo.InvariantCulture, lineInfo.LineNumber, lineInfo.LinePosition);
			}
			message += ".";
			return message;
		}
	}
}
