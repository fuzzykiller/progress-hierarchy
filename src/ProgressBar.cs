using System;
using System.Collections.Generic;
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
        private double _width;
        private int _initialCursorTop = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressBar"/> class.
        /// </summary>
        public ProgressBar()
        {
            Width = 0.6;
            Progress = new Progress();
            Progress.ProgressChanged += ProgressOnProgressChanged;
        }

        internal ProgressBar(IConsole console)
        {
            _console = console;
        }

        /// <summary>
        /// Whether to show the associated message (if any) on a separate line
        /// </summary>
        public bool StatusOnSeparateLine { get; set; }

        /// <summary>
        /// Gets or sets the width (percent) of the progress bar. Larger values may hide messages. Consider using <see cref="StatusOnSeparateLine"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value was smaller than 0 or larger than 1 when setting.</exception>
        public double Width
        {
            get => _width;
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Width is smaller than 0 or larger than 1.");
                }

                _width = value;
            }
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

                if (_initialCursorTop != -1)
                {
                    var verticalOffset = (StatusOnSeparateLine ? 2 : 1);
                    _console.SetCursorPosition(0, _initialCursorTop + verticalOffset);
                }
            }
        }

        /// <summary>
        /// Render progress bar to console.
        /// </summary>
        /// <param name="eventArgs">Progress event to render</param>
        protected virtual void Render(ProgressChangedEventArgs eventArgs)
        {
            if (_initialCursorTop == -1)
            {
                _initialCursorTop = _console.CursorTop;
            }

            var availableWidth = _console.WindowWidth - 1; // Writing to end causes implicit line wrap

            var barWidth = (int)(availableWidth * Width);
            var textOffset = StatusOnSeparateLine ? 0 : barWidth;
            var textWidth = availableWidth - textOffset;

            var statusText = CreateStatusText(eventArgs.Messages, textWidth);

            var barInnerWidth = Math.Max(0, barWidth - 12); // "[] 100.00 % "
            var barFillWidth = (int)(barInnerWidth * eventArgs.Progress);
            var barString = new string('#', barFillWidth).PadRight(barInnerWidth);

            // ReSharper disable once UseStringInterpolation
            var line1 = string.Format(
                "[{0}] {1,8:P} {2}",
                barString,
                eventArgs.Progress,
                StatusOnSeparateLine ? string.Empty : statusText);

            line1 = line1.PadRightSurrogateAware(availableWidth);

            _console.Write(line1);

            if (StatusOnSeparateLine)
            {
                statusText = statusText.PadRightSurrogateAware(availableWidth);
                _console.WriteLine();
                _console.Write(statusText);
            }
            
            _console.SetCursorPosition(0, _initialCursorTop);
        }

        /// <summary>
        /// Format status text from list of messages.
        /// </summary>
        /// <param name="messages">List of messages, ordered from least specific to most specific.</param>
        /// <param name="maxWidth">Maximum text width in characters.</param>
        /// <returns>Status text, made to fit into <paramref name="maxWidth"/>.</returns>
        protected string CreateStatusText(IReadOnlyList<string> messages, int maxWidth)
        {
            var statusText = string.Join(" – ", messages);
            statusText = statusText.LimitLength(maxWidth);
            statusText = statusText.PadRightSurrogateAware(maxWidth);

            return statusText;
        }

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
                Render(eventArgs);
            }
            finally
            {
                Monitor.Exit(_syncRoot);
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
}
