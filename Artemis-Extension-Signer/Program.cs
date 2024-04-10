using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Artemis_Extension_Signer
{
    // Takes an unsigned information file and binary and signs it with a private key.
    // Input files: ainf, dll and akyx
    // Output file: anfx

    static class Helpers
    {
        public static void Initialize(byte[] buffer, byte value)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = value;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ArtemisExtensionData
    {
        private const int SignatureSize = 64;
        private const int PadSize = 128;

        public const int StructureSize = 1024;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string ArtemisVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Author;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string Description;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PadSize, ArraySubType = UnmanagedType.U1)]
        public byte[] Pad;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string SignatureAuthor;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SignatureSize, ArraySubType = UnmanagedType.U1)]
        public byte[] Signature;

        public ArtemisExtensionData()
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
        public ArtemisExtensionData(byte[] bytes)
        {
            nint ptr = nint.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(StructureSize);
                Marshal.Copy(bytes, 0, ptr, StructureSize);
                this = Marshal.PtrToStructure<ArtemisExtensionData>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        public ArtemisExtensionData(FileInfo file)
        {
            if (!file.Exists)
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
                return;
            }

            using FileStream fs = file.OpenRead();

            byte[] bytes = new byte[StructureSize];
            fs.Read(bytes, 0, StructureSize);

            nint ptr = nint.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(StructureSize);
                Marshal.Copy(bytes, 0, ptr, StructureSize);
                this = Marshal.PtrToStructure<ArtemisExtensionData>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

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

        public readonly void WriteToFile(FileInfo file)
        {
            if (file.Exists)
                file.Delete();

            using FileStream fs = file.OpenWrite();

            byte[] bytes = Bytes;
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    internal class Program
    {
        static void SignInformationFile(FileInfo informationFile, FileInfo binaryFile, FileInfo privateKeyFile)
        {
            if (!informationFile.Exists)
                throw new ArgumentException($"Information file does not exist at '{informationFile.FullName}'.");
            if (!binaryFile.Exists)
                throw new ArgumentException($"Binary file does not exist at '{binaryFile.FullName}'.");
            if (!privateKeyFile.Exists)
                throw new ArgumentException($"Private key file does not exist at '{privateKeyFile.FullName}'.");

            byte[] data;

            using (FileStream fs = binaryFile.OpenRead())
            {
                data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
            }

            string signatureAuthor;
            byte[] privateKey;

            using (FileStream fs = privateKeyFile.OpenRead())
            {
                using StreamReader sr = new(fs);
                signatureAuthor = Encoding.ASCII.GetString(Convert.FromBase64String(sr.ReadLine()));
                privateKey = Convert.FromBase64String(sr.ReadLine());
            }

            ArtemisExtensionData extensionData = new(informationFile);

            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(privateKey, out _);
                extensionData.SignatureAuthor = signatureAuthor;
                extensionData.Signature = rsa.SignData(data, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            }

            extensionData.WriteToFile(new(informationFile.DirectoryName + $"\\{extensionData.Name}.anfx"));
        }
        static void Main(string[] args)
        {
            SignInformationFile(new("C:\\Users\\dougl\\Desktop\\TestProject.ainf"), new("C:\\Users\\dougl\\Desktop\\kdcom.dll"), new("C:\\Users\\dougl\\Desktop\\Keys\\private.akyx"));
        }
    }
}
