using System.Security.Cryptography;

namespace Artemis.Extension.Utilities
{
    /// <summary>
    /// The base class of a file containing the information section of an Artemis extension.
    /// </summary>
    public class InformationFile : FileBase
    {
        protected ExtensionData _data;
        /// <summary>
        /// The extension data.
        /// </summary>
        public ExtensionData Data { get => _data; set => _data = value; }

        /// <summary>
        /// Sets the target information file location.
        /// </summary>
        /// <param name="target">The target file information.</param>
        public InformationFile(FileInfo target) : base(target) => Data = new();
        /// <summary>
        /// Sets the target information file location and data.
        /// </summary>
        /// <param name="target">The target file information.</param>
        /// <param name="data">The extension data.</param>
        public InformationFile(FileInfo target, ExtensionData data) : base(target) => Data = data;

        /// <summary>
        /// Saves the current data to the target.
        /// </summary>
        /// <remarks>If the target file already exists, it will be overwritten.</remarks>
        /// <exception cref="IOException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public virtual void SaveToTarget()
        {
            Target.Delete();

            using FileStream fs = Target.OpenWrite();

            byte[] bytes = Data.Bytes;
            fs.Write(bytes, 0, bytes.Length);
        }
        /// <summary>
        /// Loads data from the target.
        /// </summary>
        /// <exception cref="FileNotFoundException">Target file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        public virtual void LoadFromTarget()
        {
            if (!Target.Exists)
                throw new FileNotFoundException("Cannot load data. Target file does not exist.", Target.FullName);

            using FileStream fs = Target.OpenRead();

            byte[] bytes = new byte[ExtensionData.StructureSize];
            fs.Read(bytes, 0, bytes.Length);

            Data = new(bytes);
        }
    }
    /// <summary>
    /// File containing an unsigned Artemis extension information section.
    /// </summary>
    public sealed class UnsignedInformationFile : InformationFile
    {
        /// <summary>
        /// The file extension for unsigned information files.
        /// </summary>
        public static readonly string FileExtension = ".ainf";
        /// <summary>
        /// The default signature author string for an unsigned instance.
        /// </summary>
        public static readonly string DefaultSignatureAuthor = "NOT_SIGNED";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public UnsignedInformationFile(FileInfo target) : base(target) { }
        public UnsignedInformationFile(FileInfo target, ExtensionData data) : base(target, data) { }

        /// <summary>
        /// Loads data from the target. The target must be unsigned.
        /// </summary>
        /// <exception cref="FileNotFoundException">Target file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="InvalidOperationException">Target file is signed.</exception>
        public override void LoadFromTarget()
        {
            base.LoadFromTarget();
            if (Data.SignatureAuthor != DefaultSignatureAuthor || !Helpers.IsEmpty(Data.Signature))
                throw new InvalidOperationException("The loaded data is signed, which does not qualify for this object.");
        }

        /// <summary>
        /// Signs the information file with a signature generated from the provided binary and private key.
        /// </summary>
        /// <param name="key">The private key to generate a signature with.</param>
        /// <param name="binary">The binary to sign.</param>
        /// <returns>The signed information file.</returns>
        /// <exception cref="CryptographicException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public SignedInformationFile Sign(PrivateKey key, BinaryFile binary)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(key.RsaKey, out _);
            _data.SignatureAuthor = key.Author;
            _data.Signature = rsa.SignData(binary.Bytes, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            return new SignedInformationFile(new(Target.Directory?.FullName + "\\" + _data.Name + SignedInformationFile.FileExtension), _data);
        }
        /// <summary>
        /// Signs the information file with a signature generated from the provided binary and private key and checks out the current instance.
        /// </summary>
        /// <param name="key">The private key to generate a signature with.</param>
        /// <param name="binary">The binary to sign.</param>
        /// <returns>The signed information file.</returns>
        /// <remarks>Checking out in this instance implies deleting the unsigned information file and saving the new signed information file to disk.</remarks>
        /// <exception cref="CryptographicException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public SignedInformationFile SignAndCheckout(PrivateKey key, BinaryFile binary)
        {
            SignedInformationFile file = Sign(key, binary);
            file.SaveToTarget();

            if (Target.Exists)
                Target.Delete();

            return file;
        }
    }
    /// <summary>
    /// File containing a signed Artemis extension information section.
    /// </summary>
    public sealed class SignedInformationFile : InformationFile
    {
        public static readonly string FileExtension = ".anfx";

        public SignedInformationFile(FileInfo target) : base(target) { }
        public SignedInformationFile(FileInfo target, ExtensionData data) : base(target, data) { }

        /// <summary>
        /// Loads data from the target. The target must be signed.
        /// </summary>
        /// <exception cref="FileNotFoundException">Target file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="InvalidOperationException">Target file is unsigned.</exception>
        public override void LoadFromTarget()
        {
            base.LoadFromTarget();
            if (Data.SignatureAuthor == UnsignedInformationFile.DefaultSignatureAuthor || Helpers.IsEmpty(Data.Signature))
                throw new InvalidOperationException("The loaded data is not signed, which does not qualify for this object.");
        }

        /// <summary>
        /// Verifies the vadility of the binary signature.
        /// </summary>
        /// <param name="key">The public RSA key.</param>
        /// <param name="binary">The binary file.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> and <paramref name="binary"/> match the current instance <see cref="ExtensionData.SignatureAuthor"/> and <see cref="ExtensionData.Signature"/>, otherwise <see langword="false"/>.</returns>
        /// <exception cref="CryptographicException"></exception>
        public bool Verify(PublicKey key, BinaryFile binary)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(key.RsaKey, out _);
            if (_data.SignatureAuthor == key.Author && rsa.VerifyData(binary.Bytes, _data.Signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1))
                return true;
            return false;
        }

        /// <summary>
        /// Removes the signature from the information file.
        /// </summary>
        /// <returns>The unsigned information file.</returns>
        /// <exception cref="PathTooLongException"></exception>
        public UnsignedInformationFile Unsign()
        {
            _data.SignatureAuthor = UnsignedInformationFile.DefaultSignatureAuthor;
            Helpers.Initialize(_data.Signature, 0);
            return new UnsignedInformationFile(new(Target.Directory?.FullName + "\\" + _data.Name + UnsignedInformationFile.FileExtension), _data);
        }
        /// <summary>
        /// Removes the signature from the information file and checks out the current instance.
        /// </summary>
        /// <returns>The unsigned information file.</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public UnsignedInformationFile UnsignAndCheckout()
        {
            UnsignedInformationFile file = Unsign();
            file.SaveToTarget();

            if (Target.Exists)
                Target.Delete();

            return file;
        }
    }
}
