
using System;
using System.IO;
using System.Threading;

namespace Nilfca {

    public static class Program {

        public static void Main (string[] args) {

            Console.WriteLine("Starting! ..");
            Console.WriteLine();
            Console.WriteLine("OS: " + Environment.OSVersion.ToString().Replace("Unix", "GNU/Linux"));
            Console.WriteLine("Machine Type: " + (Environment.Is64BitOperatingSystem ? "amd64" : "i386"));
            Console.WriteLine("Machine Name: " + Environment.MachineName);
            Console.WriteLine("Running in CLI: " + Environment.CommandLine);
            Console.WriteLine("Running in C# Build: " + Environment.Version);
            Console.WriteLine("Working Set: " + Environment.WorkingSet);
            Console.WriteLine("Working in: " + Environment.CurrentDirectory);
            Console.WriteLine();
            Console.WriteLine("    _   _ _ _  __           ");
            Console.WriteLine("   | \\ | (_) |/ _| ___ __ _ ");
            Console.WriteLine("   |  \\| | | | |_ / __/ _` |");
            Console.WriteLine("   | |\\  | | |  _| (_| (_| |");
            Console.WriteLine("   |_| \\_|_|_|_|  \\___\\__,_|");
            Console.WriteLine("                             2022");
            Console.WriteLine();
            Console.WriteLine("A Networked Interface Layer for the Discord API.");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Loading Discord-Token..");

            if (!File.Exists("token.txt")) Abort("Failed to read token.txt, File.Exist returned false!");
            Discord.SetToken(File.ReadAllLines("token.txt")[0]);

            Console.WriteLine("Starting Discord Thread..");
            new Thread(()=>Discord.Start()).Start();

            Console.WriteLine("Starting Server..");
            Server.Start();
        }

        private static void Abort (string reason) {

            Console.WriteLine(reason);
            Environment.Exit(-1);
        }
    }
}
