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
using System.IO;
using System.Linq;
using System.Text;
using Projbook.Extension;
using Projbook.Extension.CSharpExtractor;
using Projbook.Extension.Spi;
using Projbook.Extension.XmlExtractor;

namespace MarkdownDeep
{
	public class BlockProcessor : StringScanner
	{
		#region Enums

		internal enum MarkdownInHtmlMode
		{
			NA,         // No markdown attribute on the tag
			Block,      // markdown=1 or markdown=block
			Span,       // markdown=1 or markdown=span
			Deep,       // markdown=deep - recursive block mode
			Off,        // Markdown="something else"
		}

		#endregion

		public BlockProcessor(Markdown m, bool MarkdownInHtml)
		{
			m_markdown = m;
			m_bMarkdownInHtml = MarkdownInHtml;
			m_parentType = BlockType.Blank;
		}

		internal BlockProcessor(Markdown m, bool MarkdownInHtml, BlockType parentType)
		{
			m_markdown = m;
			m_bMarkdownInHtml = MarkdownInHtml;
			m_parentType = parentType;
		}

		internal List<Block> Process(string str)
		{
			return ScanLines(str);
		}

		internal List<Block> ScanLines(string str)
		{
			// Reset string scanner
			Reset(str);
			return ScanLines();
		}

		internal List<Block> ScanLines(string str, int start, int len)
		{
			Reset(str, start, len);
			return ScanLines();
		}

		internal bool StartTable(TableSpec spec, List<Block> lines)
		{
			// Mustn't have more than 1 preceeding line
			if (lines.Count > 1)
				return false;

			// Rewind, parse the header row then fast forward back to current pos
			if (lines.Count == 1)
			{
				int savepos = Position;
				Position = lines[0].LineStart;
				spec.Headers = spec.ParseRow(this);
				if (spec.Headers == null)
					return false;
				Position = savepos;
				lines.Clear();
			}

			// Parse all rows
			while (true)
			{
				int savepos = Position;

				var row=spec.ParseRow(this);
				if (row!=null)
				{
					spec.Rows.Add(row);
					continue;
				}

				Position = savepos;
				break;
			}

			return true;
		}

		internal List<Block> ScanLines()
		{
			// The final set of blocks will be collected here
			var blocks = new List<Block>();

			// The current paragraph/list/codeblock etc will be accumulated here
			// before being collapsed into a block and store in above `blocks` list
			var lines = new List<Block>();

			// Add all blocks
			BlockType PrevBlockType = BlockType.unsafe_html;
			while (!Eof)
			{
				// Remember if the previous line was blank
				bool bPreviousBlank = PrevBlockType == BlockType.Blank;

				// Get the next block
				var b = EvaluateLine();
				PrevBlockType = b.BlockType;

				// For dd blocks, we need to know if it was preceeded by a blank line
				// so store that fact as the block's data.
				if (b.BlockType == BlockType.dd)
				{
					b.Data = bPreviousBlank;
				}


				// SetExt header?
				if (b.BlockType == BlockType.post_h1 || b.BlockType == BlockType.post_h2)
				{
					if (lines.Count > 0)
					{
						// Remove the previous line and collapse the current paragraph
						var prevline = lines.Pop();
						CollapseLines(blocks, lines);

						// If previous line was blank, 
						if (prevline.BlockType != BlockType.Blank)
						{
							// Convert the previous line to a heading and add to block list
							prevline.RevertToPlain();
							prevline.BlockType = b.BlockType == BlockType.post_h1 ? BlockType.h1 : BlockType.h2;
							blocks.Add(prevline);
							continue;
						}
					}

					// Couldn't apply setext header to a previous line

					if (b.BlockType == BlockType.post_h1)
					{
						// `===` gets converted to normal paragraph
						b.RevertToPlain();
						lines.Add(b);
					}
					else
					{
						// `---` gets converted to hr
						if (b.ContentLen >= 3)
						{
							b.BlockType = BlockType.hr;
							blocks.Add(b);
						}
						else
						{
							b.RevertToPlain();
							lines.Add(b);
						}
					}

					continue;
				}


				// Work out the current paragraph type
				BlockType currentBlockType = lines.Count > 0 ? lines[0].BlockType : BlockType.Blank;

				// Starting a table?
				if (b.BlockType == BlockType.table_spec)
				{
					// Get the table spec, save position
					TableSpec spec = (TableSpec)b.Data;
					int savepos = Position;
					if (!StartTable(spec, lines))
					{
						// Not a table, revert the tablespec row to plain,
						// fast forward back to where we were up to and continue
						// on as if nothing happened
						Position = savepos;
						b.RevertToPlain();
					}
					else
					{
						blocks.Add(b);
						continue;
					}
				}

				// Process this line
				switch (b.BlockType)
				{
					case BlockType.Blank:
						switch (currentBlockType)
						{
							case BlockType.Blank:
								FreeBlock(b);
								break;

							case BlockType.p:
								CollapseLines(blocks, lines);
								FreeBlock(b);
								break;

							case BlockType.quote:
							case BlockType.ol_li:
							case BlockType.ul_li:
							case BlockType.dd:
							case BlockType.footnote:
							case BlockType.indent:
								lines.Add(b);
								break;

							default:
								System.Diagnostics.Debug.Assert(false);
								break;
						}
						break;

					case BlockType.p:
						switch (currentBlockType)
						{
							case BlockType.Blank:
							case BlockType.p:
								lines.Add(b);
								break;

							case BlockType.quote:
							case BlockType.ol_li:
							case BlockType.ul_li:
							case BlockType.dd:
							case BlockType.footnote:
								var prevline = lines.Last();
								if (prevline.BlockType == BlockType.Blank)
								{
									CollapseLines(blocks, lines);
									lines.Add(b);
								}
								else
								{
									lines.Add(b);
								}
								break;

							case BlockType.indent:
								CollapseLines(blocks, lines);
								lines.Add(b);
								break;

							default:
								System.Diagnostics.Debug.Assert(false);
								break;
						}
						break;

					case BlockType.indent:
						switch (currentBlockType)
						{
							case BlockType.Blank:
								// Start a code block
								lines.Add(b);
								break;

							case BlockType.p:
							case BlockType.quote:
								var prevline = lines.Last();
								if (prevline.BlockType == BlockType.Blank)
								{
									// Start a code block after a paragraph
									CollapseLines(blocks, lines);
									lines.Add(b);
								}
								else
								{
									// indented line in paragraph, just continue it
									b.RevertToPlain();
									lines.Add(b);
								}
								break;


							case BlockType.ol_li:
							case BlockType.ul_li:
							case BlockType.dd:
							case BlockType.footnote:
							case BlockType.indent:
								lines.Add(b);
								break;

							default:
								System.Diagnostics.Debug.Assert(false);
								break;
						}
						break;

					case BlockType.quote:
						if (currentBlockType != BlockType.quote)
						{
							CollapseLines(blocks, lines);
						}
						lines.Add(b);
						break;

					case BlockType.ol_li:
					case BlockType.ul_li:
						switch (currentBlockType)
						{
							case BlockType.Blank:
								lines.Add(b);
								break;

							case BlockType.p:
							case BlockType.quote:
								var prevline = lines.Last();
								if (prevline.BlockType == BlockType.Blank || m_parentType==BlockType.ol_li || m_parentType==BlockType.ul_li || m_parentType==BlockType.dd)
								{
									// List starting after blank line after paragraph or quote
									CollapseLines(blocks, lines);
									lines.Add(b);
								}
								else
								{
									// List's can't start in middle of a paragraph
									b.RevertToPlain();
									lines.Add(b);
								}
								break;

							case BlockType.ol_li:
							case BlockType.ul_li:
								if (b.BlockType!=BlockType.ol_li && b.BlockType!=BlockType.ul_li)
								{
									CollapseLines(blocks, lines);
								}
								lines.Add(b);
								break;

							case BlockType.dd:
							case BlockType.footnote:
								if (b.BlockType != currentBlockType)
								{
									CollapseLines(blocks, lines);
								}
								lines.Add(b);
								break;

							case BlockType.indent:
								// List after code block
								CollapseLines(blocks, lines);
								lines.Add(b);
								break;
						}
						break;

					case BlockType.dd:
					case BlockType.footnote:
						switch (currentBlockType)
						{
							case BlockType.Blank:
							case BlockType.p:
							case BlockType.dd:
							case BlockType.footnote:
								CollapseLines(blocks, lines);
								lines.Add(b);
								break;

							default:
								b.RevertToPlain();
								lines.Add(b);
								break;
						}
						break;

					default:
						CollapseLines(blocks, lines);
						blocks.Add(b);
						break;
				}
			}

			CollapseLines(blocks, lines);

			if (m_markdown.ExtraMode)
			{
				BuildDefinitionLists(blocks);
			}

			return blocks;
		}

		internal Block CreateBlock()
		{
			return m_markdown.CreateBlock();
		}

		internal void FreeBlock(Block b)
		{
			m_markdown.FreeBlock(b);
		}

		internal void FreeBlocks(List<Block> blocks)
		{
			foreach (var b in blocks)
				FreeBlock(b);
			blocks.Clear();
		}

		internal string RenderLines(List<Block> lines)
		{
			StringBuilder b = m_markdown.GetStringBuilder();
			foreach (var l in lines)
			{
				b.Append(l.Buf, l.ContentStart, l.ContentLen);
				b.Append('\n');
			}
			return b.ToString();
		}

		internal void CollapseLines(List<Block> blocks, List<Block> lines)
		{
			// Remove trailing blank lines
			while (lines.Count>0 && lines.Last().BlockType == BlockType.Blank)
			{
				FreeBlock(lines.Pop());
			}

			// Quit if empty
			if (lines.Count == 0)
			{
				return;
			}


			// What sort of block?
			switch (lines[0].BlockType)
			{
				case BlockType.p:
				{
					// Collapse all lines into a single paragraph
					var para = CreateBlock();
					para.BlockType = BlockType.p;
					para.Buf = lines[0].Buf;
					para.ContentStart = lines[0].ContentStart;
					para.ContentEnd = lines.Last().ContentEnd;
					blocks.Add(para);
					FreeBlocks(lines);
					break;
				}

				case BlockType.quote:
				{
					// Create a new quote block
					var quote = new Block(BlockType.quote);
					quote.Children = new BlockProcessor(m_markdown, m_bMarkdownInHtml, BlockType.quote).Process(RenderLines(lines));
					FreeBlocks(lines);
					blocks.Add(quote);
					break;
				}

				case BlockType.ol_li:
				case BlockType.ul_li:
					blocks.Add(BuildList(lines));
					break;

				case BlockType.dd:
					if (blocks.Count > 0)
					{
						var prev=blocks[blocks.Count-1];
						switch (prev.BlockType)
						{
							case BlockType.p:
								prev.BlockType = BlockType.dt;
								break;

							case BlockType.dd:
								break;

							default:
								var wrapper = CreateBlock();
								wrapper.BlockType = BlockType.dt;
								wrapper.Children = new List<Block>();
								wrapper.Children.Add(prev);
								blocks.Pop();
								blocks.Add(wrapper);
								break;
						}

					}
					blocks.Add(BuildDefinition(lines));
					break;

				case BlockType.footnote:
					m_markdown.AddFootnote(BuildFootnote(lines));
					break;

				case BlockType.indent:
				{
					var codeblock = new Block(BlockType.codeblock);
					/*
					if (m_markdown.FormatCodeBlockAttributes != null)
					{
						// Does the line first line look like a syntax specifier
						var firstline = lines[0].Content;
						if (firstline.StartsWith("{{") && firstline.EndsWith("}}"))
						{
							codeblock.data = firstline.Substring(2, firstline.Length - 4);
							lines.RemoveAt(0);
						}
					}
					 */
					codeblock.Children = new List<Block>();
					codeblock.Children.AddRange(lines);
					blocks.Add(codeblock);
					lines.Clear();
					break;
				}
			}
		}


		Block EvaluateLine()
		{
			// Create a block
			Block b=CreateBlock();

			// Store line start
			b.LineStart=Position;
			b.Buf=Input;

			// Scan the line
			b.ContentStart = Position;
			b.ContentLen = -1;
			b.BlockType=EvaluateLine(b);

			// If end of line not returned, do it automatically
			if (b.ContentLen < 0)
			{
				// Move to end of line
				SkipToEol();
				b.ContentLen = Position - b.ContentStart;
			}

			// Setup line length
			b.LineLen=Position-b.LineStart;

			// Next line
			SkipEol();

			// Create block
			return b;
		}

		BlockType EvaluateLine(Block b)
		{
			// Empty line?
			if (Eol)
				return BlockType.Blank;

			// Save start of line position
			int line_start= Position;

			// ## Heading ##		
			char ch=Current;
			if (ch == '#')
			{
				// Work out heading level
				int level = 1;
				SkipForward(1);
				while (Current == '#')
				{
					level++;
					SkipForward(1);
				}

				// Limit of 6
				if (level > 6)
					level = 6;

				// Skip any whitespace
				SkipLinespace();

				// Save start position
				b.ContentStart = Position;

				// Jump to end
				SkipToEol();

				// In extra mode, check for a trailing HTML ID
				if (m_markdown.ExtraMode && !m_markdown.SafeMode)
				{
					int end=Position;
					string strID = Utils.StripHtmlID(Input, b.ContentStart, ref end);
					if (strID!=null)
					{
						b.Data = strID;
						Position = end;
					}
				}

				// Rewind over trailing hashes
				while (Position>b.ContentStart && CharAtOffset(-1) == '#')
				{
					SkipForward(-1);
				}

				// Rewind over trailing spaces
				while (Position>b.ContentStart && char.IsWhiteSpace(CharAtOffset(-1)))
				{
					SkipForward(-1);
				}

				// Create the heading block
				b.ContentEnd = Position;

				SkipToEol();
				return BlockType.h1 + (level - 1);
			}

			// Check for entire line as - or = for setext h1 and h2
			if (ch=='-' || ch=='=')
			{
				// Skip all matching characters
				char chType = ch;
				while (Current==chType)
				{
					SkipForward(1);
				}

				// Trailing whitespace allowed
				SkipLinespace();

				// If not at eol, must have found something other than setext header
				if (Eol)
				{
					return chType == '=' ? BlockType.post_h1 : BlockType.post_h2;
				}

				Position = line_start;
			}

			// MarkdownExtra Table row indicator?
			if (m_markdown.ExtraMode)
			{
				TableSpec spec = TableSpec.Parse(this);
				if (spec!=null)
				{
					b.Data = spec;
					return BlockType.table_spec;
				}

				Position = line_start;
			}

			// Fenced code blocks?
			if((m_markdown.ExtraMode && (ch == '~' || ch=='`')) || (m_markdown.GitHubCodeBlocks && (ch=='`')))
			{
				if (ProcessFencedCodeBlock(b))
					return b.BlockType;

				// Rewind
				Position = line_start;
			}

			// Scan the leading whitespace, remembering how many spaces and where the first tab is
			int tabPos = -1;
			int leadingSpaces = 0;
			while (!Eol)
			{
				if (Current == ' ')
				{
					if (tabPos < 0)
						leadingSpaces++;
				}
				else if (Current == '\t')
				{
					if (tabPos < 0)
						tabPos = Position;
				}
				else
				{
					// Something else, get out
					break;
				}
				SkipForward(1);
			}

			// Blank line?
			if (Eol)
			{
				b.ContentEnd = b.ContentStart;
				return BlockType.Blank;
			}

			// 4 leading spaces?
			if (leadingSpaces >= 4)
			{
				b.ContentStart = line_start + 4;
				return BlockType.indent;
			}

			// Tab in the first 4 characters?
			if (tabPos >= 0 && tabPos - line_start<4)
			{
				b.ContentStart = tabPos + 1;
				return BlockType.indent;
			}

			// Treat start of line as after leading whitespace
			b.ContentStart = Position;

			// Get the next character
			ch = Current;

			// Html block?
			if (ch == '<')
			{
				// Scan html block
				if (ScanHtml(b))
					return b.BlockType;

				// Rewind
				Position = b.ContentStart;
			}

			// Block quotes start with '>' and have one space or one tab following
			if (ch == '>')
			{
				// Block quote followed by space
				if (IsLineSpace(CharAtOffset(1)))
				{
					// Skip it and create quote block
					SkipForward(2);
					b.ContentStart = Position;
					return BlockType.quote;
				}

				SkipForward(1);
				b.ContentStart = Position;
				return BlockType.quote;
			}

			// Horizontal rule - a line consisting of 3 or more '-', '_' or '*' with optional spaces and nothing else
			if (ch == '-' || ch == '_' || ch == '*')
			{
				int count = 0;
				while (!Eol)
				{
					char chType = Current;
					if (Current == ch)
					{
						count++;
						SkipForward(1);
						continue;
					}

					if (IsLineSpace(Current))
					{
						SkipForward(1);
						continue;
					}

					break;
				}

				if (Eol && count >= 3)
				{
					if (m_markdown.UserBreaks)
						return BlockType.user_break;
					else 
						return BlockType.hr;
				}

				// Rewind
				Position = b.ContentStart;
			}

			// Abbreviation definition?
			if (m_markdown.ExtraMode && ch == '*' && CharAtOffset(1) == '[')
			{
				SkipForward(2);
				SkipLinespace();

				Mark();
				while (!Eol && Current != ']')
				{
					SkipForward(1);
				}

				var abbr = Extract().Trim();
				if (Current == ']' && CharAtOffset(1) == ':' && !string.IsNullOrEmpty(abbr))
				{
					SkipForward(2);
					SkipLinespace();

					Mark();

					SkipToEol();

					var title = Extract();

					m_markdown.AddAbbreviation(abbr, title);

					return BlockType.Blank;
				}

				Position = b.ContentStart;
			}

			// Unordered list
			if ((ch == '*' || ch == '+' || ch == '-') && IsLineSpace(CharAtOffset(1)))
			{
				// Skip it
				SkipForward(1);
				SkipLinespace();
				b.ContentStart = Position;
				return BlockType.ul_li;
			}

			// Definition
			if (ch == ':' && m_markdown.ExtraMode && IsLineSpace(CharAtOffset(1)))
			{
				SkipForward(1);
				SkipLinespace();
				b.ContentStart = Position;
				return BlockType.dd;
			}

			// Ordered list
			if (char.IsDigit(ch))
			{
				// Ordered list?  A line starting with one or more digits, followed by a '.' and a space or tab

				// Skip all digits
				SkipForward(1);
				while (char.IsDigit(Current))
					SkipForward(1);

				if (SkipChar('.') && SkipLinespace())
				{
					b.ContentStart = Position;
					return BlockType.ol_li;
				}

				Position=b.ContentStart;
			}

			// Reference link definition?
			if (ch == '[')
			{
				// Footnote definition?
				if (m_markdown.ExtraMode && CharAtOffset(1) == '^')
				{
					var savepos = Position;

					SkipForward(2);

					string id;
					if (SkipFootnoteID(out id) && SkipChar(']') && SkipChar(':'))
					{
						SkipLinespace();
						b.ContentStart = Position;
						b.Data = id;
						return BlockType.footnote;
					}

					Position = savepos;
				}

				// Parse a link definition
				LinkDefinition l = LinkDefinition.ParseLinkDefinition(this, m_markdown.ExtraMode);
				if (l!=null)
				{
					m_markdown.AddLinkDefinition(l);
					return BlockType.Blank;
				}
			}

			// DocNet '@' extensions
			if(ch == '@' && m_markdown.DocNetMode)
			{
				if(HandleDocNetExtension(b))
				{
					return b.BlockType;
				}

				// Not valid, Rewind
				Position = b.ContentStart;
			}

			// Nothing special
			return BlockType.p;
		}

		internal MarkdownInHtmlMode GetMarkdownMode(HtmlTag tag)
		{
			// Get the markdown attribute
			string strMarkdownMode;
			if (!m_markdown.ExtraMode || !tag.attributes.TryGetValue("markdown", out strMarkdownMode))
			{
				if (m_bMarkdownInHtml)
					return MarkdownInHtmlMode.Deep;
				else
					return MarkdownInHtmlMode.NA;
			}

			// Remove it
			tag.attributes.Remove("markdown");

			// Parse mode
			if (strMarkdownMode == "1")
				return (tag.Flags & HtmlTagFlags.ContentAsSpan)!=0 ? MarkdownInHtmlMode.Span : MarkdownInHtmlMode.Block;

			if (strMarkdownMode == "block")
				return MarkdownInHtmlMode.Block;

			if (strMarkdownMode == "deep")
				return MarkdownInHtmlMode.Deep;

			if (strMarkdownMode == "span")
				return MarkdownInHtmlMode.Span;

			return MarkdownInHtmlMode.Off;
		}

		internal bool ProcessMarkdownEnabledHtml(Block b, HtmlTag openingTag, MarkdownInHtmlMode mode)
		{
			// Current position is just after the opening tag

			// Scan until we find matching closing tag
			int inner_pos = Position;
			int depth = 1;
			bool bHasUnsafeContent = false;
			while (!Eof)
			{
				// Find next angle bracket
				if (!Find('<'))
					break;

				// Is it a html tag?
				int tagpos = Position;
				HtmlTag tag = HtmlTag.Parse(this);
				if (tag == null)
				{
					// Nope, skip it 
					SkipForward(1);
					continue;
				}

				// In markdown off mode, we need to check for unsafe tags
				if (m_markdown.SafeMode && mode == MarkdownInHtmlMode.Off && !bHasUnsafeContent)
				{
					if (!tag.IsSafe())
						bHasUnsafeContent = true;
				}

				// Ignore self closing tags
				if (tag.closed)
					continue;

				// Same tag?
				if (tag.name == openingTag.name)
				{
					if (tag.closing)
					{
						depth--;
						if (depth == 0)
						{
							// End of tag?
							SkipLinespace();
							SkipEol();

							b.BlockType = BlockType.HtmlTag;
							b.Data = openingTag;
							b.ContentEnd = Position;

							switch (mode)
							{
								case MarkdownInHtmlMode.Span:
								{
									Block span = this.CreateBlock();
									span.Buf = Input;
									span.BlockType = BlockType.span;
									span.ContentStart = inner_pos;
									span.ContentLen = tagpos - inner_pos;

									b.Children = new List<Block>();
									b.Children.Add(span);
									break;
								}

								case MarkdownInHtmlMode.Block:
								case MarkdownInHtmlMode.Deep:
								{
									// Scan the internal content
									var bp = new BlockProcessor(m_markdown, mode == MarkdownInHtmlMode.Deep);
									b.Children = bp.ScanLines(Input, inner_pos, tagpos - inner_pos);
									break;
								}

								case MarkdownInHtmlMode.Off:
								{
									if (bHasUnsafeContent)
									{
										b.BlockType = BlockType.unsafe_html;
										b.ContentEnd = Position;
									}
									else
									{
										Block span = this.CreateBlock();
										span.Buf = Input;
										span.BlockType = BlockType.html;
										span.ContentStart = inner_pos;
										span.ContentLen = tagpos - inner_pos;

										b.Children = new List<Block>();
										b.Children.Add(span);
									}
									break;
								}
							}


							return true;
						}
					}
					else
					{
						depth++;
					}
				}
			}

			// Missing closing tag(s).  
			return false;
		}

		// Scan from the current position to the end of the html section
		internal bool ScanHtml(Block b)
		{
			// Remember start of html
			int posStartPiece = this.Position;

			// Parse a HTML tag
			HtmlTag openingTag = HtmlTag.Parse(this);
			if (openingTag == null)
				return false;

			// Closing tag?
			if (openingTag.closing)
				return false;

			// Safe mode?
			bool bHasUnsafeContent = m_markdown.SafeMode && !openingTag.IsSafe();

			HtmlTagFlags flags = openingTag.Flags;

			// Is it a block level tag?
			if ((flags & HtmlTagFlags.Block) == 0)
				return false;

			// Closed tag, hr or comment?
			if ((flags & HtmlTagFlags.NoClosing) != 0 || openingTag.closed)
			{
				SkipLinespace();
				SkipEol();

				b.ContentEnd = Position;
				b.BlockType = bHasUnsafeContent ? BlockType.unsafe_html : BlockType.html;
				return true;
			}

			// Can it also be an inline tag?
			if ((flags & HtmlTagFlags.Inline) != 0)
			{
				// Yes, opening tag must be on a line by itself
				SkipLinespace();
				if (!Eol)
					return false;
			}

			// Head block extraction?
			bool bHeadBlock = m_markdown.ExtractHeadBlocks && string.Compare(openingTag.name, "head", true) == 0;
			int headStart = this.Position;

			// Work out the markdown mode for this element
			if (!bHeadBlock && m_markdown.ExtraMode)
			{
				MarkdownInHtmlMode MarkdownMode = this.GetMarkdownMode(openingTag);
				if (MarkdownMode != MarkdownInHtmlMode.NA)
				{
					return this.ProcessMarkdownEnabledHtml(b, openingTag, MarkdownMode);
				}
			}

			List<Block> childBlocks = null;

			// Now capture everything up to the closing tag and put it all in a single HTML block
			int depth = 1;

			while (!Eof)
			{
				// Find next angle bracket
				if (!Find('<'))
					break;

				// Save position of current tag
				int posStartCurrentTag = Position;

				// Is it a html tag?
				HtmlTag tag = HtmlTag.Parse(this);
				if (tag == null)
				{
					// Nope, skip it 
					SkipForward(1);
					continue;
				}

				// Safe mode checks
				if (m_markdown.SafeMode && !tag.IsSafe())
					bHasUnsafeContent = true;

				// Ignore self closing tags
				if (tag.closed)
					continue;

				// Markdown enabled content?
				if (!bHeadBlock && !tag.closing && m_markdown.ExtraMode && !bHasUnsafeContent)
				{
					MarkdownInHtmlMode MarkdownMode = this.GetMarkdownMode(tag);
					if (MarkdownMode != MarkdownInHtmlMode.NA)
					{
						Block markdownBlock = this.CreateBlock();
						if (this.ProcessMarkdownEnabledHtml(markdownBlock, tag, MarkdownMode))
						{
							if (childBlocks==null)
							{
								childBlocks = new List<Block>();
							}

							// Create a block for everything before the markdown tag
							if (posStartCurrentTag > posStartPiece)
							{
								Block htmlBlock = this.CreateBlock();
								htmlBlock.Buf = Input;
								htmlBlock.BlockType = BlockType.html;
								htmlBlock.ContentStart = posStartPiece;
								htmlBlock.ContentLen = posStartCurrentTag - posStartPiece;

								childBlocks.Add(htmlBlock);
							}

							// Add the markdown enabled child block
							childBlocks.Add(markdownBlock);

							// Remember start of the next piece
							posStartPiece = Position;

							continue;
						}
						else
						{
							this.FreeBlock(markdownBlock);
						}
					}
				}
				
				// Same tag?
				if (tag.name == openingTag.name)
				{
					if (tag.closing)
					{
						depth--;
						if (depth == 0)
						{
							// End of tag?
							SkipLinespace();
							SkipEol();

							// If anything unsafe detected, just encode the whole block
							if (bHasUnsafeContent)
							{
								b.BlockType = BlockType.unsafe_html;
								b.ContentEnd = Position;
								return true;
							}

							// Did we create any child blocks
							if (childBlocks != null)
							{
								// Create a block for the remainder
								if (Position > posStartPiece)
								{
									Block htmlBlock = this.CreateBlock();
									htmlBlock.Buf = Input;
									htmlBlock.BlockType = BlockType.html;
									htmlBlock.ContentStart = posStartPiece;
									htmlBlock.ContentLen = Position - posStartPiece;

									childBlocks.Add(htmlBlock);
								}

								// Return a composite block
								b.BlockType = BlockType.Composite;
								b.ContentEnd = Position;
								b.Children = childBlocks;
								return true;
							}

							// Extract the head block content
							if (bHeadBlock)
							{
								var content = this.Substring(headStart, posStartCurrentTag - headStart);
								m_markdown.HeadBlockContent = (m_markdown.HeadBlockContent ?? "") + content.Trim() + "\n";
								b.BlockType = BlockType.html;
								b.ContentStart = Position;
								b.ContentEnd = Position;
								b.LineStart = Position;
								return true;
							}

							// Straight html block
							b.BlockType = BlockType.html;
							b.ContentEnd = Position;
							return true;
						}
					}
					else
					{
						depth++;
					}
				}
			}

			// Rewind to just after the tag
			return false;
		}


		/// <summary>
		/// Handles the docnet extension, starting with '@'. This can be:
		/// * @fa-
		/// * @alert
		///   @end
		/// * @tabs
		///   @tabsend
		/// </summary>
		/// <param name="b">The b.</param>
		/// <returns>true if extension was correctly handled, false otherwise (error)</returns>
		private bool HandleDocNetExtension(Block b)
		{
			var initialStart = this.Position;
			if(DoesMatch("@fa-"))
			{
				return HandleFontAwesomeExtension(b, BlockType.font_awesome);
			}
			if(DoesMatch("@fabrands-"))
			{
				return HandleFontAwesomeExtension(b, BlockType.font_awesome_brands);
			}
			if(DoesMatch("@fasolid-"))
			{
				return HandleFontAwesomeExtension(b, BlockType.font_awesome_solid);
			}
			// first match @tabs, and then @tab, as both are handled by this processor.
			if(DoesMatch("@tabs"))
			{
				return HandleTabsExtension(b);
			}
			if(DoesMatch("@tab"))
			{
				return HandleTabForTabsExtension(b);
			}
			if(DoesMatch("@alert"))
			{
				return HandleAlertExtension(b);
			}
		    if(DoesMatch("@snippet"))
		    {
		        return HandleSnippetExtension(b);
		    }
			return false;
		}


        /// <summary>
        /// Handles the alert extension:
        /// @alert type
        /// text
        /// @end
        /// 
        /// where text can be anything and has to be handled further. 
        /// type is: danger, warning, info or neutral.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool HandleAlertExtension(Block b)
		{
			// skip '@alert'
			if(!SkipString("@alert"))
			{
				return false;
			}
			SkipLinespace();
			var alertType = string.Empty;
			if(!SkipIdentifier(ref alertType))
			{
				return false;
			}
			SkipToNextLine();
			int startContent = this.Position;

			// find @end.
			if(!Find("@end"))
			{
				return false;
			}
			// Character before must be a eol char
			if(!IsLineEnd(CharAtOffset(-1)))
			{
				return false;
			}
			int endContent = Position;
			// skip @end
			SkipString("@end");
			SkipLinespace();
			if(!Eol)
			{
				return false;
			}

			// Remove the trailing line end
			endContent = UnskipCRLFBeforePos(endContent);
			b.BlockType = BlockType.alert;
			b.Data = alertType.ToLowerInvariant();
			// scan the content, as it can contain markdown statements.
			var contentProcessor = new BlockProcessor(m_markdown, m_markdown.MarkdownInHtml);
			b.Children = contentProcessor.ScanLines(Input, startContent, endContent - startContent);
			return true;
		}


		private bool HandleTabsExtension(Block b)
		{
			// skip '@tabs'
			if(!SkipString("@tabs"))
			{
				return false;
			}
			// ignore what's specified behind @tabs
			SkipToNextLine();
			int startContent = this.Position;
			// find @end.
			if(!Find("@endtabs"))
			{
				return false;
			}
			// Character before must be a eol char
			if(!IsLineEnd(CharAtOffset(-1)))
			{
				return false;
			}
			int endContent = Position;
			// skip @end
			SkipString("@endtabs");
			SkipLinespace();
			if(!Eol)
			{
				return false;
			}
			// Remove the trailing line end
			endContent = UnskipCRLFBeforePos(endContent);
			b.BlockType = BlockType.tabs;
			// scan the content, as it can contain markdown statements.
			var contentProcessor = new BlockProcessor(m_markdown, m_markdown.MarkdownInHtml);
			var scanLines = contentProcessor.ScanLines(this.Input, startContent, endContent - startContent);
			// check whether the content is solely tab blocks. If not, we ignore this tabs specification.
			if(scanLines.Any(x=>x.BlockType != BlockType.tab))
			{
				return false;
			}
			b.Children = scanLines;
			return true;
		}


		/// <summary>
		/// Handles the tab for tabs extension. This is a docnet extension and it handles:
		/// @tab tab head text
		/// tab content
		/// @end
		/// </summary>
		/// <param name="b">The current block.</param>
		/// <returns></returns>
		private bool HandleTabForTabsExtension(Block b)
		{
			// skip '@tab'
			if(!SkipString("@tab"))
			{
				return false;
			}
			SkipLinespace();
			var tabHeaderTextStart = this.Position;
			// skip to eol, then grab the content between positions.
			SkipToEol();
			var tabHeaderText = this.Input.Substring(tabHeaderTextStart, this.Position - tabHeaderTextStart);
			SkipToNextLine();
			int startContent = this.Position;

			// find @end.
			if(!Find("@end"))
			{
				return false;
			}
			// Character before must be a eol char
			if(!IsLineEnd(CharAtOffset(-1)))
			{
				return false;
			}
			int endContent = Position;
			// skip @end
			SkipString("@end");
			SkipLinespace();
			if(!Eol)
			{
				return false;
			}

			// Remove the trailing line end
			endContent = UnskipCRLFBeforePos(endContent);
			b.BlockType = BlockType.tab;
			b.Data = tabHeaderText;
			// scan the content, as it can contain markdown statements.
			var contentProcessor = new BlockProcessor(m_markdown, m_markdown.MarkdownInHtml);
			b.Children = contentProcessor.ScanLines(Input, startContent, endContent - startContent);
			return true;
		}
        

        /// <summary>
        /// Handles the snippet extension:
        /// @snippet language [filename] pattern
        /// 
        /// where 'language' can be: cs, xml or txt. If something else, txt is used
        /// '[filename]' is evaluated relatively to the document location of the current document. 
        /// 'pattern' is the pattern passed to the extractor, which is determined based on the language. This is Projbook code.
        /// 
        /// The read snippet is wrapped in a fenced code block with language as language marker, except for txt, which will get 'nohighlight'.
        /// This fenced code block is then parsed again and that result is returned as b's data.
        /// </summary>
        /// <param name="b">The block to handle.</param>
        /// <returns></returns>
        private bool HandleSnippetExtension(Block b)
        {
            // skip @snippet
            if(!SkipString("@snippet"))
            {
                return false;
            }
            if(!SkipLinespace())
            {
                return false;
            }

            // language
            var language = string.Empty;
            if(!SkipIdentifier(ref language))
            {
                return false;
            }

            if(!SkipLinespace())
            {
                return false;
            }

            // [filename]
            if(!this.SkipChar('['))
            {
                return false;
            }
            // mark start of filename string
            this.Mark();
            if(!this.Find(']'))
            {
                return false;
            }
            string filename = this.Extract();
            if(string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }
            if(!SkipChar(']'))
            {
                return false;
            }
            if(!SkipLinespace())
            {
                return false;
            }

            // pattern
            var patternStart = this.Position;
            SkipToEol();
            var pattern = this.Input.Substring(patternStart, this.Position - patternStart);
            SkipToNextLine();
            language = language.ToLowerInvariant();
            ISnippetExtractor extractor = null;
            switch(language)
            {
                case "cs":
                    extractor = new CSharpSnippetExtractor();
                    break;
                case "xml":
                    extractor = new XmlSnippetExtractor();
                    break;
                default:
                    // text
                    language = "nohighlight";
                    extractor = new DefaultSnippetExtractor();
                    break;
            }

            // extract the snippet, then build the fenced block to return.
            var fullFilename = Path.Combine(Path.GetDirectoryName(m_markdown.SourceDocumentFilename) ?? string.Empty, filename);
            var snippetText = extractor.Extract(fullFilename, pattern) ?? string.Empty;
            b.BlockType = BlockType.codeblock;
            b.Data = language;
            var child = CreateBlock();
            child.BlockType = BlockType.indent;
            child.Buf = snippetText;
            child.ContentStart = 0;
            child.ContentEnd = snippetText.Length;
            b.Children = new List<Block>() { child};
            return true;
        }


        /// <summary>
        /// Handles the font awesome extension, which is available in DocNet mode. FontAwesome extension uses @fa-iconname, where iconname is the name of the fontawesome icon.
        /// Called when '@fa-' has been seen. Current position is on 'f' of 'fa-'.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool HandleFontAwesomeExtension(Block b, BlockType typeofBlock = BlockType.font_awesome)
		{
			string iconName = string.Empty;
			int newPosition = this.Position;
			if(!Utils.SkipFontAwesome(this.Input, this.Position, out newPosition, out iconName))
			{
				return false;
			}
			this.Position = newPosition;
			b.BlockType = typeofBlock;
			b.Data = iconName;
			return true;
		}


		/*
		 * Spacing
		 * 
		 * 1-3 spaces - Promote to indented if more spaces than original item
		 * 
		 */

		/* 
		 * BuildList - build a single <ol> or <ul> list
		 */
		private Block BuildList(List<Block> lines)
		{
			// What sort of list are we dealing with
			BlockType listType = lines[0].BlockType;
			System.Diagnostics.Debug.Assert(listType == BlockType.ul_li || listType == BlockType.ol_li);

			// Preprocess
			// 1. Collapse all plain lines (ie: handle hardwrapped lines)
			// 2. Promote any unindented lines that have more leading space 
			//    than the original list item to indented, including leading 
			//    special chars
			int leadingSpace = lines[0].LeadingSpaces;
			for (int i = 1; i < lines.Count; i++)
			{
				// Join plain paragraphs
				if ((lines[i].BlockType == BlockType.p) &&
					(lines[i - 1].BlockType == BlockType.p || lines[i - 1].BlockType == BlockType.ul_li || lines[i - 1].BlockType==BlockType.ol_li))
				{
					lines[i - 1].ContentEnd = lines[i].ContentEnd;
					FreeBlock(lines[i]);
					lines.RemoveAt(i);
					i--;
					continue;
				}

				if (lines[i].BlockType != BlockType.indent && lines[i].BlockType != BlockType.Blank)
				{
					int thisLeadingSpace = lines[i].LeadingSpaces;
					if (thisLeadingSpace > leadingSpace)
					{
						// Change line to indented, including original leading chars 
						// (eg: '* ', '>', '1.' etc...)
						lines[i].BlockType = BlockType.indent;
						int saveend = lines[i].ContentEnd;
						lines[i].ContentStart = lines[i].LineStart + thisLeadingSpace;
						lines[i].ContentEnd = saveend;
					}
				}
			}


			// Create the wrapping list item
			var List = new Block(listType == BlockType.ul_li ? BlockType.ul : BlockType.ol);
			List.Children = new List<Block>();

			// Process all lines in the range		
			for (int i = 0; i < lines.Count; i++)
			{
				System.Diagnostics.Debug.Assert(lines[i].BlockType == BlockType.ul_li || lines[i].BlockType==BlockType.ol_li);

				// Find start of item, including leading blanks
				int start_of_li = i;
				while (start_of_li > 0 && lines[start_of_li - 1].BlockType == BlockType.Blank)
					start_of_li--;

				// Find end of the item, including trailing blanks
				int end_of_li = i;
				while (end_of_li < lines.Count - 1 && lines[end_of_li + 1].BlockType != BlockType.ul_li && lines[end_of_li + 1].BlockType != BlockType.ol_li)
					end_of_li++;

				// Is this a simple or complex list item?
				if (start_of_li == end_of_li)
				{
					// It's a simple, single line item item
					System.Diagnostics.Debug.Assert(start_of_li == i);
					List.Children.Add(CreateBlock().CopyFrom(lines[i]));
				}
				else
				{
					// Build a new string containing all child items
					bool bAnyBlanks = false;
					StringBuilder sb = m_markdown.GetStringBuilder();
					for (int j = start_of_li; j <= end_of_li; j++)
					{
						var l = lines[j];
						sb.Append(l.Buf, l.ContentStart, l.ContentLen);
						sb.Append('\n');

						if (lines[j].BlockType == BlockType.Blank)
						{
							bAnyBlanks = true;
						}
					}

					// Create the item and process child blocks
					var item = new Block(BlockType.li);
					item.Children = new BlockProcessor(m_markdown, m_bMarkdownInHtml, listType).Process(sb.ToString());

					// If no blank lines, change all contained paragraphs to plain text
					if (!bAnyBlanks)
					{
						foreach (var child in item.Children)
						{
							if (child.BlockType == BlockType.p)
							{
								child.BlockType = BlockType.span;
							}
						}
					}

					// Add the complex item
					List.Children.Add(item);
				}

				// Continue processing from end of li
				i = end_of_li;
			}

			FreeBlocks(lines);
			lines.Clear();

			// Continue processing after this item
			return List;
		}

		/* 
		 * BuildDefinition - build a single <dd> item
		 */
		private Block BuildDefinition(List<Block> lines)
		{
			// Collapse all plain lines (ie: handle hardwrapped lines)
			for (int i = 1; i < lines.Count; i++)
			{
				// Join plain paragraphs
				if ((lines[i].BlockType == BlockType.p) &&
					(lines[i - 1].BlockType == BlockType.p || lines[i - 1].BlockType == BlockType.dd))
				{
					lines[i - 1].ContentEnd = lines[i].ContentEnd;
					FreeBlock(lines[i]);
					lines.RemoveAt(i);
					i--;
					continue;
				}
			}

			// Single line definition
			bool bPreceededByBlank=(bool)lines[0].Data;
			if (lines.Count==1 && !bPreceededByBlank)
			{
				var ret=lines[0];
				lines.Clear();
				return ret;
			}

			// Build a new string containing all child items
			StringBuilder sb = m_markdown.GetStringBuilder();
			for (int i = 0; i < lines.Count; i++)
			{
				var l = lines[i];
				sb.Append(l.Buf, l.ContentStart, l.ContentLen);
				sb.Append('\n');
			}

			// Create the item and process child blocks
			var item = this.CreateBlock();
			item.BlockType = BlockType.dd;
			item.Children = new BlockProcessor(m_markdown, m_bMarkdownInHtml, BlockType.dd).Process(sb.ToString());

			FreeBlocks(lines);
			lines.Clear();

			// Continue processing after this item
			return item;
		}

		void BuildDefinitionLists(List<Block> blocks)
		{
			Block currentList = null;
			for (int i = 0; i < blocks.Count; i++)
			{
				switch (blocks[i].BlockType)
				{
					case BlockType.dt:
					case BlockType.dd:
						if (currentList==null)
						{
							currentList=CreateBlock();
							currentList.BlockType=BlockType.dl;
							currentList.Children=new List<Block>();
							blocks.Insert(i, currentList);
							i++;
						}

						currentList.Children.Add(blocks[i]);
						blocks.RemoveAt(i);
						i--;
						break;

					default:
						currentList = null;
						break;
				}
			}
		}

		private Block BuildFootnote(List<Block> lines)
		{
			// Collapse all plain lines (ie: handle hardwrapped lines)
			for (int i = 1; i < lines.Count; i++)
			{
				// Join plain paragraphs
				if ((lines[i].BlockType == BlockType.p) &&
					(lines[i - 1].BlockType == BlockType.p || lines[i - 1].BlockType == BlockType.footnote))
				{
					lines[i - 1].ContentEnd = lines[i].ContentEnd;
					FreeBlock(lines[i]);
					lines.RemoveAt(i);
					i--;
					continue;
				}
			}

			// Build a new string containing all child items
			StringBuilder sb = m_markdown.GetStringBuilder();
			for (int i = 0; i < lines.Count; i++)
			{
				var l = lines[i];
				sb.Append(l.Buf, l.ContentStart, l.ContentLen);
				sb.Append('\n');
			}

			// Create the item and process child blocks
			var item = this.CreateBlock();
			item.BlockType = BlockType.footnote;
			item.Data = lines[0].Data;
			item.Children = new BlockProcessor(m_markdown, m_bMarkdownInHtml, BlockType.footnote).Process(sb.ToString());

			FreeBlocks(lines);
			lines.Clear();

			// Continue processing after this item
			return item;
		}

		bool ProcessFencedCodeBlock(Block b)
		{
            char delim = Current;

			// Extract the fence
			Mark();
			while (Current == delim)
				SkipForward(1);
			string strFence = Extract();

			// Must be at least 3 long
			if (strFence.Length < 3)
				return false;

			if(m_markdown.GitHubCodeBlocks)
			{
				// check whether a name has been specified after the start ```. If so we'll store that into 'Data'.
				var languageName = string.Empty;
				// allow space between first fence and name
				SkipLinespace();
				SkipIdentifier(ref languageName);
				b.Data = string.IsNullOrWhiteSpace(languageName) ? "nohighlight" : languageName;
				// skip linespace to EOL
				SkipLinespace();
			}
			else
			{
				// Rest of line must be blank
				SkipLinespace();
				if(!Eol)
					return false;
			}

			// Skip the eol and remember start of code
			SkipEol();
			int startCode = Position;

			// Find the end fence
			if (!Find(strFence))
				return false;

			// Character before must be a eol char
			if (!IsLineEnd(CharAtOffset(-1)))
				return false;

			int endCode = Position;

			// Skip the fence
			SkipForward(strFence.Length);

			// Whitespace allowed at end
			SkipLinespace();
			if (!Eol)
				return false;

			// Create the code block
			b.BlockType = BlockType.codeblock;
			b.Children = new List<Block>();

			// Remove the trailing line end
			if (Input[endCode - 1] == '\r' && Input[endCode - 2] == '\n')
				endCode -= 2;
			else if (Input[endCode - 1] == '\n' && Input[endCode - 2] == '\r')
				endCode -= 2;
			else
				endCode--;

			// Create the child block with the entire content
			var child = CreateBlock();
			child.BlockType = BlockType.indent;
			child.Buf = Input;
			child.ContentStart = startCode;
			child.ContentEnd = endCode;
			b.Children.Add(child);

			return true;
		}

		Markdown m_markdown;
		BlockType m_parentType;
		bool m_bMarkdownInHtml;
	}
}
