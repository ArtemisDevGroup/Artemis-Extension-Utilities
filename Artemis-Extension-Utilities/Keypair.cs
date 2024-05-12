using System.Text;

namespace Artemis.Extension.Utilities
{
    /// <summary>
    /// An authored RSA key.
    /// </summary>
    public class Key
    {
        /// <summary>
        /// The author of the RSA key.
        /// </summary>
        public required string Author { get; init; }
        /// <summary>
        /// The RSA key.
        /// </summary>
        public required byte[] RsaKey { get; init; }
    }
    /// <summary>
    /// A private authored RSA key.
    /// </summary>
    public class PrivateKey : Key { }
    /// <summary>
    /// A public authored RSA key.
    /// </summary>
    public class PublicKey : Key { }

    /// <summary>
    /// A file storing a public or private RSA key instance.
    /// </summary>
    /// <param name="target">The RSA key file information.</param>
    public class KeyFile(FileInfo target) : FileBase(target)
    {
        /// <summary>
        /// The default file extension for a public RSA key file.
        /// </summary>
        public static readonly string PublicKeyExtension = ".akey";
        /// <summary>
        /// The default file extension for a private RSA key file.
        /// </summary>
        public static readonly string PrivateKeyExtension = ".akyx";

        /// <summary>
        /// Loads a key from the target.
        /// </summary>
        /// <returns>The loaded RSA key.</returns>
        /// <exception cref="FileNotFoundException">Target does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="DecoderFallbackException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        public Key LoadKey()
        {
            if (!Target.Exists)
                throw new FileNotFoundException("Target does not exist.", Target.FullName);

            using FileStream fs = Target.OpenRead();
            using StreamReader sr = new(fs);

            string author = Encoding.ASCII.GetString(Convert.FromBase64String(sr.ReadLine()));
            byte[] key = Convert.FromBase64String(sr.ReadLine());

            return new() { Author = author, RsaKey = key };
        }
        /// <summary>
        /// Loads a key as public from the target.
        /// </summary>
        /// <returns>The loaded public key.</returns>
        /// <exception cref="InvalidOperationException">The target key file is not a public key file.</exception>
        /// <exception cref="FileNotFoundException">Target does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="DecoderFallbackException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        public PublicKey LoadKeyAsPublic()
        {
            if (Target.Extension != PublicKeyExtension)
                throw new InvalidOperationException("Cannot load key as public: target has an invalid format.");
            return (PublicKey)LoadKey();
        }
        /// <summary>
        /// Loads a key as private from the target.
        /// </summary>
        /// <returns>The loaded private key.</returns>
        /// <exception cref="InvalidOperationException">The target key file is not a private key file.</exception>
        /// <exception cref="FileNotFoundException">Target does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="DecoderFallbackException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        public PrivateKey LoadKeyAsPrivate()
        {
            if (Target.Extension != PrivateKeyExtension)
                throw new InvalidOperationException("Cannot load key as private: target has an invalid format.");
            return (PrivateKey)LoadKey();
        }

        /// <summary>
        /// Saves a key to the target.
        /// </summary>
        /// <param name="key">The key to save.</param>
        /// <remarks>If the target already exists, it will be overwritten.</remarks>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        public void SaveKey(Key key)
        {
            if (Target.Exists)
                Target.Delete();

            using FileStream fs = Target.OpenWrite();
            using StreamWriter sw = new(fs);

            sw.WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(key.Author)));
            sw.WriteLine(Convert.ToBase64String(key.RsaKey));
        }
        /// <summary>
        /// Saves a key as public to the target.
        /// </summary>
        /// <param name="key">The public key to save.</param>
        /// <exception cref="InvalidOperationException">The target key file is not a public key file.</exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        public void SaveKeyAsPublic(PublicKey key)
        {
            if (Target.Extension != PublicKeyExtension)
                throw new InvalidOperationException("Cannot save key as public: target has an invalid format.");
            SaveKey(key);
        }
        /// <summary>
        /// Saves a key as private to the target.
        /// </summary>
        /// <param name="key">The private key to save.</param>
        /// <exception cref="InvalidOperationException">The target key file is not a private key file.</exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        public void SaveKeyAsPrivate(PrivateKey key)
        {
            if (Target.Extension != PrivateKeyExtension)
                throw new InvalidOperationException("Cannot save key as private: target has an invalid format.");
            SaveKey(key);
        }
    }
}
