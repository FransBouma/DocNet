
namespace Projbook.Extension.Spi
{
    /// <summary>
    /// Defines interface for snippet extractor.
    /// </summary>
    public interface ISnippetExtractor
    {
        /// <summary>
        /// Defines the target type.
        /// </summary>
        TargetType TargetType { get; }

        /// <summary>
        /// Extracts a snippet.
        /// </summary>
        /// <param name="fullFilename">The full filename (with path) to load and to extract the snippet from.</param>
        /// <param name="pattern">The extraction pattern.</param>
        /// <returns>
        /// The extracted snippet as string.
        /// </returns>
        string Extract(string fullFilename, string pattern);
    }
}