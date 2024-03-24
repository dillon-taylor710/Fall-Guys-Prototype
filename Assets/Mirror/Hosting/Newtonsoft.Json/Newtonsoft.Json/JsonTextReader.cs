using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Globalization;
using System.IO;

namespace Newtonsoft.Json
{
	[Preserve]
	public class JsonTextReader : JsonReader, IJsonLineInfo
	{
		private const char UnicodeReplacementChar = '�';

		private const int MaximumJavascriptIntegerCharacterLength = 380;

		private readonly TextReader _reader;

		private char[] _chars;

		private int _charsUsed;

		private int _charPos;

		private int _lineStartPos;

		private int _lineNumber;

		private bool _isEndOfFile;

		private StringBuffer _stringBuffer;

		private StringReference _stringReference;

		private IArrayPool<char> _arrayPool;

		internal PropertyNameTable NameTable;

		public int LineNumber
		{
			get
			{
				if (base.CurrentState == State.Start && LinePosition == 0 && TokenType != JsonToken.Comment)
				{
					return 0;
				}
				return _lineNumber;
			}
		}

		public int LinePosition
		{
			get
			{
				return _charPos - _lineStartPos;
			}
		}

		public JsonTextReader(TextReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			_reader = reader;
			_lineNumber = 1;
		}

		private void EnsureBufferNotEmpty()
		{
			if (_stringBuffer.IsEmpty)
			{
				_stringBuffer = new StringBuffer(_arrayPool, 1024);
			}
		}

		private void OnNewLine(int pos)
		{
			_lineNumber++;
			_lineStartPos = pos;
		}

		private void ParseString(char quote, ReadType readType)
		{
			_charPos++;
			ShiftBufferIfNeeded();
			ReadStringIntoBuffer(quote);
			SetPostValueState(true);
			switch (readType)
			{
			case ReadType.ReadAsInt32:
			case ReadType.ReadAsDecimal:
			case ReadType.ReadAsBoolean:
				break;
			case ReadType.ReadAsBytes:
			{
				Guid g;
				byte[] value2 = (_stringReference.Length == 0) ? new byte[0] : ((_stringReference.Length != 36 || !ConvertUtils.TryConvertGuid(_stringReference.ToString(), out g)) ? Convert.FromBase64CharArray(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length) : g.ToByteArray());
				SetToken(JsonToken.Bytes, value2, false);
				break;
			}
			case ReadType.ReadAsString:
			{
				string value = _stringReference.ToString();
				SetToken(JsonToken.String, value, false);
				_quoteChar = quote;
				break;
			}
			default:
				if (_dateParseHandling != 0)
				{
					DateParseHandling dateParseHandling;
					switch (readType)
					{
					case ReadType.ReadAsDateTime:
						dateParseHandling = DateParseHandling.DateTime;
						break;
					case ReadType.ReadAsDateTimeOffset:
						dateParseHandling = DateParseHandling.DateTimeOffset;
						break;
					default:
						dateParseHandling = _dateParseHandling;
						break;
					}
					DateTimeOffset dt2;
					if (dateParseHandling == DateParseHandling.DateTime)
					{
						DateTime dt;
						if (DateTimeUtils.TryParseDateTime(_stringReference, base.DateTimeZoneHandling, base.DateFormatString, base.Culture, out dt))
						{
							SetToken(JsonToken.Date, dt, false);
							break;
						}
					}
					else if (DateTimeUtils.TryParseDateTimeOffset(_stringReference, base.DateFormatString, base.Culture, out dt2))
					{
						SetToken(JsonToken.Date, dt2, false);
						break;
					}
				}
				SetToken(JsonToken.String, _stringReference.ToString(), false);
				_quoteChar = quote;
				break;
			}
		}

		private static void BlockCopyChars(char[] src, int srcOffset, char[] dst, int dstOffset, int count)
		{
			Buffer.BlockCopy(src, srcOffset * 2, dst, dstOffset * 2, count * 2);
		}

		private void ShiftBufferIfNeeded()
		{
			int num = _chars.Length;
			if ((double)(num - _charPos) <= (double)num * 0.1)
			{
				int num2 = _charsUsed - _charPos;
				if (num2 > 0)
				{
					BlockCopyChars(_chars, _charPos, _chars, 0, num2);
				}
				_lineStartPos -= _charPos;
				_charPos = 0;
				_charsUsed = num2;
				_chars[_charsUsed] = '\0';
			}
		}

		private int ReadData(bool append)
		{
			return ReadData(append, 0);
		}

		private int ReadData(bool append, int charsRequired)
		{
			if (_isEndOfFile)
			{
				return 0;
			}
			if (_charsUsed + charsRequired >= _chars.Length - 1)
			{
				if (append)
				{
					int minSize = Math.Max(_chars.Length * 2, _charsUsed + charsRequired + 1);
					char[] array = BufferUtils.RentBuffer(_arrayPool, minSize);
					BlockCopyChars(_chars, 0, array, 0, _chars.Length);
					BufferUtils.ReturnBuffer(_arrayPool, _chars);
					_chars = array;
				}
				else
				{
					int num = _charsUsed - _charPos;
					if (num + charsRequired + 1 >= _chars.Length)
					{
						char[] array2 = BufferUtils.RentBuffer(_arrayPool, num + charsRequired + 1);
						if (num > 0)
						{
							BlockCopyChars(_chars, _charPos, array2, 0, num);
						}
						BufferUtils.ReturnBuffer(_arrayPool, _chars);
						_chars = array2;
					}
					else if (num > 0)
					{
						BlockCopyChars(_chars, _charPos, _chars, 0, num);
					}
					_lineStartPos -= _charPos;
					_charPos = 0;
					_charsUsed = num;
				}
			}
			int count = _chars.Length - _charsUsed - 1;
			int num2 = _reader.Read(_chars, _charsUsed, count);
			_charsUsed += num2;
			if (num2 == 0)
			{
				_isEndOfFile = true;
			}
			_chars[_charsUsed] = '\0';
			return num2;
		}

		private bool EnsureChars(int relativePosition, bool append)
		{
			if (_charPos + relativePosition >= _charsUsed)
			{
				return ReadChars(relativePosition, append);
			}
			return true;
		}

		private bool ReadChars(int relativePosition, bool append)
		{
			if (_isEndOfFile)
			{
				return false;
			}
			int num = _charPos + relativePosition - _charsUsed + 1;
			int num2 = 0;
			do
			{
				int num3 = ReadData(append, num - num2);
				if (num3 == 0)
				{
					break;
				}
				num2 += num3;
			}
			while (num2 < num);
			if (num2 < num)
			{
				return false;
			}
			return true;
		}

		public override bool Read()
		{
			EnsureBuffer();
			do
			{
				switch (_currentState)
				{
				case State.Start:
				case State.Property:
				case State.ArrayStart:
				case State.Array:
				case State.ConstructorStart:
				case State.Constructor:
					return ParseValue();
				case State.ObjectStart:
				case State.Object:
					return ParseObject();
				case State.PostValue:
					break;
				case State.Finished:
					if (EnsureChars(0, false))
					{
						EatWhitespace(false);
						if (_isEndOfFile)
						{
							SetToken(JsonToken.None);
							return false;
						}
						if (_chars[_charPos] == '/')
						{
							ParseComment(true);
							return true;
						}
						throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
					}
					SetToken(JsonToken.None);
					return false;
				default:
					throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
				}
			}
			while (!ParsePostValue());
			return true;
		}

		public override int? ReadAsInt32()
		{
			return (int?)ReadNumberValue(ReadType.ReadAsInt32);
		}

		public override DateTime? ReadAsDateTime()
		{
			return (DateTime?)ReadStringValue(ReadType.ReadAsDateTime);
		}

		public override string ReadAsString()
		{
			return (string)ReadStringValue(ReadType.ReadAsString);
		}

		public override byte[] ReadAsBytes()
		{
			EnsureBuffer();
			bool flag = false;
			switch (_currentState)
			{
			case State.Start:
			case State.Property:
			case State.ArrayStart:
			case State.Array:
			case State.PostValue:
			case State.ConstructorStart:
			case State.Constructor:
				while (true)
				{
					char c = _chars[_charPos];
					switch (c)
					{
					case '\0':
						if (ReadNullChar())
						{
							SetToken(JsonToken.None, null, false);
							return null;
						}
						break;
					case '"':
					case '\'':
					{
						ParseString(c, ReadType.ReadAsBytes);
						byte[] array = (byte[])Value;
						if (flag)
						{
							ReaderReadAndAssert();
							if (TokenType != JsonToken.EndObject)
							{
								throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
							}
							SetToken(JsonToken.Bytes, array, false);
						}
						return array;
					}
					case '{':
						_charPos++;
						SetToken(JsonToken.StartObject);
						ReadIntoWrappedTypeObject();
						flag = true;
						break;
					case '[':
						_charPos++;
						SetToken(JsonToken.StartArray);
						return ReadArrayIntoByteArray();
					case 'n':
						HandleNull();
						return null;
					case '/':
						ParseComment(false);
						break;
					case ',':
						ProcessValueComma();
						break;
					case ']':
						_charPos++;
						if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
						{
							SetToken(JsonToken.EndArray);
							return null;
						}
						throw CreateUnexpectedCharacterException(c);
					case '\r':
						ProcessCarriageReturn(false);
						break;
					case '\n':
						ProcessLineFeed();
						break;
					case '\t':
					case ' ':
						_charPos++;
						break;
					default:
						_charPos++;
						if (!char.IsWhiteSpace(c))
						{
							throw CreateUnexpectedCharacterException(c);
						}
						break;
					}
				}
			case State.Finished:
				ReadFinished();
				return null;
			default:
				throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
			}
		}

		private object ReadStringValue(ReadType readType)
		{
			EnsureBuffer();
			switch (_currentState)
			{
			case State.Start:
			case State.Property:
			case State.ArrayStart:
			case State.Array:
			case State.PostValue:
			case State.ConstructorStart:
			case State.Constructor:
				while (true)
				{
					char c = _chars[_charPos];
					switch (c)
					{
					case '\0':
						if (ReadNullChar())
						{
							SetToken(JsonToken.None, null, false);
							return null;
						}
						break;
					case '"':
					case '\'':
						ParseString(c, readType);
						switch (readType)
						{
						case ReadType.ReadAsBytes:
							return Value;
						case ReadType.ReadAsString:
							return Value;
						case ReadType.ReadAsDateTime:
							if (Value is DateTime)
							{
								return (DateTime)Value;
							}
							return ReadDateTimeString((string)Value);
						case ReadType.ReadAsDateTimeOffset:
							if (Value is DateTimeOffset)
							{
								return (DateTimeOffset)Value;
							}
							return ReadDateTimeOffsetString((string)Value);
						default:
							throw new ArgumentOutOfRangeException("readType");
						}
					case '-':
						if (EnsureChars(1, true) && _chars[_charPos + 1] == 'I')
						{
							return ParseNumberNegativeInfinity(readType);
						}
						ParseNumber(readType);
						return Value;
					case '.':
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						if (readType != ReadType.ReadAsString)
						{
							_charPos++;
							throw CreateUnexpectedCharacterException(c);
						}
						ParseNumber(ReadType.ReadAsString);
						return Value;
					case 'f':
					case 't':
					{
						if (readType != ReadType.ReadAsString)
						{
							_charPos++;
							throw CreateUnexpectedCharacterException(c);
						}
						string text = (c == 't') ? JsonConvert.True : JsonConvert.False;
						if (!MatchValueWithTrailingSeparator(text))
						{
							throw CreateUnexpectedCharacterException(_chars[_charPos]);
						}
						SetToken(JsonToken.String, text);
						return text;
					}
					case 'I':
						return ParseNumberPositiveInfinity(readType);
					case 'N':
						return ParseNumberNaN(readType);
					case 'n':
						HandleNull();
						return null;
					case '/':
						ParseComment(false);
						break;
					case ',':
						ProcessValueComma();
						break;
					case ']':
						_charPos++;
						if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
						{
							SetToken(JsonToken.EndArray);
							return null;
						}
						throw CreateUnexpectedCharacterException(c);
					case '\r':
						ProcessCarriageReturn(false);
						break;
					case '\n':
						ProcessLineFeed();
						break;
					case '\t':
					case ' ':
						_charPos++;
						break;
					default:
						_charPos++;
						if (!char.IsWhiteSpace(c))
						{
							throw CreateUnexpectedCharacterException(c);
						}
						break;
					}
				}
			case State.Finished:
				ReadFinished();
				return null;
			default:
				throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
			}
		}

		private JsonReaderException CreateUnexpectedCharacterException(char c)
		{
			return JsonReaderException.Create(this, "Unexpected character encountered while parsing value: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
		}

		public override bool? ReadAsBoolean()
		{
			EnsureBuffer();
			switch (_currentState)
			{
			case State.Start:
			case State.Property:
			case State.ArrayStart:
			case State.Array:
			case State.PostValue:
			case State.ConstructorStart:
			case State.Constructor:
				while (true)
				{
					char c = _chars[_charPos];
					switch (c)
					{
					case '\0':
						if (ReadNullChar())
						{
							SetToken(JsonToken.None, null, false);
							return null;
						}
						break;
					case '"':
					case '\'':
						ParseString(c, ReadType.Read);
						return ReadBooleanString(_stringReference.ToString());
					case 'n':
						HandleNull();
						return null;
					case '-':
					case '.':
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					{
						ParseNumber(ReadType.Read);
						bool flag2 = Convert.ToBoolean(Value, CultureInfo.InvariantCulture);
						SetToken(JsonToken.Boolean, flag2, false);
						return flag2;
					}
					case 'f':
					case 't':
					{
						bool flag = c == 't';
						string value = flag ? JsonConvert.True : JsonConvert.False;
						if (!MatchValueWithTrailingSeparator(value))
						{
							throw CreateUnexpectedCharacterException(_chars[_charPos]);
						}
						SetToken(JsonToken.Boolean, flag);
						return flag;
					}
					case '/':
						ParseComment(false);
						break;
					case ',':
						ProcessValueComma();
						break;
					case ']':
						_charPos++;
						if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
						{
							SetToken(JsonToken.EndArray);
							return null;
						}
						throw CreateUnexpectedCharacterException(c);
					case '\r':
						ProcessCarriageReturn(false);
						break;
					case '\n':
						ProcessLineFeed();
						break;
					case '\t':
					case ' ':
						_charPos++;
						break;
					default:
						_charPos++;
						if (!char.IsWhiteSpace(c))
						{
							throw CreateUnexpectedCharacterException(c);
						}
						break;
					}
				}
			case State.Finished:
				ReadFinished();
				return null;
			default:
				throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
			}
		}

		private void ProcessValueComma()
		{
			_charPos++;
			if (_currentState != State.PostValue)
			{
				SetToken(JsonToken.Undefined);
				throw CreateUnexpectedCharacterException(',');
			}
			SetStateBasedOnCurrent();
		}

		private object ReadNumberValue(ReadType readType)
		{
			EnsureBuffer();
			switch (_currentState)
			{
			case State.Start:
			case State.Property:
			case State.ArrayStart:
			case State.Array:
			case State.PostValue:
			case State.ConstructorStart:
			case State.Constructor:
				while (true)
				{
					char c = _chars[_charPos];
					switch (c)
					{
					case '\0':
						if (ReadNullChar())
						{
							SetToken(JsonToken.None, null, false);
							return null;
						}
						break;
					case '"':
					case '\'':
						ParseString(c, readType);
						switch (readType)
						{
						case ReadType.ReadAsInt32:
							return ReadInt32String(_stringReference.ToString());
						case ReadType.ReadAsDecimal:
							return ReadDecimalString(_stringReference.ToString());
						case ReadType.ReadAsDouble:
							return ReadDoubleString(_stringReference.ToString());
						default:
							throw new ArgumentOutOfRangeException("readType");
						}
					case 'n':
						HandleNull();
						return null;
					case 'N':
						return ParseNumberNaN(readType);
					case 'I':
						return ParseNumberPositiveInfinity(readType);
					case '-':
						if (EnsureChars(1, true) && _chars[_charPos + 1] == 'I')
						{
							return ParseNumberNegativeInfinity(readType);
						}
						ParseNumber(readType);
						return Value;
					case '.':
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						ParseNumber(readType);
						return Value;
					case '/':
						ParseComment(false);
						break;
					case ',':
						ProcessValueComma();
						break;
					case ']':
						_charPos++;
						if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
						{
							SetToken(JsonToken.EndArray);
							return null;
						}
						throw CreateUnexpectedCharacterException(c);
					case '\r':
						ProcessCarriageReturn(false);
						break;
					case '\n':
						ProcessLineFeed();
						break;
					case '\t':
					case ' ':
						_charPos++;
						break;
					default:
						_charPos++;
						if (!char.IsWhiteSpace(c))
						{
							throw CreateUnexpectedCharacterException(c);
						}
						break;
					}
				}
			case State.Finished:
				ReadFinished();
				return null;
			default:
				throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
			}
		}

		public override DateTimeOffset? ReadAsDateTimeOffset()
		{
			return (DateTimeOffset?)ReadStringValue(ReadType.ReadAsDateTimeOffset);
		}

		public override decimal? ReadAsDecimal()
		{
			return (decimal?)ReadNumberValue(ReadType.ReadAsDecimal);
		}

		public override double? ReadAsDouble()
		{
			return (double?)ReadNumberValue(ReadType.ReadAsDouble);
		}

		private void HandleNull()
		{
			if (EnsureChars(1, true))
			{
				if (_chars[_charPos + 1] == 'u')
				{
					ParseNull();
					return;
				}
				_charPos += 2;
				throw CreateUnexpectedCharacterException(_chars[_charPos - 1]);
			}
			_charPos = _charsUsed;
			throw CreateUnexpectedEndException();
		}

		private void ReadFinished()
		{
			if (EnsureChars(0, false))
			{
				EatWhitespace(false);
				if (_isEndOfFile)
				{
					return;
				}
				if (_chars[_charPos] != '/')
				{
					throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
				}
				ParseComment(false);
			}
			SetToken(JsonToken.None);
		}

		private bool ReadNullChar()
		{
			if (_charsUsed == _charPos)
			{
				if (ReadData(false) == 0)
				{
					_isEndOfFile = true;
					return true;
				}
			}
			else
			{
				_charPos++;
			}
			return false;
		}

		private void EnsureBuffer()
		{
			if (_chars == null)
			{
				_chars = BufferUtils.RentBuffer(_arrayPool, 1024);
				_chars[0] = '\0';
			}
		}

		private void ReadStringIntoBuffer(char quote)
		{
			int num = _charPos;
			int charPos = _charPos;
			int num2 = _charPos;
			_stringBuffer.Position = 0;
			while (true)
			{
				switch (_chars[num++])
				{
				case '\0':
					if (_charsUsed == num - 1)
					{
						num--;
						if (ReadData(true) == 0)
						{
							_charPos = num;
							throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
						}
					}
					break;
				case '\\':
				{
					_charPos = num;
					if (!EnsureChars(0, true))
					{
						throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
					}
					int writeToPosition = num - 1;
					char c = _chars[num];
					num++;
					char c2;
					switch (c)
					{
					case 'b':
						c2 = '\b';
						break;
					case 't':
						c2 = '\t';
						break;
					case 'n':
						c2 = '\n';
						break;
					case 'f':
						c2 = '\f';
						break;
					case 'r':
						c2 = '\r';
						break;
					case '\\':
						c2 = '\\';
						break;
					case '"':
					case '\'':
					case '/':
						c2 = c;
						break;
					case 'u':
						_charPos = num;
						c2 = ParseUnicode();
						if (StringUtils.IsLowSurrogate(c2))
						{
							c2 = '�';
						}
						else if (StringUtils.IsHighSurrogate(c2))
						{
							bool flag;
							do
							{
								flag = false;
								if (EnsureChars(2, true) && _chars[_charPos] == '\\' && _chars[_charPos + 1] == 'u')
								{
									char writeChar = c2;
									_charPos += 2;
									c2 = ParseUnicode();
									if (!StringUtils.IsLowSurrogate(c2))
									{
										if (StringUtils.IsHighSurrogate(c2))
										{
											writeChar = '�';
											flag = true;
										}
										else
										{
											writeChar = '�';
										}
									}
									EnsureBufferNotEmpty();
									WriteCharToBuffer(writeChar, num2, writeToPosition);
									num2 = _charPos;
								}
								else
								{
									c2 = '�';
								}
							}
							while (flag);
						}
						num = _charPos;
						break;
					default:
						_charPos = num;
						throw JsonReaderException.Create(this, "Bad JSON escape sequence: {0}.".FormatWith(CultureInfo.InvariantCulture, "\\" + c.ToString()));
					}
					EnsureBufferNotEmpty();
					WriteCharToBuffer(c2, num2, writeToPosition);
					num2 = num;
					break;
				}
				case '\r':
					_charPos = num - 1;
					ProcessCarriageReturn(true);
					num = _charPos;
					break;
				case '\n':
					_charPos = num - 1;
					ProcessLineFeed();
					num = _charPos;
					break;
				case '"':
				case '\'':
					if (_chars[num - 1] == quote)
					{
						num--;
						if (charPos == num2)
						{
							_stringReference = new StringReference(_chars, charPos, num - charPos);
						}
						else
						{
							EnsureBufferNotEmpty();
							if (num > num2)
							{
								_stringBuffer.Append(_arrayPool, _chars, num2, num - num2);
							}
							_stringReference = new StringReference(_stringBuffer.InternalBuffer, 0, _stringBuffer.Position);
						}
						num = (_charPos = num + 1);
						return;
					}
					break;
				}
			}
		}

		private void WriteCharToBuffer(char writeChar, int lastWritePosition, int writeToPosition)
		{
			if (writeToPosition > lastWritePosition)
			{
				_stringBuffer.Append(_arrayPool, _chars, lastWritePosition, writeToPosition - lastWritePosition);
			}
			_stringBuffer.Append(_arrayPool, writeChar);
		}

		private char ParseUnicode()
		{
			if (!EnsureChars(4, true))
			{
				throw JsonReaderException.Create(this, "Unexpected end while parsing unicode character.");
			}
			char result = Convert.ToChar(ConvertUtils.HexTextToInt(_chars, _charPos, _charPos + 4));
			_charPos += 4;
			return result;
		}

		private void ReadNumberIntoBuffer()
		{
			int num = _charPos;
			while (true)
			{
				switch (_chars[num])
				{
				case '\0':
					_charPos = num;
					if (_charsUsed != num || ReadData(true) == 0)
					{
						return;
					}
					break;
				case '+':
				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
				case 'X':
				case 'a':
				case 'b':
				case 'c':
				case 'd':
				case 'e':
				case 'f':
				case 'x':
					num++;
					break;
				default:
				{
					_charPos = num;
					char c = _chars[_charPos];
					if (char.IsWhiteSpace(c) || c == ',' || c == '}' || c == ']' || c == ')' || c == '/')
					{
						return;
					}
					throw JsonReaderException.Create(this, "Unexpected character encountered while parsing number: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
				}
				}
			}
		}

		private void ClearRecentString()
		{
			_stringBuffer.Position = 0;
			_stringReference = default(StringReference);
		}

		private bool ParsePostValue()
		{
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (_charsUsed == _charPos)
					{
						if (ReadData(false) == 0)
						{
							_currentState = State.Finished;
							return false;
						}
					}
					else
					{
						_charPos++;
					}
					break;
				case '}':
					_charPos++;
					SetToken(JsonToken.EndObject);
					return true;
				case ']':
					_charPos++;
					SetToken(JsonToken.EndArray);
					return true;
				case ')':
					_charPos++;
					SetToken(JsonToken.EndConstructor);
					return true;
				case '/':
					ParseComment(true);
					return true;
				case ',':
					_charPos++;
					SetStateBasedOnCurrent();
					return false;
				case '\t':
				case ' ':
					_charPos++;
					break;
				case '\r':
					ProcessCarriageReturn(false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				default:
					if (!char.IsWhiteSpace(c))
					{
						throw JsonReaderException.Create(this, "After parsing a value an unexpected character was encountered: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
					}
					_charPos++;
					break;
				}
			}
		}

		private bool ParseObject()
		{
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (_charsUsed == _charPos)
					{
						if (ReadData(false) == 0)
						{
							return false;
						}
					}
					else
					{
						_charPos++;
					}
					break;
				case '}':
					SetToken(JsonToken.EndObject);
					_charPos++;
					return true;
				case '/':
					ParseComment(true);
					return true;
				case '\r':
					ProcessCarriageReturn(false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					if (!char.IsWhiteSpace(c))
					{
						return ParseProperty();
					}
					_charPos++;
					break;
				}
			}
		}

		private bool ParseProperty()
		{
			char c = _chars[_charPos];
			char c2;
			if (c == '"' || c == '\'')
			{
				_charPos++;
				c2 = c;
				ShiftBufferIfNeeded();
				ReadStringIntoBuffer(c2);
			}
			else
			{
				if (!ValidIdentifierChar(c))
				{
					throw JsonReaderException.Create(this, "Invalid property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
				}
				c2 = '\0';
				ShiftBufferIfNeeded();
				ParseUnquotedProperty();
			}
			string text;
			if (NameTable != null)
			{
				text = NameTable.Get(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length);
				if (text == null)
				{
					text = _stringReference.ToString();
				}
			}
			else
			{
				text = _stringReference.ToString();
			}
			EatWhitespace(false);
			if (_chars[_charPos] != ':')
			{
				throw JsonReaderException.Create(this, "Invalid character after parsing property name. Expected ':' but got: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
			}
			_charPos++;
			SetToken(JsonToken.PropertyName, text);
			_quoteChar = c2;
			ClearRecentString();
			return true;
		}

		private bool ValidIdentifierChar(char value)
		{
			if (!char.IsLetterOrDigit(value) && value != '_')
			{
				return value == '$';
			}
			return true;
		}

		private void ParseUnquotedProperty()
		{
			int charPos = _charPos;
			char c;
			while (true)
			{
				if (_chars[_charPos] == '\0')
				{
					if (_charsUsed != _charPos)
					{
						_stringReference = new StringReference(_chars, charPos, _charPos - charPos);
						return;
					}
					if (ReadData(true) == 0)
					{
						throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
					}
				}
				else
				{
					c = _chars[_charPos];
					if (!ValidIdentifierChar(c))
					{
						break;
					}
					_charPos++;
				}
			}
			if (char.IsWhiteSpace(c) || c == ':')
			{
				_stringReference = new StringReference(_chars, charPos, _charPos - charPos);
				return;
			}
			throw JsonReaderException.Create(this, "Invalid JavaScript property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
		}

		private bool ParseValue()
		{
			while (true)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (_charsUsed == _charPos)
					{
						if (ReadData(false) == 0)
						{
							return false;
						}
					}
					else
					{
						_charPos++;
					}
					break;
				case '"':
				case '\'':
					ParseString(c, ReadType.Read);
					return true;
				case 't':
					ParseTrue();
					return true;
				case 'f':
					ParseFalse();
					return true;
				case 'n':
					if (!EnsureChars(1, true))
					{
						_charPos++;
						throw CreateUnexpectedEndException();
					}
					switch (_chars[_charPos + 1])
					{
					case 'u':
						ParseNull();
						break;
					case 'e':
						ParseConstructor();
						break;
					default:
						throw CreateUnexpectedCharacterException(_chars[_charPos]);
					}
					return true;
				case 'N':
					ParseNumberNaN(ReadType.Read);
					return true;
				case 'I':
					ParseNumberPositiveInfinity(ReadType.Read);
					return true;
				case '-':
					if (EnsureChars(1, true) && _chars[_charPos + 1] == 'I')
					{
						ParseNumberNegativeInfinity(ReadType.Read);
					}
					else
					{
						ParseNumber(ReadType.Read);
					}
					return true;
				case '/':
					ParseComment(true);
					return true;
				case 'u':
					ParseUndefined();
					return true;
				case '{':
					_charPos++;
					SetToken(JsonToken.StartObject);
					return true;
				case '[':
					_charPos++;
					SetToken(JsonToken.StartArray);
					return true;
				case ']':
					_charPos++;
					SetToken(JsonToken.EndArray);
					return true;
				case ',':
					SetToken(JsonToken.Undefined);
					return true;
				case ')':
					_charPos++;
					SetToken(JsonToken.EndConstructor);
					return true;
				case '\r':
					ProcessCarriageReturn(false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					_charPos++;
					break;
				default:
					if (!char.IsWhiteSpace(c))
					{
						if (char.IsNumber(c) || c == '-' || c == '.')
						{
							ParseNumber(ReadType.Read);
							return true;
						}
						throw CreateUnexpectedCharacterException(c);
					}
					_charPos++;
					break;
				}
			}
		}

		private void ProcessLineFeed()
		{
			_charPos++;
			OnNewLine(_charPos);
		}

		private void ProcessCarriageReturn(bool append)
		{
			_charPos++;
			if (EnsureChars(1, append) && _chars[_charPos] == '\n')
			{
				_charPos++;
			}
			OnNewLine(_charPos);
		}

		private bool EatWhitespace(bool oneOrMore)
		{
			bool flag = false;
			bool flag2 = false;
			while (!flag)
			{
				char c = _chars[_charPos];
				switch (c)
				{
				case '\0':
					if (_charsUsed == _charPos)
					{
						if (ReadData(false) == 0)
						{
							flag = true;
						}
					}
					else
					{
						_charPos++;
					}
					break;
				case '\r':
					ProcessCarriageReturn(false);
					break;
				case '\n':
					ProcessLineFeed();
					break;
				default:
					if (!char.IsWhiteSpace(c))
					{
						flag = true;
						break;
					}
					goto case ' ';
				case ' ':
					flag2 = true;
					_charPos++;
					break;
				}
			}
			return !oneOrMore | flag2;
		}

		private void ParseConstructor()
		{
			if (MatchValueWithTrailingSeparator("new"))
			{
				EatWhitespace(false);
				int charPos = _charPos;
				int charPos2;
				while (true)
				{
					char c = _chars[_charPos];
					if (c == '\0')
					{
						if (_charsUsed != _charPos)
						{
							charPos2 = _charPos;
							_charPos++;
							break;
						}
						if (ReadData(true) == 0)
						{
							throw JsonReaderException.Create(this, "Unexpected end while parsing constructor.");
						}
					}
					else
					{
						if (!char.IsLetterOrDigit(c))
						{
							switch (c)
							{
							case '\r':
								charPos2 = _charPos;
								ProcessCarriageReturn(true);
								break;
							case '\n':
								charPos2 = _charPos;
								ProcessLineFeed();
								break;
							default:
								if (char.IsWhiteSpace(c))
								{
									charPos2 = _charPos;
									_charPos++;
								}
								else
								{
									if (c != '(')
									{
										throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
									}
									charPos2 = _charPos;
								}
								break;
							}
							break;
						}
						_charPos++;
					}
				}
				_stringReference = new StringReference(_chars, charPos, charPos2 - charPos);
				string value = _stringReference.ToString();
				EatWhitespace(false);
				if (_chars[_charPos] != '(')
				{
					throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
				}
				_charPos++;
				ClearRecentString();
				SetToken(JsonToken.StartConstructor, value);
				return;
			}
			throw JsonReaderException.Create(this, "Unexpected content while parsing JSON.");
		}

		private void ParseNumber(ReadType readType)
		{
			ShiftBufferIfNeeded();
			char c = _chars[_charPos];
			int charPos = _charPos;
			ReadNumberIntoBuffer();
			SetPostValueState(true);
			_stringReference = new StringReference(_chars, charPos, _charPos - charPos);
			bool flag = char.IsDigit(c) && _stringReference.Length == 1;
			bool flag2 = c == '0' && _stringReference.Length > 1 && _stringReference.Chars[_stringReference.StartIndex + 1] != '.' && _stringReference.Chars[_stringReference.StartIndex + 1] != 'e' && _stringReference.Chars[_stringReference.StartIndex + 1] != 'E';
			JsonToken newToken;
			object value;
			switch (readType)
			{
			case ReadType.ReadAsString:
			{
				string text5 = _stringReference.ToString();
				double result5;
				if (flag2)
				{
					try
					{
						if (text5.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
						{
							Convert.ToInt64(text5, 16);
						}
						else
						{
							Convert.ToInt64(text5, 8);
						}
					}
					catch (Exception ex4)
					{
						throw JsonReaderException.Create(this, "Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, text5), ex4);
					}
				}
				else if (!double.TryParse(text5, NumberStyles.Float, CultureInfo.InvariantCulture, out result5))
				{
					throw JsonReaderException.Create(this, "Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
				}
				newToken = JsonToken.String;
				value = text5;
				break;
			}
			case ReadType.ReadAsInt32:
				if (flag)
				{
					value = c - 48;
				}
				else if (flag2)
				{
					string text6 = _stringReference.ToString();
					try
					{
						value = (text6.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(text6, 16) : Convert.ToInt32(text6, 8));
					}
					catch (Exception ex5)
					{
						throw JsonReaderException.Create(this, "Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, text6), ex5);
					}
				}
				else
				{
					int value3;
					switch (ConvertUtils.Int32TryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out value3))
					{
					case ParseResult.Success:
						break;
					case ParseResult.Overflow:
						throw JsonReaderException.Create(this, "JSON integer {0} is too large or small for an Int32.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
					default:
						throw JsonReaderException.Create(this, "Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
					}
					value = value3;
				}
				newToken = JsonToken.Integer;
				break;
			case ReadType.ReadAsDecimal:
				if (flag)
				{
					value = (decimal)c - 48m;
				}
				else if (flag2)
				{
					string text3 = _stringReference.ToString();
					try
					{
						value = Convert.ToDecimal(text3.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text3, 16) : Convert.ToInt64(text3, 8));
					}
					catch (Exception ex2)
					{
						throw JsonReaderException.Create(this, "Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, text3), ex2);
					}
				}
				else
				{
					decimal result3;
					if (!decimal.TryParse(_stringReference.ToString(), NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out result3))
					{
						throw JsonReaderException.Create(this, "Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
					}
					value = result3;
				}
				newToken = JsonToken.Float;
				break;
			case ReadType.ReadAsDouble:
				if (flag)
				{
					value = (double)(int)c - 48.0;
				}
				else if (flag2)
				{
					string text4 = _stringReference.ToString();
					try
					{
						value = Convert.ToDouble(text4.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text4, 16) : Convert.ToInt64(text4, 8));
					}
					catch (Exception ex3)
					{
						throw JsonReaderException.Create(this, "Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, text4), ex3);
					}
				}
				else
				{
					double result4;
					if (!double.TryParse(_stringReference.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result4))
					{
						throw JsonReaderException.Create(this, "Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
					}
					value = result4;
				}
				newToken = JsonToken.Float;
				break;
			default:
				if (flag)
				{
					value = (long)c - 48L;
					newToken = JsonToken.Integer;
				}
				else if (flag2)
				{
					string text = _stringReference.ToString();
					try
					{
						value = (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text, 16) : Convert.ToInt64(text, 8));
					}
					catch (Exception ex)
					{
						throw JsonReaderException.Create(this, "Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, text), ex);
					}
					newToken = JsonToken.Integer;
				}
				else
				{
					long value2;
					switch (ConvertUtils.Int64TryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out value2))
					{
					case ParseResult.Success:
						value = value2;
						newToken = JsonToken.Integer;
						break;
					case ParseResult.Overflow:
						throw JsonReaderException.Create(this, "JSON integer {0} is too large or small for an Int64.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
					default:
					{
						string text2 = _stringReference.ToString();
						if (_floatParseHandling == FloatParseHandling.Decimal)
						{
							decimal result;
							if (!decimal.TryParse(text2, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out result))
							{
								throw JsonReaderException.Create(this, "Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, text2));
							}
							value = result;
						}
						else
						{
							double result2;
							if (!double.TryParse(text2, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out result2))
							{
								throw JsonReaderException.Create(this, "Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, text2));
							}
							value = result2;
						}
						newToken = JsonToken.Float;
						break;
					}
					}
				}
				break;
			}
			ClearRecentString();
			SetToken(newToken, value, false);
		}

		private void ParseComment(bool setToken)
		{
			_charPos++;
			if (EnsureChars(1, false))
			{
				bool flag;
				if (_chars[_charPos] == '*')
				{
					flag = false;
				}
				else
				{
					if (_chars[_charPos] != '/')
					{
						throw JsonReaderException.Create(this, "Error parsing comment. Expected: *, got {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
					}
					flag = true;
				}
				_charPos++;
				int charPos = _charPos;
				while (true)
				{
					switch (_chars[_charPos])
					{
					case '\0':
						if (_charsUsed == _charPos)
						{
							if (ReadData(true) == 0)
							{
								if (!flag)
								{
									throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
								}
								EndComment(setToken, charPos, _charPos);
								return;
							}
						}
						else
						{
							_charPos++;
						}
						break;
					case '*':
						_charPos++;
						if (!flag && EnsureChars(0, true) && _chars[_charPos] == '/')
						{
							EndComment(setToken, charPos, _charPos - 1);
							_charPos++;
							return;
						}
						break;
					case '\r':
						if (flag)
						{
							EndComment(setToken, charPos, _charPos);
							return;
						}
						ProcessCarriageReturn(true);
						break;
					case '\n':
						if (flag)
						{
							EndComment(setToken, charPos, _charPos);
							return;
						}
						ProcessLineFeed();
						break;
					default:
						_charPos++;
						break;
					}
				}
			}
			throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
		}

		private void EndComment(bool setToken, int initialPosition, int endPosition)
		{
			if (setToken)
			{
				SetToken(JsonToken.Comment, new string(_chars, initialPosition, endPosition - initialPosition));
			}
		}

		private bool MatchValue(string value)
		{
			if (!EnsureChars(value.Length - 1, true))
			{
				_charPos = _charsUsed;
				throw CreateUnexpectedEndException();
			}
			for (int i = 0; i < value.Length; i++)
			{
				if (_chars[_charPos + i] != value[i])
				{
					_charPos += i;
					return false;
				}
			}
			_charPos += value.Length;
			return true;
		}

		private bool MatchValueWithTrailingSeparator(string value)
		{
			if (!MatchValue(value))
			{
				return false;
			}
			if (!EnsureChars(0, false))
			{
				return true;
			}
			if (!IsSeparator(_chars[_charPos]))
			{
				return _chars[_charPos] == '\0';
			}
			return true;
		}

		private bool IsSeparator(char c)
		{
			switch (c)
			{
			case ',':
			case ']':
			case '}':
				return true;
			case '/':
			{
				if (!EnsureChars(1, false))
				{
					return false;
				}
				char c2 = _chars[_charPos + 1];
				if (c2 != '*')
				{
					return c2 == '/';
				}
				return true;
			}
			case ')':
				if (base.CurrentState == State.Constructor || base.CurrentState == State.ConstructorStart)
				{
					return true;
				}
				break;
			case '\t':
			case '\n':
			case '\r':
			case ' ':
				return true;
			default:
				if (char.IsWhiteSpace(c))
				{
					return true;
				}
				break;
			}
			return false;
		}

		private void ParseTrue()
		{
			if (MatchValueWithTrailingSeparator(JsonConvert.True))
			{
				SetToken(JsonToken.Boolean, true);
				return;
			}
			throw JsonReaderException.Create(this, "Error parsing boolean value.");
		}

		private void ParseNull()
		{
			if (MatchValueWithTrailingSeparator(JsonConvert.Null))
			{
				SetToken(JsonToken.Null);
				return;
			}
			throw JsonReaderException.Create(this, "Error parsing null value.");
		}

		private void ParseUndefined()
		{
			if (MatchValueWithTrailingSeparator(JsonConvert.Undefined))
			{
				SetToken(JsonToken.Undefined);
				return;
			}
			throw JsonReaderException.Create(this, "Error parsing undefined value.");
		}

		private void ParseFalse()
		{
			if (MatchValueWithTrailingSeparator(JsonConvert.False))
			{
				SetToken(JsonToken.Boolean, false);
				return;
			}
			throw JsonReaderException.Create(this, "Error parsing boolean value.");
		}

		private object ParseNumberNegativeInfinity(ReadType readType)
		{
			if (MatchValueWithTrailingSeparator(JsonConvert.NegativeInfinity))
			{
				switch (readType)
				{
				case ReadType.Read:
				case ReadType.ReadAsDouble:
					if (_floatParseHandling == FloatParseHandling.Double)
					{
						SetToken(JsonToken.Float, double.NegativeInfinity);
						return double.NegativeInfinity;
					}
					break;
				case ReadType.ReadAsString:
					SetToken(JsonToken.String, JsonConvert.NegativeInfinity);
					return JsonConvert.NegativeInfinity;
				}
				throw JsonReaderException.Create(this, "Cannot read -Infinity value.");
			}
			throw JsonReaderException.Create(this, "Error parsing -Infinity value.");
		}

		private object ParseNumberPositiveInfinity(ReadType readType)
		{
			if (MatchValueWithTrailingSeparator(JsonConvert.PositiveInfinity))
			{
				switch (readType)
				{
				case ReadType.Read:
				case ReadType.ReadAsDouble:
					if (_floatParseHandling == FloatParseHandling.Double)
					{
						SetToken(JsonToken.Float, double.PositiveInfinity);
						return double.PositiveInfinity;
					}
					break;
				case ReadType.ReadAsString:
					SetToken(JsonToken.String, JsonConvert.PositiveInfinity);
					return JsonConvert.PositiveInfinity;
				}
				throw JsonReaderException.Create(this, "Cannot read Infinity value.");
			}
			throw JsonReaderException.Create(this, "Error parsing Infinity value.");
		}

		private object ParseNumberNaN(ReadType readType)
		{
			if (MatchValueWithTrailingSeparator(JsonConvert.NaN))
			{
				switch (readType)
				{
				case ReadType.Read:
				case ReadType.ReadAsDouble:
					if (_floatParseHandling == FloatParseHandling.Double)
					{
						SetToken(JsonToken.Float, double.NaN);
						return double.NaN;
					}
					break;
				case ReadType.ReadAsString:
					SetToken(JsonToken.String, JsonConvert.NaN);
					return JsonConvert.NaN;
				}
				throw JsonReaderException.Create(this, "Cannot read NaN value.");
			}
			throw JsonReaderException.Create(this, "Error parsing NaN value.");
		}

		public override void Close()
		{
			base.Close();
			if (_chars != null)
			{
				BufferUtils.ReturnBuffer(_arrayPool, _chars);
				_chars = null;
			}
			if (base.CloseInput && _reader != null)
			{
				_reader.Close();
			}
			_stringBuffer.Clear(_arrayPool);
		}

		public bool HasLineInfo()
		{
			return true;
		}
	}
}
