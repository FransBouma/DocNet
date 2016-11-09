using EnsureThat;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Projbook.Extension.Exception;
using Projbook.Extension.Spi;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace Projbook.Extension.CSharpExtractor
{
    /// <summary>
    /// Extractor in charge of browsing source directories. load file content and extract requested member.
    /// </summary>
    [Syntax(name: "csharp")]
    public class CSharpSnippetExtractor : DefaultSnippetExtractor
    {
        /// <summary>
        /// Represents the matching trie used for member matching.
        /// Because of the cost of building the Trie, this value is lazy loaded and kept for future usages.
        /// </summary>
        private CSharpSyntaxMatchingNode syntaxTrie;

        /// <summary>
        /// Extracts a snippet from a given rule pattern.
        /// </summary>
        /// <param name="fileSystemInfo">The file system info.</param>
        /// <param name="memberPattern">The member pattern to extract.</param>
        /// <returns>The extracted snippet.</returns>
        public override Model.Snippet Extract(FileSystemInfoBase fileSystemInfo, string memberPattern)
        {
            // Return the entire code if no member is specified
            if (string.IsNullOrWhiteSpace(memberPattern))
            {
                return base.Extract(fileSystemInfo, memberPattern);
            }

            // Parse the matching rule from the pattern
            CSharpMatchingRule rule = CSharpMatchingRule.Parse(memberPattern);

            // Load the trie for pattern matching
            if (null == this.syntaxTrie)
            {
                // Load file content
                string sourceCode = base.LoadFile(this.ConvertToFile(fileSystemInfo));

                // Build a syntax tree from the source code
                SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
                SyntaxNode root = tree.GetRoot();

                // Visit the syntax tree for generating a Trie for pattern matching
                CSharpSyntaxWalkerMatchingBuilder syntaxMatchingBuilder = new CSharpSyntaxWalkerMatchingBuilder();
                syntaxMatchingBuilder.Visit(root);

                // Retrieve the Trie root
                this.syntaxTrie = syntaxMatchingBuilder.Root;
            }

            // Match the rule from the syntax matching Trie
            CSharpSyntaxMatchingNode matchingTrie = syntaxTrie.Match(rule.MatchingChunks);
            if (null == matchingTrie)
            {
                throw new SnippetExtractionException("Cannot find member", memberPattern);
            }

            // Build a snippet for extracted syntax nodes
            return this.BuildSnippet(matchingTrie.MatchingSyntaxNodes, rule.ExtractionMode);
        }

        /// <summary>
        /// Builds a snippet from extracted syntax nodes.
        /// </summary>
        /// <param name="nodes">The exctracted nodes.</param>
        /// <param name="extractionMode">The extraction mode.</param>
        /// <returns>The built snippet.</returns>
        private Model.Snippet BuildSnippet(SyntaxNode[] nodes, CSharpExtractionMode extractionMode)
        {
            // Data validation
            Ensure.That(() => nodes).IsNotNull();
            Ensure.That(() => nodes).HasItems();

            // Extract code from each snippets
            StringBuilder stringBuilder = new StringBuilder();
            bool firstSnippet = true;
            foreach (SyntaxNode node in nodes)
            {
                // Write line return between each snippet
                if (!firstSnippet)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                }
                
                // Write each snippet line
                string[] lines = node.GetText().Lines.Select(x => x.ToString()).ToArray();
                int contentPosition = this.DetermineContentPosition(node);
                this.WriteAndCleanupSnippet(stringBuilder, lines, extractionMode, contentPosition);

                // Flag the first snippet as false
                firstSnippet = false;
            }
            
            // Create the snippet from the exctracted code
            return new Model.PlainTextSnippet(stringBuilder.ToString());
        }

        /// <summary>
        /// Determines the content's block position depending on the node type.
        /// </summary>
        /// <param name="node">The node to extract the content position from.</param>
        /// <returns>The determined content position or 0 if not found.</returns>
        private int DetermineContentPosition(SyntaxNode node)
        {
            // Data validation
            Ensure.That(() => node).IsNotNull();

            // Select the content node element depending on the node type
            TextSpan? contentTextSpan = null;
            switch (node.Kind())
            {
                // Accessor list content
                case SyntaxKind.PropertyDeclaration:
                case SyntaxKind.IndexerDeclaration:
                case SyntaxKind.EventDeclaration:
                    AccessorListSyntax accessorList = node.DescendantNodes().OfType<AccessorListSyntax>().FirstOrDefault();
                    if (null != accessorList)
                    {
                        contentTextSpan = accessorList.FullSpan;
                    }
                    break;
                
                // Contains children
                case SyntaxKind.NamespaceDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.ClassDeclaration:
                    SyntaxToken token = node.ChildTokens().Where(x => x.Kind() == SyntaxKind.OpenBraceToken).FirstOrDefault();
                    if (null != token)
                    {
                        contentTextSpan = token.FullSpan;
                    }
                    break;
                
                // Block content
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.DestructorDeclaration:
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                    BlockSyntax block = node.DescendantNodes().OfType<BlockSyntax>().FirstOrDefault();
                    if (null != block)
                    {
                        contentTextSpan = block.FullSpan;
                    }
                    break;
                
                // Not processed by projbook csharp extractor
                default:
                    break;
            }

            // Compute a line break insensitive position based on the fetched content text span if any is found
            if (null != contentTextSpan)
            {
                int relativeTextSpanStart = contentTextSpan.Value.Start - node.FullSpan.Start;
                return node
                    .ToFullString()
                    .Substring(0, relativeTextSpanStart)
                    .Replace("\r\n", "")
                    .Replace("\n", "").Length;
            }

            // Otherwise return 0 as default value
            return 0;
        }

        /// <summary>
        /// Writes and cleanup line snippets.
        /// Snippets are moved out of their context, for this reason we need to trim lines aroung and remove a part of the indentation.
        /// </summary>
        /// <param name="stringBuilder">The string builder used as output.</param>
        /// <param name="lines">The lines to process.</param>
        /// <param name="extractionMode">The extraction mode.</param>
        /// <param name="contentPosition">The content position.</param>
        private void WriteAndCleanupSnippet(StringBuilder stringBuilder, string[] lines, CSharpExtractionMode extractionMode, int contentPosition)
        {
            // Data validation
            Ensure.That(() => stringBuilder).IsNotNull();
            Ensure.That(() => lines).IsNotNull();

            // Do not process if lines are empty
            if (0 >= lines.Length)
            {
                return;
            }

            // Compute the index of the first selected line
            int startPos = 0;
            int skippedCharNumber = 0;
            if (CSharpExtractionMode.ContentOnly == extractionMode)
            {
                // Compute the content position index in the first processed line
                int contentPositionFirstLineIndex = 0;
                for (int totalLinePosition = 0; startPos < lines.Length; ++startPos)
                {
                    // Compute the content position in the current line
                    string line = lines[startPos];
                    int relativePosition = contentPosition - totalLinePosition;
                    int contentPositionInLine = relativePosition < line.Length ? relativePosition: -1;

                    // In expected in the current line
                    if (contentPositionInLine >= 0)
                    {
                        // Look for the relative index in the current line
                        // Save the found index and break the iteration if any open bracket is found
                        int indexOf = line.IndexOf('{', contentPositionInLine);
                        if (0 <= indexOf)
                        {
                            contentPositionFirstLineIndex = indexOf;
                            break;
                        }
                    }

                    // Move the total line position after the processed line
                    totalLinePosition += lines[startPos].Length;
                }

                // Extract block code if any opening bracket has been found
                if (startPos < lines.Length)
                {
                    int openingBracketPos = lines[startPos].IndexOf('{', contentPositionFirstLineIndex);
                    if (openingBracketPos >= 0)
                    {
                        // Extract the code before the curly bracket
                        if (lines[startPos].Length > openingBracketPos)
                        {
                            lines[startPos] = lines[startPos].Substring(openingBracketPos + 1);
                        }

                        // Skip the current line if empty
                        if (string.IsNullOrWhiteSpace(lines[startPos]) && lines.Length > 1 + startPos)
                        {
                            ++startPos;
                        }
                    }
                }
            }
            else
            {
                // Skip leading whitespace lines and keep track of the amount of skipped char
                for (; startPos < lines.Length; ++startPos)
                {
                    // Break on non whitespace line
                    string line = lines[startPos];
                    if (line.Trim().Length > 0)
                    {
                        break;
                    }

                    // Record skipped char number
                    skippedCharNumber += line.Length;
                }
            }

            // Compute the index of the lastselected line
            int endPos = -1 + lines.Length;
            if (CSharpExtractionMode.ContentOnly == extractionMode)
            {
                for (; 0 <= endPos && !lines[endPos].ToString().Contains('}'); --endPos);

                // Extract block code if any closing bracket has been found
                if (0 <= endPos)
                {
                    int closingBracketPos = lines[endPos].IndexOf('}');
                    if (closingBracketPos >= 0)
                    {
                        // Extract the code before the curly bracket
                        if (lines[endPos].Length > closingBracketPos)
                            lines[endPos] = lines[endPos].Substring(0, closingBracketPos).TrimEnd();
                    }

                    // Skip the current line if empty
                    if (string.IsNullOrWhiteSpace(lines[endPos]) && lines.Length > -1 + endPos)
                    {
                        --endPos;
                    }
                }
            }
            else
            {
                for (; 0 <= endPos && lines[endPos].ToString().Trim().Length == 0; --endPos);
            }

            // Compute the padding to remove for removing a part of the indentation
            int leftPadding = int.MaxValue;
            for (int i = startPos; i <= endPos; ++i)
            {
                // Ignore empty lines in the middle of the snippet
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    // Adjust the left padding with the available whitespace at the beginning of the line
                    leftPadding = Math.Min(leftPadding, lines[i].ToString().TakeWhile(Char.IsWhiteSpace).Count());
                }
            }

            // Write selected lines to the string builder
            bool firstLine = true;
            for (int i = startPos; i <= endPos; ++i)
            {
                // Write line return between each line
                if (!firstLine)
                {
                    stringBuilder.AppendLine();
                }

                // Remove a part of the indentation padding
                if (lines[i].Length > leftPadding)
                {
                    string line = lines[i].Substring(leftPadding);

                    // Process the snippet depending on the extraction mode
                    switch (extractionMode)
                    {
                        // Extract the block structure only
                        case CSharpExtractionMode.BlockStructureOnly:

                            // Compute the content position in the current line
                            int relativePosition = contentPosition - skippedCharNumber;
                            int contentPositionInLine = relativePosition < line.Length + leftPadding ? relativePosition : -1;

                            // Look for open bracket from the content position in line
                            int openingBracketPos = -1;
                            if (contentPositionInLine >= 0)
                            {
                                openingBracketPos = line.IndexOf('{', Math.Max(0, contentPositionInLine - leftPadding));
                            }

                            // Anonymize code content if an open bracket is found
                            if (openingBracketPos >= 0)
                            {
                                // Extract the code before the curly bracket
                                if (line.Length > openingBracketPos)
                                    line = line.Substring(0, 1 + openingBracketPos);

                                // Replace the content and close the block
                                line += string.Format("{0}    // ...{0}}}", Environment.NewLine);

                                // Stop the iteration
                                endPos = i;
                            }
                            break;
                    }

                    // Append the line
                    stringBuilder.Append(line);
                    skippedCharNumber += lines[i].Length;
                }
                
                // Flag the first line as false
                firstLine = false;
            }
        }
    }
}