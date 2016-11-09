namespace Projbook.Extension.CSharpExtractor
{
    /// <summary>
    /// Represents the extraction mode.
    /// </summary>
    public enum CSharpExtractionMode
    {
        /// <summary>
        /// Full member: Do not process the snippet and print it as it.
        /// </summary>
        FullMember,

        /// <summary>
        /// Content only: Extract the code block and print this part only.
        /// </summary>
        ContentOnly,

        /// <summary>
        /// Block structure only: Remove the block content and print the code structure only.
        /// </summary>
        BlockStructureOnly
    }
}