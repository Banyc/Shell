using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.InteropServices;
using Shell.Helpers;

namespace Shell
{
    public partial class MainConsole
    {
        string _pwd = "./";
        List<string> _pathVariables = new List<string>();

        public void MainEntry(string[] args)
        {
            string path = Environment.GetEnvironmentVariable("PATH");
            // for Windows
            _pathVariables.AddRange(path.Split(';'));

            while (true)
            {
                _pwd = Path.GetFullPath(_pwd).TrimEnd('\\');
                ConsoleHelper.Write(string.Format($"{_pwd}$ "), ConsoleColor.White);

                // read
                string readLine = ConsoleHelper.ReadLine(ConsoleColor.Yellow);  // not including '\n'
                // parse
                // string[] tokens = readLine.Split();
                string[] tokens = Regex.Split(readLine, "\\s+(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                int i;
                for (i = 0; i < tokens.Length; i++)
                {
                    tokens[i] = tokens[i].Trim('\"');
                }
                // execute
                CheckCommand(tokens);
            }
        }

        private void CheckCommand(string[] tokens)
        {
            // Aliases
            // TODO

            // build-in command
            switch (tokens[0])
            {
                case "exit":
                    // exit
                    Exit(tokens);
                    return;
                case "cd":
                    // cd
                    Cd(tokens);
                    return;
                case "setenv":
                    // setenv
                    Setenv(tokens);
                    return;
            }

            // pipe
            HandlePine(tokens);
        }

        private void HandlePine(string[] tokens)
        {
            int start = 0;
            int foundIndex;
            string[] partition;
            StreamReader lastOutput = null;
            while (start < tokens.Length)
            {
                foundIndex = Array.FindIndex(tokens, start, x => x == "|");

                if (foundIndex < 0)
                {
                    foundIndex = tokens.Length;
                }

                partition = new string[foundIndex - start];
                Array.Copy(tokens, partition, foundIndex - start);

                bool isReturnOutput = true;
                if (foundIndex >= tokens.Length)
                    isReturnOutput = false;

                // PATH
                // string path = Environment.GetEnvironmentVariable(tokens[0]);
                lastOutput = LaunchOneProcess(partition, lastOutput, isReturnOutput);

                start = foundIndex + 1;
            }
        }

        private StreamReader LaunchOneProcess(string[] tokens, StreamReader standardInput, bool isReturnOutput)
        {
            string fullFilePath;
            StreamReader standardOutput = null;

            // check ./
            fullFilePath = Path.Combine(_pwd, tokens[0]);

            try 
            {
                standardOutput = StartProcess(fullFilePath, tokens, standardInput, isReturnOutput);
            }
            catch (ExternalException)
            {
                // check PATH
                bool isFound = false;
                foreach (string directory in _pathVariables)
                {
                    fullFilePath = Path.Combine(directory, tokens[0]);
                    if (isFound)
                    {
                        return standardOutput;
                    }
                    try
                    {
                        standardOutput = StartProcess(fullFilePath, tokens, standardInput, isReturnOutput);
                        isFound = true;
                    }
                    catch (ExternalException) {}
                }
                if (!isFound)
                {
                    ConsoleHelper.WriteLine($"[Error] \"{tokens[0]}\" not found.", ConsoleColor.Red);
                    return standardOutput;
                }
            }
            return standardOutput;
        }

        private StreamReader StartProcess(string fullFilePath, string[] tokens, StreamReader standardInput, bool isReturnOutput)
        {
            Process process = new Process();
            // Configure the process using the StartInfo properties.
            process.StartInfo.FileName = fullFilePath;
            bool isFirstToken = true;
            foreach (string token in tokens)
            {
                if (isFirstToken)
                {
                    isFirstToken = false;
                    continue;
                }
                process.StartInfo.ArgumentList.Add(token);
            }
            process.StartInfo.UseShellExecute = false;
            if (isReturnOutput)
                process.StartInfo.RedirectStandardOutput = true;
            if (standardInput != null)
                process.StartInfo.RedirectStandardInput = true;
            process.Start();
            // push the output from the previous pipe into this process
            if (standardInput != null)
                process.StandardInput.Write(standardInput.ReadToEnd());
            process.WaitForExit();// Waits here for the process to exit.
            
            if (isReturnOutput)
                return process.StandardOutput;
            return null;
        }
    }
}
