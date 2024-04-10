using System.Security.Cryptography;
using System.Text;

namespace Artemis_Extension_Keypair_Generator
{
    // Generates a public and private keypair, and writes them to their respective files.
    // Output files: public.akey and private.akyx // Artemis Extension Public Key File and Artemis Extension Private Key File

    internal class Program
    {
        static void CreateNewKeypair(DirectoryInfo directory, string keypairOwner)
        {
            FileInfo publicKeyFile = new(directory.FullName + "\\public.akey");
            if (publicKeyFile.Exists)
                publicKeyFile.Delete();

            FileInfo privateKeyFile = new(directory.FullName + "\\private.akyx");
            if (privateKeyFile.Exists)
                privateKeyFile.Delete();

            string encodedKeypairOwner = Convert.ToBase64String(Encoding.ASCII.GetBytes(keypairOwner));

            using RSA rsa = RSA.Create();

            using (FileStream fs = publicKeyFile.OpenWrite())
            {
                using StreamWriter sw = new(fs);
                sw.WriteLine(encodedKeypairOwner);
                sw.WriteLine(Convert.ToBase64String(rsa.ExportRSAPublicKey()));
            }

            using (FileStream fs = privateKeyFile.OpenWrite())
            {
                using StreamWriter sw = new(fs);
                sw.WriteLine(encodedKeypairOwner);
                sw.WriteLine(Convert.ToBase64String(rsa.ExportRSAPrivateKey()));
            }
        }

        static void Main(string[] args)
        {
            CreateNewKeypair(Directory.CreateDirectory("C:\\Users\\dougl\\Desktop\\Keys"), "Astrea");
        }
    }
}
