using System;
using System.Threading;

namespace ConsoleProgressBar
{
    /// <summary>
    /// Represents a console progress bar.
    /// </summary>
    public class ProgressBar : IDisposable
    {
        private readonly IConsole _console = new SystemConsole();
        private readonly object _syncRoot = new object();
        private ProgressChangedEventArgs _lastEventArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressBar"/> class.
        /// </summary>
        public ProgressBar()
        {
            Progress = new Progress();
            Progress.ProgressChanged += ProgressOnProgressChanged;
        }

        /// <summary>
        /// Whether to show the associated message (if any) on a separate line
        /// </summary>
        public bool StatusOnSeparateLine { get; set; }
        
        private void ProgressOnProgressChanged(object sender, ProgressChangedEventArgs eventArgs)
        {
            var previousEventArgs = _lastEventArgs;
            
            // Skip unchanged progress within 0.01 %
            if (Equals(previousEventArgs, eventArgs)) return;

            // Update remembered event args unless some other thread was faster
            Interlocked.CompareExchange(ref _lastEventArgs, eventArgs, previousEventArgs);

            // Simply discard new progress messages when we’re still busy writing the previous one
            if (!Monitor.TryEnter(_syncRoot)) return;
            try
            {
                ProgressBarRenderer.Render(_console, eventArgs, StatusOnSeparateLine);
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }
        }
        
        internal ProgressBar(IConsole console)
        {
            _console = console;
        }

        /// <summary>
        /// Gets the progress bar’s root progress scope
        /// </summary>
        public Progress Progress { get; }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Perform specific tasks to free resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called from <see cref="Dispose()"/>, <c>false</c> otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Progress.Dispose();
                _console.WriteLine();
            }
        }

        private static bool Equals(ProgressChangedEventArgs a, ProgressChangedEventArgs b)
        {
            if (ReferenceEquals(a, b)) return true;

            // Either is NULL but not both?
            if (a == null ^ b == null) return false;

            // Tolerance: 0.01 %
            if (Math.Abs(a.Progress - b.Progress) > 0.0001) return false;

            if (a.Messages.Count != b.Messages.Count) return false;

            // Optimization: Message arrays are more like to differ near the end
            for (var i = a.Messages.Count - 1; i >= 0; i--)
            {
                if (a.Messages[i] != b.Messages[i]) return false;
            }

            return true;
        }
    }

    internal static class ProgressBarRenderer
    {
        public static void Render(IConsole console, ProgressChangedEventArgs eventArgs, bool statusOnSeparateLine)
        {
            console.SetCursorPosition(0, console.CursorTop);
            console.Write(string.Format("{0,10:P} {1}", eventArgs.Progress, string.Join(" – ", eventArgs.Messages)));
        }
    }
}

