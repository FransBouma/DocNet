using System.Text;
using System;
using Projbook.Extension.Spi;
using System.IO;

namespace Projbook.Extension
{
    /// <summary>
    /// Extractor in charge of browsing source directories. load file content and extract requested member.
    /// </summary>
    public class DefaultSnippetExtractor : ISnippetExtractor
    {
        /// <summary>
        /// File target type.
        /// </summary>
        public TargetType TargetType { get { return TargetType.File; } }

        /// <summary>
        /// Extracts a snippet.
        /// </summary>
        /// <param name="fullFilename">The full filename (with path) to load and to extract the snippet from.</param>
        /// <param name="pattern">The extraction pattern, never used for this implementation.</param>
        /// <returns>
        /// The extracted snippet.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">fileSystemInfo</exception>
        public virtual string Extract(string fullFilename, string pattern)
        {
            if(string.IsNullOrEmpty(fullFilename))
            {
                throw new ArgumentNullException(nameof(fullFilename));
            }
            return this.LoadFile(fullFilename) ?? string.Empty;
        }

        /// <summary>
        /// Loads a file from the file name.
        /// </summary>
        /// <param name="fullFilename">The full filename.</param>
        /// <returns>
        /// The file's content.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">fileInfo</exception>
        protected string LoadFile(string fullFilename)
        {
            if(string.IsNullOrEmpty(fullFilename))
            {
                throw new ArgumentNullException(nameof(fullFilename));
            }
            return File.ReadAllText(fullFilename, Encoding.UTF8);
        }
    }
}