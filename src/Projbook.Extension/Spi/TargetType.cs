namespace Projbook.Extension.Spi
{
    /// <summary>
    /// Represents an extraction target.
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        /// Free text target, used by plugins extracting from free value.
        /// </summary>
        FreeText,

        /// <summary>
        /// File target, used by plugins extracting from a file.
        /// </summary>
        File,

        /// <summary>
        /// Folder target, ised bu plugins extracting from a folder.
        /// </summary>
        Folder
    }
}