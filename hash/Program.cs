using System;

namespace hash
{
    public class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                string commandLine = Console.ReadLine();
                CommandLine parsed = Shell.GetCommandLine(commandLine);
                
                Console.WriteLine("Command: {0}", parsed.CommandName);
                for (int i = 0; i < parsed.Args.Count; i++)
                {
                    Console.WriteLine("Arg: {0} -> {1}", parsed.Args[i].Key, parsed.Args[i].Value);
                }
            }
        }
    }
}