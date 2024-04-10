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
    }
}
