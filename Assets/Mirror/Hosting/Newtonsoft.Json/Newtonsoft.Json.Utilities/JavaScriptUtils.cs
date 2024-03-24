using Newtonsoft.Json.Shims;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Newtonsoft.Json.Utilities
{
	[Preserve]
	internal static class JavaScriptUtils
	{
		internal static readonly bool[] SingleQuoteCharEscapeFlags;

		internal static readonly bool[] DoubleQuoteCharEscapeFlags;

		internal static readonly bool[] HtmlCharEscapeFlags;

		private const int UnicodeTextLength = 6;

		private const string EscapedUnicodeText = "!";

		static JavaScriptUtils()
		{
			SingleQuoteCharEscapeFlags = new bool[128];
			DoubleQuoteCharEscapeFlags = new bool[128];
			HtmlCharEscapeFlags = new bool[128];
			IList<char> list = new List<char>
			{
				'\n',
				'\r',
				'\t',
				'\\',
				'\f',
				'\b'
			};
			for (int i = 0; i < 32; i++)
			{
				list.Add((char)i);
			}
			foreach (char item in list.Union(new char[1]
			{
				'\''
			}))
			{
				SingleQuoteCharEscapeFlags[item] = true;
			}
			foreach (char item2 in list.Union(new char[1]
			{
				'"'
			}))
			{
				DoubleQuoteCharEscapeFlags[item2] = true;
			}
			foreach (char item3 in list.Union(new char[5]
			{
				'"',
				'\'',
				'<',
				'>',
				'&'
			}))
			{
				HtmlCharEscapeFlags[item3] = true;
			}
		}

		public static bool[] GetCharEscapeFlags(StringEscapeHandling stringEscapeHandling, char quoteChar)
		{
			if (stringEscapeHandling == StringEscapeHandling.EscapeHtml)
			{
				return HtmlCharEscapeFlags;
			}
			if (quoteChar == '"')
			{
				return DoubleQuoteCharEscapeFlags;
			}
			return SingleQuoteCharEscapeFlags;
		}

		public static bool ShouldEscapeJavaScriptString(string s, bool[] charEscapeFlags)
		{
			if (s == null)
			{
				return false;
			}
			foreach (char c in s)
			{
				if (c >= charEscapeFlags.Length || charEscapeFlags[c])
				{
					return true;
				}
			}
			return false;
		}

		public static void WriteEscapedJavaScriptString(TextWriter writer, string s, char delimiter, bool appendDelimiters, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, IArrayPool<char> bufferPool, ref char[] writeBuffer)
		{
			if (appendDelimiters)
			{
				writer.Write(delimiter);
			}
			if (s != null)
			{
				int num = 0;
				for (int i = 0; i < s.Length; i++)
				{
					char c = s[i];
					if (c >= charEscapeFlags.Length || charEscapeFlags[c])
					{
						string text;
						switch (c)
						{
						case '\t':
							text = "\\t";
							break;
						case '\n':
							text = "\\n";
							break;
						case '\r':
							text = "\\r";
							break;
						case '\f':
							text = "\\f";
							break;
						case '\b':
							text = "\\b";
							break;
						case '\\':
							text = "\\\\";
							break;
						case '\u0085':
							text = "\\u0085";
							break;
						case '\u2028':
							text = "\\u2028";
							break;
						case '\u2029':
							text = "\\u2029";
							break;
						default:
							if (c < charEscapeFlags.Length || stringEscapeHandling == StringEscapeHandling.EscapeNonAscii)
							{
								if (c == '\'' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
								{
									text = "\\'";
								}
								else if (c == '"' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
								{
									text = "\\\"";
								}
								else
								{
									if (writeBuffer == null || writeBuffer.Length < 6)
									{
										writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, 6, writeBuffer);
									}
									StringUtils.ToCharAsUnicode(c, writeBuffer);
									text = "!";
								}
							}
							else
							{
								text = null;
							}
							break;
						}
						if (text != null)
						{
							bool flag = string.Equals(text, "!");
							if (i > num)
							{
								int num2 = i - num + (flag ? 6 : 0);
								int num3 = flag ? 6 : 0;
								if (writeBuffer == null || writeBuffer.Length < num2)
								{
									char[] array = BufferUtils.RentBuffer(bufferPool, num2);
									if (flag)
									{
										Array.Copy(writeBuffer, array, 6);
									}
									BufferUtils.ReturnBuffer(bufferPool, writeBuffer);
									writeBuffer = array;
								}
								s.CopyTo(num, writeBuffer, num3, num2 - num3);
								writer.Write(writeBuffer, num3, num2 - num3);
							}
							num = i + 1;
							if (!flag)
							{
								writer.Write(text);
							}
							else
							{
								writer.Write(writeBuffer, 0, 6);
							}
						}
					}
				}
				if (num == 0)
				{
					writer.Write(s);
				}
				else
				{
					int num4 = s.Length - num;
					if (writeBuffer == null || writeBuffer.Length < num4)
					{
						writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, num4, writeBuffer);
					}
					s.CopyTo(num, writeBuffer, 0, num4);
					writer.Write(writeBuffer, 0, num4);
				}
			}
			if (appendDelimiters)
			{
				writer.Write(delimiter);
			}
		}

		public static string ToEscapedJavaScriptString(string value, char delimiter, bool appendDelimiters, StringEscapeHandling stringEscapeHandling)
		{
			bool[] charEscapeFlags = GetCharEscapeFlags(stringEscapeHandling, delimiter);
			using (StringWriter stringWriter = StringUtils.CreateStringWriter(StringUtils.GetLength(value) ?? 16))
			{
				char[] writeBuffer = null;
				WriteEscapedJavaScriptString(stringWriter, value, delimiter, appendDelimiters, charEscapeFlags, stringEscapeHandling, null, ref writeBuffer);
				return stringWriter.ToString();
			}
		}
	}
}
