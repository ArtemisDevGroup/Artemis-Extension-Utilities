namespace Artemis.Extension.Utilities
{
    /// <summary>
    /// Base class for file types.
    /// </summary>
    /// <param name="target">The target file to read and write data to.</param>
    public class FileBase(FileInfo target)
    {
        /// <summary>
        /// The current instance target file information.
        /// </summary>
        public FileInfo Target { get; } = target;
    }
}
