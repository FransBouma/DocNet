using Projbook.Extension.Exception;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Projbook.Extension.CSharpExtractor
{
    /// <summary>
    /// Represents a matching rule for referencing a C# member.
    /// </summary>
    public class CSharpMatchingRule
    {
        /// <summary>
        /// The matching chunk to identify which member are the snippet targets.
        /// </summary>
        public string[] MatchingChunks { get; private set; }

        /// <summary>
        /// The snippet extraction mode.
        /// </summary>
        public CSharpExtractionMode ExtractionMode { get; private set; }

        /// <summary>
        /// Defines rule regex used to parse the snippet into chunks.
        /// Expected input format: Path/File.cs [My.Name.Space.Class.Method][(string, string)]
        /// * The first chunk is the file name and will be loaded in TargetFile
        /// * The optional second chunks are all full qualified name to the member separated by "."
        /// * The optional last chunk is the method parameters if matching a method.
        /// </summary>
        private static Regex ruleRegex = new Regex(@"^([-=])?([^(]+)?\s*(\([^)]*\s*\))?\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Parses the token
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static CSharpMatchingRule Parse(string pattern)
        {
            // Try to match the regex
            pattern = Regex.Replace(pattern, @"\s", string.Empty);
            Match match = CSharpMatchingRule.ruleRegex.Match(pattern);
            if (!match.Success || string.IsNullOrWhiteSpace(match.Groups[0].Value))
            {
                throw new SnippetExtractionException("Invalid extraction rule", pattern);
            }

            // Retrieve values from the regex matching
            string extractionOption = match.Groups[1].Value;
            string rawMember = match.Groups[2].Value.Trim();
            string rawParameters = match.Groups[3].Value.Trim();

            // Build The matching chunk with extracted data
            List<string> matchingChunks = new List<string>();
            matchingChunks.AddRange(rawMember.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
            if (rawParameters.Length >= 1)
            {
                matchingChunks.Add(rawParameters);
            }

            // Read extraction mode
            CSharpExtractionMode extractionMode = CSharpExtractionMode.FullMember;
            switch (extractionOption)
            {
                case "-":
                    extractionMode = CSharpExtractionMode.ContentOnly;
                    break;
                case "=":
                    extractionMode = CSharpExtractionMode.BlockStructureOnly;
                    break;
            }

            // Build the matching rule based on the regex matching
            return new CSharpMatchingRule
            {
                MatchingChunks = matchingChunks.ToArray(),
                ExtractionMode = extractionMode
            };
        }
    }
}