namespace Artemis.Extension.Utilities
{
    /// <summary>
    /// File containing a packed Artemis Extension.
    /// </summary>
    public class ExtensionFile : FileBase
    {
        /// <summary>
        /// The file extension for extension files.
        /// </summary>
        public static readonly string FileExtension = ".aext";

        /// <summary>
        /// The information section of the extension file.
        /// </summary>
        public InformationFile? Information { get; set; }
        /// <summary>
        /// The binary section of the extension file.
        /// </summary>
        public BinaryFile? Binary { get; set; }
        /// <summary>
        /// Whether the information sectión contains a signature of the binary or not.
        /// </summary>
        public bool IsSigned
        {
            get
            {
                if (Information?.Data.SignatureAuthor == UnsignedInformationFile.DefaultSignatureAuthor && Helpers.IsEmpty(Information?.Data.Signature ?? []))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Sets the target extension file.
        /// </summary>
        /// <param name="target">The target extension file.</param>
        public ExtensionFile(FileInfo target) : base(target) { }

        /// <summary>
        /// Sets the target information file as well as the information and binary sections of it.
        /// </summary>
        /// <param name="target">The target extension file.</param>
        /// <param name="information">The information section.</param>
        /// <param name="binary">The binary section.</param>
        public ExtensionFile(FileInfo target, InformationFile information, BinaryFile binary) : base(target)
        {
            Information = information;
            Binary = binary;
        }

        /// <summary>
        /// Packs the information and binary data and saves it to the target.
        /// </summary>
        /// <remarks>If the target file already exists, it will be overwritten.</remarks>
        /// <exception cref="IOException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="InvalidOperationException">If Information or Binary is null.</exception>
        public void PackAndSave()
        {
            if (Target.Exists)
                Target.Delete();

            using FileStream fs = Target.OpenWrite();

            if (Information == null)
                throw new InvalidOperationException("Information property was null.");

            byte[] informationBytes = Information.Data.Bytes;
            fs.Write(informationBytes, 0, informationBytes.Length);

            if (Binary == null)
                throw new InvalidOperationException("Binary property was null.");

            byte[] binaryBytes = Binary.Bytes;
            fs.Write(binaryBytes, 0, binaryBytes.Length);
        }
        /// <summary>
        /// Unpacks the information and binary data from the target and loads it into this instance.
        /// </summary>
        /// <exception cref="FileNotFoundException">Target file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        public void UnpackAndLoad()
        {
            if (!Target.Exists)
                throw new FileNotFoundException("Cannot load data: target file does not exist.", Target.FullName);

            using FileStream fs = Target.OpenRead();
            
            byte[] informationBytes = new byte[ExtensionData.StructureSize];
            fs.Read(informationBytes, 0, informationBytes.Length);

            byte[] binaryBytes = new byte[fs.Length - ExtensionData.StructureSize];
            fs.Read(binaryBytes, 0, binaryBytes.Length);
        }
    }
}
