using System;
using System.Threading;

namespace ConsoleProgressBar.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();

            using (var pb = new ProgressBar())
            {
                using (var p1 = pb.Progress.Fork(0.33, "Task 1"))
                {
                    p1.Fork(0.1, "Hello there!").Dispose();
                    Thread.Sleep(500);

                    using (var p11 = p1.Fork(0.8))
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            p11.Report(i/11d, $"Doing a lot of stuff: {i}/11");
                            Thread.Sleep(300);
                        }
                    }
                }

                using (var p2 = pb.Progress.Fork(0.66))
                {
                    for (int i = 0; i < 33; i++)
                    {
                        p2.Report(i / 100d, "Installing...");
                        Thread.Sleep(100);
                    }

                    for (int i = 33 - 1; i >= 0; i--)
                    {
                        p2.Report(i / 100d, "Rolling back...");
                        Thread.Sleep(100);
                    }
                }
            }

            Console.WriteLine("Press any key");
            Console.ReadLine();
        }
    }
}
