using System;
using System.Threading;

namespace ConsoleProgressBar.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var pb = new ProgressBar())
            {
                for (int i = 0; i < 100000; i++)
                {
                    pb.Progress.Report(i / 100000d, "Hallo Welt");
                    Thread.Sleep(1);
                }
            }

            Console.ReadLine();
        }
    }
}
