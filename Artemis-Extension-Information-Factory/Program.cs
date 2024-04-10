using System.Runtime.InteropServices;

namespace Artemis_Extension_Information_Factory
{
    // Creates an Artemis extension information file with author data.
    // Output file: ainf // Artemis Extension Information File
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
        static string PromptUser(string prompt, int maxLength)
        {
            while (true)
            {
                Console.Clear();

                Console.Write(prompt + ": ");
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Input cannot be null or empty.");
                    Console.ReadKey();
                    continue;
                }

                if (input.Length > maxLength)
                {
                    Console.WriteLine($"Input cannot be more than {maxLength} characters.");
                    Console.ReadKey();
                    continue;
                }

                Console.WriteLine("Is your input correct? [y/N]");
                string? correct = Console.ReadLine();

                if (string.IsNullOrEmpty(correct))
                    continue;
                if (correct[0] == 'y' || correct[0] == 'Y')
                    return input;
            }
        }

        static void Selection1()
        {
            ArtemisExtensionData data = new();
            data.ArtemisVersion = "ARTEMIS_VERSION_V1.0.0";
            data.Name = PromptUser("Enter the project name", 64 - 1);
            data.Author = PromptUser("Enter your name/psuedonym to be listed as author", 64 - 1);
            data.Description = PromptUser("Enter an ample description for the project", 512 - 1);
            data.Version = PromptUser("Enter the project version", 64 - 1);
            data.SignatureAuthor = "NOT_SIGNED";
            Console.Clear();
            data.WriteToFile(new FileInfo(data.Name + ".ainf"));
            Console.WriteLine($"Successfully created Artemis Extension Information File for project '{data.Name}'.");
            Console.WriteLine($"The file has been saved as '{data.Name}.ainf' in the process' root directory.");
            Console.ReadKey();
        }
        static void Selection2()
        {
            ArtemisExtensionData data = new(new FileInfo("TestProject.ainf"));
            Console.WriteLine(data.ArtemisVersion);
            Console.WriteLine(data.Name);
            Console.WriteLine(data.Author);
            Console.WriteLine(data.Description);
            Console.WriteLine(data.Version);
            Console.WriteLine(data.SignatureAuthor);
            Console.Write("{ ");
            for (int i = 0; i < data.Signature.Length - 1; i++)
                Console.Write($"0x{data.Signature[i]:X2}, ");
            Console.WriteLine($"0x{data.Signature.Last():X2} " + '}');
        }
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();

                Console.WriteLine("Welcome to the Artemis Extension Information File Factory!");
                Console.WriteLine("[1] Create new Information File");
                Console.WriteLine("[2] TEST");
                Console.Write("Selection: ");
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Selection cannot be null or empty.");
                    Console.ReadKey();
                    continue;
                }

                switch (input[0])
                {
                    case '1':
                        Selection1();
                        break;
                    case '2':
                        Selection2();
                        break;
                    default:
                        Console.WriteLine("Invalid selection.");
                        Console.ReadKey();
                        continue;
                }

                break;
            }
        }
    }
}