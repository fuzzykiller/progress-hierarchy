using System;

namespace ConsoleProgressBar
{
    internal class SystemConsole : IConsole
    {
        public int CursorTop => Console.CursorTop;
        public int CursorLeft => Console.CursorLeft;
        public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);
        public void WriteLine() => Console.WriteLine();
        public void WriteLine(string value) => Console.WriteLine(value);
        public void Write(string value) => Console.Write(value);
    }
}