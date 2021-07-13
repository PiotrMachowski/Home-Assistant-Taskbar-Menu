using System;
using System.Globalization;

namespace Home_Assistant_Taskbar_Menu.Utils
{
    public static class ConsoleWriter
    {
        public static void WriteLine(string s, ConsoleColor color, bool addPrefix = true, bool align = false)
        {
            Write(s, color, addPrefix, align, true);
        }

        public static void Write(string s, ConsoleColor color, bool addPrefix = true, bool align = false,
            bool breakLine = false)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            var prefix = addPrefix ? $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] " :
                align ? new string(' ', 22) : "";
            Console.Write($"{prefix}{s}");
            if (breakLine)
            {
                Console.WriteLine();
            }

            Console.ForegroundColor = oldColor;
        }
    }
}