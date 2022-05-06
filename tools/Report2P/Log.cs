using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report2P
{
    public static class Log
    {
        public static void Warn(string message) => WriteLine(message, ConsoleColor.Yellow);
        public static void Info(string message) => WriteLine(message, ConsoleColor.White);
        public static void Debug(string message) => WriteLine(" » " + message, ConsoleColor.DarkGray);

        public static void WriteLine(string message, ConsoleColor? color = null)
        {
            if (color is not null)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                Console.WriteLine(message);
                Console.ForegroundColor = originalColor;
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
