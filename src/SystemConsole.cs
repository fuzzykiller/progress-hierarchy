using System;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleProgressBar
{
#if HAVE_EXCLUDE_FROM_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
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