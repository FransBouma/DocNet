namespace Projbook.Extension.Exception
{
    /// <summary>
    /// Represents a snippet extraction exception.
    /// </summary>
    public class SnippetExtractionException : System.Exception
    {
        /// <summary>
        /// The pattern the exception is about.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ProjbookEngine"/>.
        /// </summary>
        /// <param name="message">Initializes the required <see cref="Message"/>.</param>
        /// <param name="pattern">Initializes the required <see cref="Pattern"/>.</param>
        public SnippetExtractionException(string message, string pattern)
            : base(message)
        {
            // Initialize
            this.Pattern = pattern;
        }
    }
}