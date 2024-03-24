using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;

namespace Newtonsoft.Json.Linq
{
	[Preserve]
	public class JTokenReader : JsonReader, IJsonLineInfo
	{
		private readonly JToken _root;

		private string _initialPath;

		private JToken _parent;

		private JToken _current;

		public JToken CurrentToken
		{
			get
			{
				return _current;
			}
		}

		int IJsonLineInfo.LineNumber
		{
			get
			{
				if (base.CurrentState == State.Start)
				{
					return 0;
				}
				IJsonLineInfo current = _current;
				if (current != null)
				{
					return current.LineNumber;
				}
				return 0;
			}
		}

		int IJsonLineInfo.LinePosition
		{
			get
			{
				if (base.CurrentState == State.Start)
				{
					return 0;
				}
				IJsonLineInfo current = _current;
				if (current != null)
				{
					return current.LinePosition;
				}
				return 0;
			}
		}

		public override string Path
		{
			get
			{
				string text = base.Path;
				if (_initialPath == null)
				{
					_initialPath = _root.Path;
				}
				if (!string.IsNullOrEmpty(_initialPath))
				{
					if (string.IsNullOrEmpty(text))
					{
						return _initialPath;
					}
					text = ((!text.StartsWith('[')) ? (_initialPath + "." + text) : (_initialPath + text));
				}
				return text;
			}
		}

		public JTokenReader(JToken token)
		{
			ValidationUtils.ArgumentNotNull(token, "token");
			_root = token;
		}

		public override bool Read()
		{
			if (base.CurrentState != 0)
			{
				if (_current == null)
				{
					return false;
				}
				JContainer jContainer = _current as JContainer;
				if (jContainer != null && _parent != jContainer)
				{
					return ReadInto(jContainer);
				}
				return ReadOver(_current);
			}
			_current = _root;
			SetToken(_current);
			return true;
		}

		private bool ReadOver(JToken t)
		{
			if (t == _root)
			{
				return ReadToEnd();
			}
			JToken next = t.Next;
			if (next == null || next == t || t == t.Parent.Last)
			{
				if (t.Parent == null)
				{
					return ReadToEnd();
				}
				return SetEnd(t.Parent);
			}
			_current = next;
			SetToken(_current);
			return true;
		}

		private bool ReadToEnd()
		{
			_current = null;
			SetToken(JsonToken.None);
			return false;
		}

		private JsonToken? GetEndToken(JContainer c)
		{
			switch (c.Type)
			{
			case JTokenType.Object:
				return JsonToken.EndObject;
			case JTokenType.Array:
				return JsonToken.EndArray;
			case JTokenType.Constructor:
				return JsonToken.EndConstructor;
			case JTokenType.Property:
				return null;
			default:
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", c.Type, "Unexpected JContainer type.");
			}
		}

		private bool ReadInto(JContainer c)
		{
			JToken first = c.First;
			if (first == null)
			{
				return SetEnd(c);
			}
			SetToken(first);
			_current = first;
			_parent = c;
			return true;
		}

		private bool SetEnd(JContainer c)
		{
			JsonToken? endToken = GetEndToken(c);
			if (endToken.HasValue)
			{
				SetToken(endToken.GetValueOrDefault());
				_current = c;
				_parent = c;
				return true;
			}
			return ReadOver(c);
		}

		private void SetToken(JToken token)
		{
			switch (token.Type)
			{
			case JTokenType.Object:
				SetToken(JsonToken.StartObject);
				break;
			case JTokenType.Array:
				SetToken(JsonToken.StartArray);
				break;
			case JTokenType.Constructor:
				SetToken(JsonToken.StartConstructor, ((JConstructor)token).Name);
				break;
			case JTokenType.Property:
				SetToken(JsonToken.PropertyName, ((JProperty)token).Name);
				break;
			case JTokenType.Comment:
				SetToken(JsonToken.Comment, ((JValue)token).Value);
				break;
			case JTokenType.Integer:
				SetToken(JsonToken.Integer, ((JValue)token).Value);
				break;
			case JTokenType.Float:
				SetToken(JsonToken.Float, ((JValue)token).Value);
				break;
			case JTokenType.String:
				SetToken(JsonToken.String, ((JValue)token).Value);
				break;
			case JTokenType.Boolean:
				SetToken(JsonToken.Boolean, ((JValue)token).Value);
				break;
			case JTokenType.Null:
				SetToken(JsonToken.Null, ((JValue)token).Value);
				break;
			case JTokenType.Undefined:
				SetToken(JsonToken.Undefined, ((JValue)token).Value);
				break;
			case JTokenType.Date:
				SetToken(JsonToken.Date, ((JValue)token).Value);
				break;
			case JTokenType.Raw:
				SetToken(JsonToken.Raw, ((JValue)token).Value);
				break;
			case JTokenType.Bytes:
				SetToken(JsonToken.Bytes, ((JValue)token).Value);
				break;
			case JTokenType.Guid:
				SetToken(JsonToken.String, SafeToString(((JValue)token).Value));
				break;
			case JTokenType.Uri:
			{
				object value = ((JValue)token).Value;
				if (value is Uri)
				{
					SetToken(JsonToken.String, ((Uri)value).OriginalString);
				}
				else
				{
					SetToken(JsonToken.String, SafeToString(value));
				}
				break;
			}
			case JTokenType.TimeSpan:
				SetToken(JsonToken.String, SafeToString(((JValue)token).Value));
				break;
			default:
				throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", token.Type, "Unexpected JTokenType.");
			}
		}

		private string SafeToString(object value)
		{
			if (value == null)
			{
				return null;
			}
			return value.ToString();
		}

		bool IJsonLineInfo.HasLineInfo()
		{
			if (base.CurrentState == State.Start)
			{
				return false;
			}
			IJsonLineInfo current = _current;
			if (current != null)
			{
				return current.HasLineInfo();
			}
			return false;
		}
	}
}
