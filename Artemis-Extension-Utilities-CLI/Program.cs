namespace Artemis.Extension.Utilities.CLI
{
    internal class Target
    {
        public static readonly DirectoryInfo DefaultRoot = new($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Artemis\\Utilities\\Projects");

        public FileInfo RootFile { get; } = new FileInfo($"{DefaultRoot.Parent?.FullName ?? ""}\\root.acfg");

        private DirectoryInfo root;
        public DirectoryInfo Root
        {
            get => root;
            set
            {
                root = value;

                RootFile.Delete();
                using FileStream fs = RootFile.OpenWrite();
                using StreamWriter sw = new(fs);
                sw.WriteLine(value.FullName);
            }
        }
        public string? SubTarget { get; set; }

        public Target()
        {
            if (RootFile.Exists)
            {
                using FileStream fs = RootFile.OpenRead();
                using StreamReader sr = new(fs);
                root = new DirectoryInfo(sr.ReadLine() ?? "");
            }
            else root = DefaultRoot;
        }

        public override string ToString() => Root.Name + (SubTarget != null ? $"\\{SubTarget}" : "");
    }
    internal class Command
    {
        public string Name { get; }
        public string[] Parameters { get; }

        public Command(string command)
        {
            string[] split = command.Split(' ');
            List<string> parsed = [];

            Name = split[0].ToUpper();

            for (int i = 1; i < split.Length; i++)
            {
                if (split[i][0] == '"')
                {
                    string concat = split[i][1..] + ' ';
                    
                    for (i++; split[i].Last() != '"'; i++)
                    {
                        concat += split[i] + ' ';
                    }

                    i++;
                    concat += split[i];
                    parsed.Add(concat);
                    continue;
                }

                parsed.Add(split[i]);
            }

            Parameters = [.. parsed];
        }
    }
    internal static class Program
    {
        static void PrintHeader()
        {
            Console.WriteLine("Artemis Extension Utilities (Version 1.0)");
            Console.WriteLine("(c) Artemis Group. This project is licensed under the MIT license.");
            Console.WriteLine("https://github.com/ArtemisDevGroup/Artemis-Extension-Utilities/blob/main/LICENSE.md");
        }

        static void Main(string[] args)
        {
            Target target = new();

            PrintHeader();

            while (true)
            {
                Console.WriteLine();
                Console.Write($"{target}> ");

                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Input cannot be null or empty. Refer to 'help' for a list of available commands.");
                    continue;
                }

                Command cmd = new(input);

                switch (cmd.Name)
                {
                    case "CLEAR":
                    case "CLS":
                        Console.Clear();
                        PrintHeader();
                        break;
                    case "TARGET":  // Commands interacting with the target project of the applicaton.
                        switch (cmd.Parameters[0].ToUpper())
                        {
                            case "GET": // Gets the current path of the target project.
                                break;
                            case "SET": // Sets the current project. This command only accepts folder names that are contained inside of the current root.
                                break;
                            case "LIST": // Lists all projects inside of the current root.
                                break;
                        }
                        break;
                    case "ROOT":    // Commands interacting with the root folder of the application.
                        switch (cmd.Parameters[0].ToUpper())
                        {
                            case "GET": // Gets the current root directory.
                                Console.WriteLine($"The current root directory target is: '{target.Root.FullName}'");
                                break;
                            case "SET": // Sets the current root directory.
                                target.Root = new DirectoryInfo(cmd.Parameters[1]);
                                Console.WriteLine($"Set the current root directory to: '{target.Root.FullName}'");
                                break;
                            case "MOVE":    // Moves everything in the current root directory to a different directory and sets the current root directory to it.
                                target.Root.MoveTo(cmd.Parameters[1]);
                                target.Root = new DirectoryInfo(cmd.Parameters[1]);
                                break;
                        }
                        break;
                    case "EXIT":    // Exits the application.
                        return;
                    default:
                        Console.WriteLine($"'{input}' is not a recognized internal command. Refer to 'help' for a list of available commands.");
                        break;
                }
            }
        }
    }
}