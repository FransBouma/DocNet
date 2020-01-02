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

namespace MarkdownDeep
{
	public class LinkDefinition
	{
		public LinkDefinition(string id) : this(id, string.Empty, string.Empty)
		{
		}

		public LinkDefinition(string id, string url) : this(id, url, string.Empty)
		{
		}

		public LinkDefinition(string id, string url, string title)
		{
			this.Id = id;
			this.Url = url;
			this.Title = title;
		}


		internal void RenderLink(Markdown m, StringBuilder b, string link_text, List<string> specialAttributes)
		{
			if (this.Url.StartsWith("mailto:"))
			{
				b.Append("<a href=\"");
				Utils.HtmlRandomize(b, this.Url);
				b.Append('\"');
				if (!String.IsNullOrEmpty(this.Title))
				{
					b.Append(" title=\"");
					Utils.SmartHtmlEncodeAmpsAndAngles(b, this.Title);
					b.Append('\"');
				}
				b.Append('>');
				Utils.HtmlRandomize(b, link_text);
				b.Append("</a>");
			}
			else
			{
				HtmlTag tag = new HtmlTag("a");

				var url = this.Url;

				if (m.DocNetMode && m.ConvertLocalLinks)
				{
					// A few requirements before we can convert local links:
					//   1. Link contains .md
					//   2. Link is relative
					//   3. Link is included in the index
					var index = url.LastIndexOf(".md", StringComparison.OrdinalIgnoreCase);
					if (index >= 0)
					{
						var linkProcessor = m.LocalLinkProcessor;
						if (linkProcessor != null)
						{
							url = linkProcessor(url);
						}
						if (Uri.TryCreate(url, UriKind.Relative, out var uri))
						{
							url = String.Concat(url.Substring(0, index), ".htm", url.Substring(index + ".md".Length));
						}
					}
				}

				// encode url
				StringBuilder sb = m.GetStringBuilder();
				Utils.SmartHtmlEncodeAmpsAndAngles(sb, url);
				tag.attributes["href"] = sb.ToString();

				// encode title
				if (!String.IsNullOrEmpty(this.Title))
				{
					sb.Length = 0;
					Utils.SmartHtmlEncodeAmpsAndAngles(sb, this.Title);
					tag.attributes["title"] = sb.ToString();
				}

				if (specialAttributes.Any())
				{
					LinkDefinition.HandleSpecialAttributes(specialAttributes, sb, tag);
				}

				// Do user processing
				m.OnPrepareLink(tag);

				// Render the opening tag
				tag.RenderOpening(b);

				b.Append(link_text);      // Link text already escaped by SpanFormatter
				b.Append("</a>");
			}
		}


		internal void RenderImg(Markdown m, StringBuilder b, string alt_text, List<string> specialAttributes)
		{
			HtmlTag tag = new HtmlTag("img");

			// encode url
			StringBuilder sb = m.GetStringBuilder();
			Utils.SmartHtmlEncodeAmpsAndAngles(sb, Url);
			tag.attributes["src"] = sb.ToString();

			// encode alt text
			if (!String.IsNullOrEmpty(alt_text))
			{
				sb.Length = 0;
				Utils.SmartHtmlEncodeAmpsAndAngles(sb, alt_text);
				tag.attributes["alt"] = sb.ToString();
			}

			// encode title
			if (!String.IsNullOrEmpty(Title))
			{
				sb.Length = 0;
				Utils.SmartHtmlEncodeAmpsAndAngles(sb, Title);
				tag.attributes["title"] = sb.ToString();
			}
			if (specialAttributes.Any())
			{
				LinkDefinition.HandleSpecialAttributes(specialAttributes, sb, tag);
			}
			tag.closed = true;

			m.OnPrepareImage(tag, m.RenderingTitledImage);

			tag.RenderOpening(b);
		}


		// Parse a link definition from a string (used by test cases)
		internal static LinkDefinition ParseLinkDefinition(string str, bool ExtraMode)
		{
			StringScanner p = new StringScanner(str);
			return ParseLinkDefinitionInternal(p, ExtraMode);
		}

		// Parse a link definition
		internal static LinkDefinition ParseLinkDefinition(StringScanner p, bool ExtraMode)
		{
			int savepos = p.Position;
			var l = ParseLinkDefinitionInternal(p, ExtraMode);
			if (l == null)
				p.Position = savepos;
			return l;

		}

		internal static LinkDefinition ParseLinkDefinitionInternal(StringScanner p, bool ExtraMode)
		{
			// Skip leading white space
			p.SkipWhitespace();

			// Must start with an opening square bracket
			if (!p.SkipChar('['))
				return null;

			// Extract the id
			p.Mark();
			if (!p.Find(']'))
				return null;
			string id = p.Extract();
			if (id.Length == 0)
				return null;
			if (!p.SkipString("]:"))
				return null;

			// Parse the url and title
			var link = ParseLinkTarget(p, id, ExtraMode);

			// and trailing whitespace
			p.SkipLinespace();

			// Trailing crap, not a valid link reference...
			if (!p.Eol)
				return null;

			return link;
		}

		// Parse just the link target
		// For reference link definition, this is the bit after "[id]: thisbit"
		// For inline link, this is the bit in the parens: [link text](thisbit)
		internal static LinkDefinition ParseLinkTarget(StringScanner p, string id, bool ExtraMode)
		{
			// Skip whitespace
			p.SkipWhitespace();

			// End of string?
			if (p.Eol)
				return null;

			// Create the link definition
			var r = new LinkDefinition(id);

			// Is the url enclosed in angle brackets
			if (p.SkipChar('<'))
			{
				// Extract the url
				p.Mark();

				// Find end of the url
				while (p.Current != '>')
				{
					if (p.Eof)
						return null;
					p.SkipEscapableChar(ExtraMode);
				}

				string url = p.Extract();
				if (!p.SkipChar('>'))
					return null;

				// Unescape it
				r.Url = Utils.UnescapeString(url.Trim(), ExtraMode);
			}
			else
			{
				// Find end of the url
				p.Mark();
				int paren_depth = 1;
				while (!p.Eol)
				{
					char ch = p.Current;
					if (char.IsWhiteSpace(ch))
						break;
					if (id == null)
					{
						if (ch == '(')
							paren_depth++;
						else if (ch == ')')
						{
							paren_depth--;
							if (paren_depth == 0)
								break;
						}
					}

					p.SkipEscapableChar(ExtraMode);
				}

				r.Url = Utils.UnescapeString(p.Extract().Trim(), ExtraMode);
			}

			p.SkipLinespace();

			// End of inline target
			if (p.DoesMatch(')'))
				return r;

			bool bOnNewLine = p.Eol;
			int posLineEnd = p.Position;
			if (p.Eol)
			{
				p.SkipEol();
				p.SkipLinespace();
			}

			// Work out what the title is delimited with
			char delim;
			switch (p.Current)
			{
				case '\'':
				case '\"':
					delim = p.Current;
					break;

				case '(':
					delim = ')';
					break;

				default:
					if (bOnNewLine)
					{
						p.Position = posLineEnd;
						return r;
					}
					else
						return null;
			}

			// Skip the opening title delimiter
			p.SkipForward(1);

			// Find the end of the title
			p.Mark();
			while (true)
			{
				if (p.Eol)
					return null;

				if (p.Current == delim)
				{

					if (delim != ')')
					{
						int savepos = p.Position;

						// Check for embedded quotes in title

						// Skip the quote and any trailing whitespace
						p.SkipForward(1);
						p.SkipLinespace();

						// Next we expect either the end of the line for a link definition
						// or the close bracket for an inline link
						if ((id == null && p.Current != ')') ||
							(id != null && !p.Eol))
						{
							continue;
						}

						p.Position = savepos;
					}

					// End of title
					break;
				}

				p.SkipEscapableChar(ExtraMode);
			}

			// Store the title
			r.Title = Utils.UnescapeString(p.Extract(), ExtraMode);

			// Skip closing quote
			p.SkipForward(1);

			// Done!
			return r;
		}


		private static void HandleSpecialAttributes(List<string> specialAttributes, StringBuilder sb, HtmlTag tag)
		{
			string id = specialAttributes.FirstOrDefault(s => s.StartsWith("#"));
			if (id != null && id.Length > 1)
			{
				sb.Length = 0;
				Utils.SmartHtmlEncodeAmpsAndAngles(sb, id.Substring(1));
				tag.attributes["id"] = sb.ToString();
			}
			var cssClasses = new List<string>();
			foreach (var cssClass in specialAttributes.Where(s => s.StartsWith(".") && s.Length > 1))
			{
				sb.Length = 0;
				Utils.SmartHtmlEncodeAmpsAndAngles(sb, cssClass.Substring(1));
				cssClasses.Add(sb.ToString());
			}
			if (cssClasses.Any())
			{
				tag.attributes["class"] = string.Join(" ", cssClasses.ToArray());
			}
			foreach (var nameValuePair in specialAttributes.Where(s => s.Contains("=") && s.Length > 2 && !s.StartsWith(".") && !s.StartsWith("#")))
			{
				var pair = nameValuePair.Split('=');
				if (pair.Length == 2)
				{
					sb.Length = 0;
					Utils.SmartHtmlEncodeAmpsAndAngles(sb, pair[0]);
					var key = sb.ToString();
					sb.Length = 0;
					Utils.SmartHtmlEncodeAmpsAndAngles(sb, pair[1]);
					var value = sb.ToString();
					tag.attributes[key] = value;
				}
			}
		}


		#region Properties
		public string Id { get; set; }
		public string Url { get; set; }
		public string Title { get; set; }
		#endregion
	}
}
