using System;
using System.Collections.Generic;
using System.Linq;
namespace Artemis.Extension.Utilities
{
    /// <summary>
    /// A class for accessing the data of a binary file.
    /// </summary>
    public class BinaryFile
    {
        /// <summary>
        /// The binary file info.
        /// </summary>
        public FileInfo Target { get; }

        /// <summary>
        /// All bytes in the binary file.
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// Reads the bytes of a binary file into the instance.
        /// </summary>
        /// <param name="target">The target binary file.</param>
        /// <exception cref="FileNotFoundException">Binary file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Inherited from <see cref="FileInfo.OpenRead()"/>.</exception>
        /// <exception cref="DirectoryNotFoundException">Inherited from <see cref="FileInfo.OpenRead()"/>.</exception>
        /// <exception cref="IOException">Inherited from <see cref="FileInfo.OpenRead()"/>.</exception>
        public BinaryFile(FileInfo target)
        {
            if (!target.Exists)
                throw new FileNotFoundException("Binary file does not exist.", target.FullName);

            Target = target;
            Bytes = new byte[target.Length];

            using FileStream fs = target.OpenRead();
            fs.Read(Bytes, 0, Bytes.Length);
        }
        /// <summary>
        /// Sets the target and the bytes of the binary file.
        /// </summary>
        /// <param name="target">The target file.</param>
        /// <param name="bytes">The binary bytes.</param>
        public BinaryFile(FileInfo target, byte[] bytes)
        {
            Target = target;
            Bytes = bytes;
        }

        /// <summary>
        /// Saves the bytes in <see cref="Bytes"/> to the target.
        /// </summary>
        /// <remarks>If the target file already exists, it will be overwritten.</remarks>
        /// <exception cref="IOException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void SaveToTarget()
        {
            if (Target.Exists)
                Target.Delete();

            using FileStream fs = Target.OpenWrite();
            fs.Write(Bytes, 0, Bytes.Length);
        }
    }
}
