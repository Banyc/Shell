using System;

namespace Shell.Helpers
{
    public static class ConsoleHelper
    {
        public static void WriteLine(string toPrint, ConsoleColor color)
        {
            Write(toPrint, color);
            Console.WriteLine();
        }

        public static void Write(string toPrint, ConsoleColor color)
        {
            // change color
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            // print
            Console.Write(toPrint);
            // restore the previous color
            Console.ForegroundColor = defaultColor;
        }

        public static string ReadLine(ConsoleColor color)
        {
            // change color
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            // read
            string readline = Console.ReadLine();
            // restore the previous color
            Console.ForegroundColor = defaultColor;
            
            return readline;
        }
    }
}
