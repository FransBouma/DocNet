// 
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
using System.Web.UI;

namespace MarkdownDeep
{
	// Some block types are only used during block parsing, some
	// are only used during rendering and some are used during both
	internal enum BlockType
	{
		Blank,			// blank line (parse only)
		h1,				// headings (render and parse)
		h2, 
		h3, 
		h4, 
		h5, 
		h6,
		post_h1,		// setext heading lines (parse only)
		post_h2,
		quote,			// block quote (render and parse)
		ol_li,			// list item in an ordered list	(render and parse)
		ul_li,			// list item in an unordered list (render and parse)
		p,				// paragraph (or plain line during parse)
		indent,			// an indented line (parse only)
		hr,				// horizontal rule (render and parse)
		user_break,		// user break
		html,			// html content (render and parse)
		unsafe_html,	// unsafe html that should be encoded
		span,			// an undecorated span of text (used for simple list items 
						//			where content is not wrapped in paragraph tags
		codeblock,		// a code block (render only). If in githubcodeblocks mode, `Data` contains the language name specified after the leading ```. 
		li,				// a list item (render only)
		ol,				// ordered list (render only)
		ul,				// unordered list (render only)
		HtmlTag,		// Data=(HtmlTag), children = content
		Composite,		// Just a list of child blocks
		table_spec,		// A table row specifier eg:  |---: | ---|	`data` = TableSpec reference
		dd,				// definition (render and parse)	`data` = bool true if blank line before
		dt,				// render only
		dl,				// render only
		footnote,		// footnote definition  eg: [^id]   `data` holds the footnote id
		p_footnote,		// paragraph with footnote return link append.  Return link string is in `data`.
		// DocNet extensions
		font_awesome,	// Data is icon name
		alert,			// Data is the alert type specified.
	}

	class Block
	{
		internal Block()
		{

		}

		internal Block(BlockType type)
		{
			BlockType = type;
		}

		public string Content
		{
			get
			{
				switch (BlockType)
				{
					case BlockType.codeblock:
						StringBuilder s = new StringBuilder();
						foreach (var line in Children)
						{
							s.Append(line.Content);
							s.Append('\n');
						}
						return s.ToString();
				}


				if (Buf==null)
					return null;
				else
					return ContentStart == -1 ? Buf : Buf.Substring(ContentStart, ContentLen);
			}
		}

		/// <summary>
		/// Gets the absolute line start: if line start is 0, it's returning the content start.
		/// </summary>
		public int LineStartAbsolute
		{
			get
			{
				return LineStart == 0 ? ContentStart : LineStart;
			}
		}

		internal void RenderChildren(Markdown m, StringBuilder b)
		{
			foreach (var block in Children)
			{
				block.Render(m, b);
			}
		}

		internal void RenderChildrenPlain(Markdown m, StringBuilder b)
		{
			foreach (var block in Children)
			{
				block.RenderPlain(m, b);
			}
		}

		internal string ResolveHeaderID(Markdown m)
		{
			// Already resolved?
			if (this.Data!=null && this.Data is string)
				return (string)this.Data;

			// Approach 1 - PHP Markdown Extra style header id
			int end=ContentEnd;
			string id = Utils.StripHtmlID(Buf, ContentStart, ref end);
			if (id != null)
			{
				ContentEnd = end;
			}
			else
			{
				// Approach 2 - pandoc style header id
				id = m.MakeUniqueHeaderID(Buf, ContentStart, ContentLen);
			}

			this.Data = id;
			return id;
		}

		internal void Render(Markdown m, StringBuilder b)
		{
			switch (BlockType)
			{
				case BlockType.Blank:
					return;

				case BlockType.p:
					m.SpanFormatter.FormatParagraph(b, Buf, ContentStart, ContentLen);
					break;

				case BlockType.span:
					m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
					b.Append("\n");
					break;

				case BlockType.h1:
				case BlockType.h2:
				case BlockType.h3:
				case BlockType.h4:
				case BlockType.h5:
				case BlockType.h6:
					string id = string.Empty;
					if (m.ExtraMode && !m.SafeMode)
					{
						b.Append("<" + BlockType.ToString());
						id = ResolveHeaderID(m);
						if (!String.IsNullOrEmpty(id))
						{
							b.Append(" id=\"");
							b.Append(id);
							b.Append("\">");
						}
						else
						{
							b.Append(">");
						}
					}
					else
					{
						b.Append("<" + BlockType.ToString() + ">");
					}
					if(m.DocNetMode && BlockType == BlockType.h2 && !string.IsNullOrWhiteSpace(id))
					{
						// collect h2 id + text in collector
						var h2ContentSb = new StringBuilder();
						m.SpanFormatter.Format(h2ContentSb, Buf, ContentStart, ContentLen);
						var h2ContentAsString = h2ContentSb.ToString();
						b.Append(h2ContentAsString);
						m.CreatedH2IdCollector.Add(new Tuple<string, string>(id, h2ContentAsString));
					}
					else
					{
						m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
					}
					b.Append("</" + BlockType.ToString() + ">\n");
					break;

				case BlockType.hr:
					b.Append("<hr />\n");
					return;
				
				case BlockType.user_break:
					return;

				case BlockType.ol_li:
				case BlockType.ul_li:
					b.Append("<li>");
					m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
					b.Append("</li>\n");
					break;

				case BlockType.dd:
					b.Append("<dd>");
					if (Children != null)
					{
						b.Append("\n");
						RenderChildren(m, b);
					}
					else
						m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
					b.Append("</dd>\n");
					break;

				case BlockType.dt:
				{
					if (Children == null)
					{
						foreach (var l in Content.Split('\n'))
						{
							b.Append("<dt>");
							m.SpanFormatter.Format(b, l.Trim());
							b.Append("</dt>\n");
						}
					}
					else
					{
						b.Append("<dt>\n");
						RenderChildren(m, b);
						b.Append("</dt>\n");
					}
					break;
				}

				case BlockType.dl:
					b.Append("<dl>\n");
					RenderChildren(m, b);
					b.Append("</dl>\n");
					return;

				case BlockType.html:
					b.Append(Buf, ContentStart, ContentLen);
					return;

				case BlockType.unsafe_html:
					m.HtmlEncode(b, Buf, ContentStart, ContentLen);
					return;

				case BlockType.codeblock:
					if(m.FormatCodeBlockFunc == null)
					{
						var dataArgument = this.Data as string ?? string.Empty;
						string tagSuffix = "</code></pre>\n\n";

						if(m.GitHubCodeBlocks && !string.IsNullOrWhiteSpace(dataArgument))
						{
							if(dataArgument == "nohighlight")
							{
								b.Append("<pre class=\"nocode\">");
								tagSuffix = "</pre>";
							}
							else
							{
								b.AppendFormat("<pre><code class=\"{0}\">", dataArgument);
							}
						}
						else
						{
							b.Append("<pre><code>");
						}
						foreach(var line in Children)
						{
							m.HtmlEncodeAndConvertTabsToSpaces(b, line.Buf, line.ContentStart, line.ContentLen);
							b.Append("\n");
						}
						b.Append(tagSuffix);
					}
					else
					{
						var sb = new StringBuilder();
						foreach(var line in Children)
						{
							m.HtmlEncodeAndConvertTabsToSpaces(sb, line.Buf, line.ContentStart, line.ContentLen);
							sb.Append("\n");
						}
						b.Append(m.FormatCodeBlockFunc(m, sb.ToString()));
					}
					return;

				case BlockType.quote:
					b.Append("<blockquote>\n");
					RenderChildren(m, b);
					b.Append("</blockquote>\n");
					return;

				case BlockType.li:
					b.Append("<li>\n");
					RenderChildren(m, b);
					b.Append("</li>\n");
					return;

				case BlockType.ol:
					b.Append("<ol>\n");
					RenderChildren(m, b);
					b.Append("</ol>\n");
					return;

				case BlockType.ul:
					b.Append("<ul>\n");
					RenderChildren(m, b);
					b.Append("</ul>\n");
					return;

				case BlockType.HtmlTag:
					var tag = (HtmlTag)Data;

					// Prepare special tags
					var name=tag.name.ToLowerInvariant();
					if (name == "a")
					{
						m.OnPrepareLink(tag);
					}
					else if (name == "img")
					{
						m.OnPrepareImage(tag, m.RenderingTitledImage);
					}

					tag.RenderOpening(b);
					b.Append("\n");
					RenderChildren(m, b);
					tag.RenderClosing(b);
					b.Append("\n");
					return;

				case BlockType.Composite:
				case BlockType.footnote:
					RenderChildren(m, b);
					return;

				case BlockType.table_spec:
					((TableSpec)Data).Render(m, b);
					break;

				case BlockType.p_footnote:
					b.Append("<p>");
					if (ContentLen > 0)
					{
						m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
						b.Append("&nbsp;");
					}
					b.Append((string)Data);
					b.Append("</p>\n");
					break;
// DocNet Extensions
				case BlockType.font_awesome:
					if(m.DocNetMode)
					{
						b.Append("<i class=\"fa fa-");
						b.Append(this.Data as string ?? string.Empty);
						b.Append("\"></i>");
					}
					break;
				case BlockType.alert:
					if(m.DocNetMode)
					{
						RenderAlert(m, b);
					}
					break;

// End DocNet Extensions
				default:
					b.Append("<" + BlockType.ToString() + ">");
					m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
					b.Append("</" + BlockType.ToString() + ">\n");
					break;
			}
		}


		internal void RenderPlain(Markdown m, StringBuilder b)
		{
			switch (BlockType)
			{
				case BlockType.Blank:
					return;

				case BlockType.p:
				case BlockType.span:
					m.SpanFormatter.FormatPlain(b, Buf, ContentStart, ContentLen);
					b.Append(" ");
					break;

				case BlockType.h1:
				case BlockType.h2:
				case BlockType.h3:
				case BlockType.h4:
				case BlockType.h5:
				case BlockType.h6:
					m.SpanFormatter.FormatPlain(b, Buf, ContentStart, ContentLen);
					b.Append(" - ");
					break;


				case BlockType.ol_li:
				case BlockType.ul_li:
					b.Append("* ");
					m.SpanFormatter.FormatPlain(b, Buf, ContentStart, ContentLen);
					b.Append(" ");
					break;

				case BlockType.dd:
					if (Children != null)
					{
						b.Append("\n");
						RenderChildrenPlain(m, b);
					}
					else
						m.SpanFormatter.FormatPlain(b, Buf, ContentStart, ContentLen);
					break;

				case BlockType.dt:
					{
						if (Children == null)
						{
							foreach (var l in Content.Split('\n'))
							{
								var str = l.Trim();
								m.SpanFormatter.FormatPlain(b, str, 0, str.Length);
							}
						}
						else
						{
							RenderChildrenPlain(m, b);
						}
						break;
					}

				case BlockType.dl:
					RenderChildrenPlain(m, b);
					return;

				case BlockType.codeblock:
					foreach (var line in Children)
					{
						b.Append(line.Buf, line.ContentStart, line.ContentLen);
						b.Append(" ");
					}
					return;

				case BlockType.quote:
				case BlockType.li:
				case BlockType.ol:
				case BlockType.ul:
				case BlockType.HtmlTag:
					RenderChildrenPlain(m, b);
					return;
			}
		}

		public void RevertToPlain()
		{
			BlockType = BlockType.p;
			ContentStart = LineStart;
			ContentLen = LineLen;
		}


		private void RenderAlert(Markdown m, StringBuilder b)
		{
			var alertType = this.Data as string;
			if(string.IsNullOrWhiteSpace(alertType))
			{
				alertType = "info";
			}
			string title = string.Empty;
			string faIconName = string.Empty;
			switch(alertType)
			{
				case "danger":
					title = "Danger!";
					faIconName = "times-circle";
					break;
				case "warning":
					title = "Warning!";
					faIconName = "warning";
					break;
				case "neutral":
				case "info":
					title = "Info";
					faIconName = "info-circle";
					break;
			}
			b.Append("<div class=\"alert alert-");
			b.Append(alertType);
			b.Append("\"><span class=\"alert-title\"><i class=\"fa fa-");
			b.Append(faIconName);
			b.Append("\"></i>");
			b.Append(title);
			b.Append("</span>");
			RenderChildren(m, b);
			b.Append("</div>");
		}

		#region Properties
		public int ContentEnd
		{
			get
			{
				return ContentStart + ContentLen;
			}
			set
			{
				ContentLen = value - ContentStart;
			}
		}

		// Count the leading spaces on a block
		// Used by list item evaluation to determine indent levels
		// irrespective of indent line type.
		public int LeadingSpaces
		{
			get
			{
				int count = 0;
				for (int i = LineStart; i < LineStart + LineLen; i++)
				{
					if (Buf[i] == ' ')
					{
						count++;
					}
					else
					{
						break;
					}
				}
				return count;
			}
		}

		public override string ToString()
		{
			string c = Content;
			return BlockType.ToString() + " - " + (c==null ? "<null>" : c);
		}

		public Block CopyFrom(Block other)
		{
			BlockType = other.BlockType;
			Buf = other.Buf;
			ContentStart = other.ContentStart;
			ContentLen = other.ContentLen;
			LineStart = other.LineStart;
			LineLen = other.LineLen;
			return this;
		}


		internal BlockType BlockType { get; set; }
		internal string Buf { get; set; }
		internal int ContentStart { get; set; }
		internal int ContentLen { get; set; }
		internal int LineStart { get; set; }
		internal int LineLen { get; set; }
		internal object Data { get; set; }			// content depends on block type
		internal List<Block> Children { get; set; }

		#endregion
	}
}
