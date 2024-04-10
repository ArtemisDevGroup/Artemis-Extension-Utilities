using System.Runtime.InteropServices;

namespace Artemis_Extension_Packer
{
    internal class Program
    {
        static void PackExtension(FileInfo informationFile, FileInfo binaryFile, FileInfo extensionFile)
        {
            if (!informationFile.Exists)
                throw new ArgumentException($"Information file does not exist at '{informationFile.FullName}'.");
            if (informationFile.Length != 1024)
                throw new FormatException($"Information file has the wrong format.");

            if (!binaryFile.Exists)
                throw new ArgumentException($"Binary file does not exist at '{binaryFile.FullName}'.");

            if (extensionFile.Exists)
                extensionFile.Delete();

            byte[] informationSection;

            using (FileStream fs = informationFile.OpenRead())
            {
                informationSection = new byte[fs.Length];
                fs.Read(informationSection, 0, informationSection.Length);
            }

            byte[] binarySection;

            using (FileStream fs = binaryFile.OpenRead())
            {
                binarySection = new byte[fs.Length];
                fs.Read(binarySection, 0, binarySection.Length);
            }

            using (FileStream fs = extensionFile.OpenWrite())
            {
                fs.Write(informationSection, 0, informationSection.Length);
                fs.Write(binarySection, 0, binarySection.Length);
            }
        }

        static void Main(string[] args)
        {
            PackExtension(new("C:\\Users\\dougl\\Desktop\\TestProject.anfx"), new("C:\\Users\\dougl\\Desktop\\kdcom.dll"), new("C:\\Users\\dougl\\Desktop\\TestProject.aext"));
        }
    }
}
