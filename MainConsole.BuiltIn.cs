using System;
using System.IO;
using Shell.Helpers;

namespace Shell
{
    // built-in functions
    public partial class MainConsole
    {
        private void Exit(string[] tokens)
        {
            if (tokens[0] != "exit")
                return;
            if (tokens.Length > 1)
                Environment.Exit(int.Parse(tokens[1]));
            else
                Environment.Exit(0);
        }
        private void Cd(string[] tokens)
        {
            if (tokens[0] != "cd")
                return;
            if (tokens.Length > 2)
            {
                Console.WriteLine("-bash: cd: too many arguments");
                return;
            }
            if (tokens.Length == 1)
                _pwd = "./";
            else if (tokens.Length == 2)
            {
                string newPath = Path.Combine(_pwd, tokens[1]);
                if (!Directory.Exists(newPath))
                {
                    ConsoleHelper.WriteLine($"[Error] Path not exists", ConsoleColor.Red);
                    return;
                }
                _pwd = newPath;
            }
        }
        private void Setenv(string[] tokens)
        {
            if (tokens[0] == "setenv" && tokens.Length > 2)
                Environment.SetEnvironmentVariable(tokens[1], tokens[2]);
        }
    }
}
