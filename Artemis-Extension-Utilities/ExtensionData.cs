using System.Runtime.InteropServices;

namespace Artemis.Extension.Utilities
{
    /// <summary>
    /// Information section of an Artemis extension file.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ExtensionData
    {
        /// <summary>
        /// The size of the RSA signature in bytes.
        /// </summary>
        private const int SignatureSize = 64;
        /// <summary>
        /// The size of the byte pad in bytes.
        /// </summary>
        private const int PadSize = 128;

        /// <summary>
        /// The size of the structure in bytes.
        /// </summary>
        public const int StructureSize = 1024;

        /// <summary>
        /// The Artemis version.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string ArtemisVersion;

        /// <summary>
        /// The name of the extension.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Name;

        /// <summary>
        /// The author of the extension.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Author;

        /// <summary>
        /// The extension description.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string Description;

        /// <summary>
        /// The extension version.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Version;

        /// <summary>
        /// The information section pad, required to force the struct to be 1024 bytes in size.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PadSize, ArraySubType = UnmanagedType.U1)]
        public byte[] Pad;

        /// <summary>
        /// The RSA signature author.
        /// </summary>
        /// <remarks>If the extension is unsigned, this field will be equal to <see cref="UnsignedInformationFile.DefaultSignatureAuthor"/>.</remarks>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string SignatureAuthor;

        /// <summary>
        /// The RSA signature.
        /// </summary>
        /// <remarks>If the extension is unsigned, all bytes will be initialized to zero.</remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SignatureSize, ArraySubType = UnmanagedType.U1)]
        public byte[] Signature;

        /// <summary>
        /// Initializes all fields to be zero.
        /// </summary>
        public ExtensionData()
        {
            ArtemisVersion = string.Empty;
            Name = string.Empty;
            Author = string.Empty;
            Description = string.Empty;
            Version = string.Empty;
            Pad = new byte[PadSize];
            Helpers.Initialize(Pad, 0);
            SignatureAuthor = string.Empty;
            Signature = new byte[SignatureSize];
            Helpers.Initialize(Signature, 0);
        }
        /// <summary>
        /// Initializes the struct with the raw bytes provided.
        /// </summary>
        /// <param name="bytes">The raw bytes of the struct. Must be 1024 bytes in size.</param>
        /// <exception cref="ArgumentException">The byte array is not 1024 bytes in size.</exception>
        /// <exception cref="OutOfMemoryException"></exception>
        /// <exception cref="MissingMethodException"></exception>
        public ExtensionData(byte[] bytes)
        {
            if (bytes.Length != 1024)
                throw new ArgumentException("Byte array must be 1024 bytes in size.", nameof(bytes));

            nint ptr = nint.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(StructureSize);
                Marshal.Copy(bytes, 0, ptr, StructureSize);
                this = Marshal.PtrToStructure<ExtensionData>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Gets a raw byte representation of the current struct instance.
        /// </summary>
        /// <exception cref="OutOfMemoryException"></exception>
        public readonly byte[] Bytes
        {
            get
            {
                byte[] bytes = new byte[StructureSize];

                nint ptr = nint.Zero;
                try
                {
                    ptr = Marshal.AllocHGlobal(bytes.Length);
                    Marshal.StructureToPtr(this, ptr, false);
                    Marshal.Copy(ptr, bytes, 0, bytes.Length);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
                return bytes;
            }
        }

        /// <summary>
        /// Writes a byte representation of the current struct instance to a target file.
        /// </summary>
        /// <param name="file">The target file to write to.</param>
        /// <remarks>If the target file already exists, it will be overwritten.</remarks>
        /// <exception cref="IOException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public readonly void WriteToFile(FileInfo file)
        {
            if (file.Exists)
                file.Delete();

            using FileStream fs = file.OpenWrite();

            byte[] bytes = Bytes;
            fs.Write(bytes, 0, bytes.Length);
        }
    }
}
