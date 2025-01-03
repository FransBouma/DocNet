﻿// 
//   MarkdownDeep - http://www.toptensoftware.com/markdowndeep
//	 Copyright (C) 2010-2011 Topten Software
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this product except in 
//   compliance with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License is 
//   distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownDeep
{
	/*
	 * Various utility and extension methods
	 */
	static class Utils
	{
		// Extension method. Get the last item in a list (or null if empty)
		public static T Last<T>(this List<T> list) where T:class
		{
			if (list.Count > 0)
				return list[list.Count - 1];
			else
				return null;
		}

		// Extension method. Get the first item in a list (or null if empty)
		public static T First<T>(this List<T> list) where T : class
		{
			if (list.Count > 0)
				return list[0];
			else
				return null;
		}

		// Extension method.  Use a list like a stack
		public static void Push<T>(this List<T> list, T value) where T : class
		{
			list.Add(value);
		}

		// Extension method.  Remove last item from a list
		public static T Pop<T>(this List<T> list) where T : class
		{
			if (list.Count == 0)
				return null;
			else
			{
				T val = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				return val;
			}
		}


		// Scan a string for a valid identifier.  Identifier must start with alpha or underscore
		// and can be followed by alpha, digit or underscore. If dashIsValidChar, additionally to underscore, a '-' is also accepted as a valid identifier character. 
		// Updates `pos` to character after the identifier if matched
		public static bool ParseIdentifier(string str, ref int pos, ref string identifer, bool dashIsValidChar)
		{
			if (pos >= str.Length)
				return false;

			// Must start with a letter or underscore, or if marked as valid, a dash
			if (!char.IsLetter(str[pos]) && str[pos] != '_' && (!dashIsValidChar || (dashIsValidChar && str[pos] != '-')))
			{
				return false;
			}

			// Find the end
			int startpos = pos;
			pos++;
		    while(pos < str.Length && (char.IsDigit(str[pos]) || char.IsLetter(str[pos]) || str[pos] == '_' || (dashIsValidChar && str[pos]=='-')))
		    {
		        pos++;
		    }

		    // Return it
			identifer = str.Substring(startpos, pos - startpos);
			return true;
		}

		
		// Skip over anything that looks like a valid html entity (eg: &amp, &#123, &#nnn) etc...
		// Updates `pos` to character after the entity if matched
		public static bool SkipHtmlEntity(string str, ref int pos, ref string entity)
		{
			if (str[pos] != '&')
				return false;

			int savepos = pos;
			int len = str.Length;
			int i = pos+1;

			// Number entity?
			bool bNumber=false;
			bool bHex = false;
			if (i < len && str[i] == '#')
			{
				bNumber = true;
				i++;

				// Hex identity?
				if (i < len && (str[i] == 'x' || str[i] == 'X'))
				{
					bHex = true;
					i++;
				}
			}

			// Parse the content
			int contentpos = i;
			while (i < len)
			{
				char ch=str[i];

				if (bHex)
				{
					if (!(char.IsDigit(ch) || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')))
						break;
				}

				else if (bNumber)
				{
					if (!char.IsDigit(ch))
						break;
				}
				else if (!char.IsLetterOrDigit(ch))
					break;

				i++;
			}

			// Quit if ran out of string
			if (i == len)
				return false;

			// Quit if nothing in the content
			if (i == contentpos)
				return false;

			// Quit if didn't find a semicolon
			if (str[i] != ';')
				return false;

			// Looks good...
			pos = i + 1;

			entity = str.Substring(savepos, pos - savepos);
			return true;
		}

		// Randomize a string using html entities;
		public static void HtmlRandomize(StringBuilder dest, string str)
		{
			// Deterministic random seed
			int seed = 0;
			foreach (char ch in str)
			{
				seed = unchecked(seed + ch);
			}
			Random r = new Random(seed);

			// Randomize
			foreach (char ch in str)
			{
				int x = r.Next() % 100;
				if (x > 90 && ch != '@')
				{
					dest.Append(ch);
				}
				else if (x > 45)
				{
					dest.Append("&#");
					dest.Append(((int)ch).ToString());
					dest.Append(";");
				}
				else
				{
					dest.Append("&#x");
					dest.Append(((int)ch).ToString("x"));
					dest.Append(";");
				}

			}
		}

		// Like HtmlEncode, but don't escape &'s that look like html entities
		public static void SmartHtmlEncodeAmpsAndAngles(StringBuilder dest, string str)
		{
			if (str == null)
				return;

			for (int i=0; i<str.Length; i++)
			{
				switch (str[i])
				{
					case '&':
						int start = i;
						string unused=null;
						if (SkipHtmlEntity(str, ref i, ref unused))
						{
							dest.Append(str, start, i - start);
							i--;
						}
						else
						{
							dest.Append("&amp;");
						}
						break;

					case '<':
						dest.Append("&lt;");
						break;

					case '>':
						dest.Append("&gt;");
						break;

					case '\"':
						dest.Append("&quot;");
						break;

					default:
						dest.Append(str[i]);
						break;
				}
			}
		}


		// Like HtmlEncode, but only escape &'s that don't look like html entities
		public static void SmartHtmlEncodeAmps(StringBuilder dest, string str, int startOffset, int len)
		{
			int end = startOffset + len;
			for (int i = startOffset; i < end; i++)
			{
				switch (str[i])
				{
					case '&':
						int start = i;
						string unused = null;
						if (SkipHtmlEntity(str, ref i, ref unused))
						{
							dest.Append(str, start, i - start);
							i--;
						}
						else
						{
							dest.Append("&amp;");
						}
						break;

					default:
						dest.Append(str[i]);
						break;
				}
			}
		}

		// Check if a string is in an array of strings
		public static bool IsInList(string str, string[] list)
		{
			foreach (var t in list)
			{
				if (string.Compare(t, str) == 0)
					return true;
			}
			return false;
		}

		// Check if a url is "safe" (we require urls start with valid protocol)
		// Definitely don't allow "javascript:" or any of it's encodings.
		public static bool IsSafeUrl(string url)
		{
			if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("ftp://"))
				return false;

			return true;
		}

		// Check if a character is escapable in markdown
		public static bool IsEscapableChar(char ch, bool ExtraMode)
		{
			switch (ch)
			{
				case '\\':
				case '`':
				case '*':
				case '_':
				case '{':
				case '}':
				case '[':
				case ']':
				case '(':
				case ')':
				case '>':		// Not in markdown documentation, but is in markdown.pl
				case '#':
				case '+':
				case '-':
				case '.':
				case '!':
					return true;

				case ':':
				case '|':
				case '=':		// Added for escaping Setext H1
				case '<':
					return ExtraMode;
			}

			return false;
		}

		// Extension method.  Skip an escapable character, or one normal character
		public static void SkipEscapableChar(this StringScanner p, bool ExtraMode)
		{
			if (p.Current == '\\' && IsEscapableChar(p.CharAtOffset(1), ExtraMode))
			{
				p.SkipForward(2);
			}
			else
			{
				p.SkipForward(1);
			}
		}


		// Remove the markdown escapes from a string
		public static string UnescapeString(string str, bool ExtraMode)
		{
			if (str == null || str.IndexOf('\\')==-1)
				return str;

			var b = new StringBuilder();
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '\\' && i+1<str.Length && IsEscapableChar(str[i+1], ExtraMode))
				{
					b.Append(str[i + 1]);
					i++;
				}
				else
				{
					b.Append(str[i]);
				}
			}

			return b.ToString();

		}

		// Normalize the line ends in a string to just '\n'
		// Handles all encodings - '\r\n' (windows), '\n\r' (mac), '\n' (unix) '\r' (something?)
		static char[] lineends = new char[] { '\r', '\n' };
		public static string NormalizeLineEnds(string str)
		{
			if (str.IndexOfAny(lineends) < 0)
				return str;

			StringBuilder sb = new StringBuilder();
			StringScanner sp = new StringScanner(str);
			while (!sp.Eof)
			{
				if (sp.Eol)
				{
					sb.Append('\n');
					sp.SkipEol();
				}
				else
				{
					sb.Append(sp.Current);
					sp.SkipForward(1);
				}
			}

			return sb.ToString();
		}

		/*
		 * These two functions IsEmailAddress and IsWebAddress
		 * are intended as a quick and dirty way to tell if a 
		 * <autolink> url is email, web address or neither.
		 * 
		 * They are not intended as validating checks.
		 * 
		 * (use of Regex for more correct test unnecessarily
		 *  slowed down some test documents by up to 300%.)
		 */

		// Check if a string looks like an email address
		public static bool IsEmailAddress(string str)
		{
			int posAt = str.IndexOf('@');
			if (posAt < 0)
				return false;

			int posLastDot = str.LastIndexOf('.');
			if (posLastDot < posAt)
				return false;

			return true;
		}

		// Check if a string looks like a url
		public static bool IsWebAddress(string str)
		{
			return str.StartsWith("http://") ||
					str.StartsWith("https://") ||
					str.StartsWith("ftp://") ||
					str.StartsWith("file://");
		}

		// Check if a string is a valid HTML ID identifier
		internal static bool IsValidHtmlID(string str)
		{
			if (String.IsNullOrEmpty(str))
				return false;

			// Must start with a letter
			if (!Char.IsLetter(str[0]))
				return false;

			// Check the rest
			for (int i = 0; i < str.Length; i++)
			{
				char ch = str[i];
				if (Char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == ':' || ch == '.')
					continue;

				return false;
			}

			// OK
			return true;
		}

		// Strip the trailing HTML ID from a header string
		// ie:      ## header text ##			{#<idhere>}
		//			^start           ^out end              ^end
		//
		// Returns null if no header id
		public static string StripHtmlID(string str, int start, ref int end)
		{
			// Skip trailing whitespace
			int pos = end - 1;
			while (pos >= start && Char.IsWhiteSpace(str[pos]))
			{
				pos--;
			}

			// Skip closing '{'
			if (pos < start || str[pos] != '}')
				return null;

			int endId = pos;
			pos--;

			// Find the opening '{'
			while (pos >= start && str[pos] != '{')
				pos--;

			// Check for the #
			if (pos < start || str[pos + 1] != '#')
				return null;

			// Extract and check the ID
			int startId = pos + 2;
			string strID = str.Substring(startId, endId - startId);
			if (!IsValidHtmlID(strID))
				return null;

			// Skip any preceeding whitespace
			while (pos > start && Char.IsWhiteSpace(str[pos - 1]))
				pos--;

			// Done!
			end = pos;
			return strID;
		}


		/// <summary>
		/// Strips the special attributes specified at the location of start. Special attributes are a php markdown extension
		/// and specified between {}. Attributes are separated with spaces. It recognizes the following attributes:
		/// id, which start with a '#'. Only the first one is used
		/// css classes, which start with a'.'. All specified are used
		/// name=value pairs, which will end up as attributes on the element. 
		/// </summary>
		/// <param name="str">The string we're scanning.</param>
		/// <param name="start">The start (current) position.</param>
		/// <param name="end">The end position. Is only valid if returned list contains at least one value.</param>
		/// <returns>list of special attributes found, or null if none found or error in string format.</returns>
		public static List<string> StripSpecialAttributes(string str, int start, out int end)
		{
			end = start;
			var scanner = new StringScanner(str, start);
			if(!scanner.DoesMatch('{'))
			{
				// not a start of a special attribute block
				return null;
			}
			// first find the end of the block, 
			scanner.SkipForward(1);
			var startOfAttributes = scanner.Position;
			// first find the next EOL, as the closing } has to be on this line.
			scanner.SkipToEol();
			var nextEolPos = scanner.Position;
			scanner.Position = startOfAttributes;
			scanner.Mark();
			if(!scanner.Find('}') || scanner.Position >= nextEolPos)
			{
				// not enclosed properly.
				return null;
			}
			var attributesString = scanner.Extract();
			if(string.IsNullOrWhiteSpace(attributesString))
			{
				return null;
			}
			// Position is on enclosing '}' (due to the Find('}'), so we have to skip 1 character
			end = scanner.Position + 1;
			return attributesString.Split(' ').Where(s=>!string.IsNullOrWhiteSpace(s)).ToList();
		}

		public static bool IsUrlFullyQualified(string url)
		{
			return url.Contains("://") || url.StartsWith("mailto:");
		}


		/// <summary>
		/// Skips the font awesome specification at the current specified pos. Pos is inside str at the '@'. font awesome
		/// directives are @fa-iconname. newPos is positioned to character after iconname if successful match. iconname scanned is specified in iconname.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="currentPos">The current position.</param>
		/// <param name="newPos">The new position.</param>
		/// <param name="iconName">Name of the icon.</param>
		/// <returns>
		/// bool if match was found and properly skipped, otherwise false.
		/// </returns>
		public static bool SkipFontAwesome(string str, int currentPos, out int newPos, out string iconName)
		{
			newPos = currentPos;
			iconName = string.Empty;
			if(str[currentPos] != '@')
			{
				return false;
			}

			var scanner = new StringScanner(str, currentPos);
			// skip '@'
			scanner.SkipForward(1);
			if(!MatchAnyAndSkip(scanner, new List<string>() { "fa-", "fabrands-", "fasolid-" }))
			{
				// nothing matches
				return false;
			}
			iconName = string.Empty;
			if(!scanner.SkipIdentifier(ref iconName, dashIsValidChar:true))
			{
				// no icon name specified
				return false;
			}
			if(string.IsNullOrWhiteSpace(iconName))
			{
				return false;
			}
			// matched a fontawesome specification
			newPos = scanner.Position;
			return true;
		}


		/// <summary>
		/// Tries to match any of the strings passed in at the current scanner position. If matched it'll skip the matched string on the scanner and return true,
		/// otherwise it won't skip and false
		/// </summary>
		/// <param name="scanner"></param>
		/// <param name="toMatch"></param>
		/// <returns></returns>
		private static bool MatchAnyAndSkip(StringScanner scanner, List<string> toMatch)
		{
			int toSkip = 0;
			foreach(string s in toMatch)
			{
				if(scanner.DoesMatch(s))
				{
					toSkip = s.Length;
					break;
				}
			}

			if(toSkip == 0)
			{
				return false;
			}
			scanner.SkipForward(toSkip);
			return true;
		}
	}
}
