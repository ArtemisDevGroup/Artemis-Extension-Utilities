using System.Text;

namespace Artemis.Extension.Utilities
{
    class KeyWebService
    {
        public static readonly Uri KeyDirectoryBaseAddress = new("https://www.example.com/pkeys/");
        public static HttpClient Client { get; } = new() { BaseAddress = KeyDirectoryBaseAddress };

        public static PublicKey Fetch(string keyAuthor)
        {
            using Stream response = Client.GetStreamAsync($"{keyAuthor}.akey").Result;
            using StreamReader sr = new(response);

            string author = Encoding.ASCII.GetString(Convert.FromBase64String(sr.ReadLine()));
            byte[] rsaBytes = Convert.FromBase64String(sr.ReadLine());

            return new PublicKey() { Author = author, RsaKey = rsaBytes };
        }
        public static List<PublicKey> FetchAll()
        {
            List<PublicKey> keys = [];
            string[] authors = [];

            using (Stream response = Client.GetStreamAsync("keys.txt").Result)
            {
                using StreamReader sr = new(response);
                authors = sr.ReadToEnd().Split('\n');
            }

            foreach (string author in authors)
            {
                keys.Add(Fetch(author));
            }

            return keys;
        }
    }
}
