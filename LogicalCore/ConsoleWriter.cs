using System;
using System.Text;

namespace LogicalCore
{
    /// <summary>
    /// Попытка создать потокобезопасный класс для выведения данных в консоль, позволяющий сразу же задавать цвет текста.
    /// </summary>
    public static class ConsoleWriter
    {
        private static readonly object _MessageLock = new object();

        static ConsoleWriter()
        {
            Console.OutputEncoding = Encoding.UTF8;
            WriteLine("Установленная кодировка вывода в консоль: " + Console.OutputEncoding.EncodingName, ConsoleColor.White, ConsoleColor.DarkGray);
        }

        public static void WriteLine()
        {
            lock (_MessageLock)
            {
                Console.WriteLine();
            }
        }

        public static void WriteLine<T>(T message, ConsoleColor foreground = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black)
        {
            lock (_MessageLock)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        public static void Write<T>(T message, ConsoleColor foreground = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black)
        {
            lock (_MessageLock)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                Console.Write(message);
                Console.ResetColor();
            }
        }
    }
}
