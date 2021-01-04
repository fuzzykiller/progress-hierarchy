using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ProgressHierarchy
{
    /// <summary>
    /// Provides thread-safe hierarchical progress reporting. This class cannot be inherited.
    /// </summary>
    public sealed class Progress : IDisposable, IProgress<double>
    {
        private delegate void ParentProgressChangedHandler(double progress, IReadOnlyList<string> messages);

        /// <remarks>Values *must* come from <see cref="Status"/></remarks>
        private int _status;

        /// <remarks>Used to accumulate progress from forks</remarks>
        private double _progress;

        /// <remarks>Used when forking: Stores fork message, if any</remarks>
        private string[] _forkMessage;

        /// <remarks>Used by parent progress, if any</remarks>
        private double _lastProgress;

        private bool _disposed;

        /// <remarks>Used by sub progress to efficiently report to parent progress</remarks>
        private ParentProgressChangedHandler _parentProgressChangedHandler;

        private event ProgressChangedEventHandler InternalProgressChanged;

        /// <summary>
        /// Occurs when progress changes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The <see cref="Progress"/> instance was already disposed.</exception>
        public event ProgressChangedEventHandler ProgressChanged
        {
            add
            {
                if (_disposed) throw new ObjectDisposedException(nameof(Progress));
                InternalProgressChanged += value;
            }
            remove
            {
                if (_disposed) throw new ObjectDisposedException(nameof(Progress));
                InternalProgressChanged -= value;
            }
        }

        /// <summary>
        /// Report progress.
        /// </summary>
        /// <param name="progress">Progress percentage.</param>
        /// <exception cref="ObjectDisposedException">The <see cref="Progress"/> instance was already disposed.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Fork"/> method was called on this <see cref="Progress"/> instance.</exception>
        public void Report(double progress)
        {
            Report(progress, null);
        }

        /// <summary>
        /// Report progress with optional message.
        /// </summary>
        /// <param name="progress">Progress percentage.</param>
        /// <param name="message">Optional message that will be reported with the current progress.</param>
        /// <exception cref="ObjectDisposedException">The <see cref="Progress"/> instance was already disposed.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Fork"/> method was called on this <see cref="Progress"/> instance.</exception>
        public void Report(double progress, string message)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Progress));

            var status = (Status)Interlocked.CompareExchange(
                ref _status,
                (int)Status.ProgressReported,
                (int)Status.None);
            if (status == Status.Forked)
                throw new InvalidOperationException("Progress has been forked. It can no longer be set explicitly.");

            var messageArray = message != null ? new[] { message } : new string[0];
            OnProgressChanged(progress, messageArray);
        }

        /// <summary>
        /// Fork progress, optionally partly and with message.
        /// </summary>
        /// <param name="scale">Scale of the forked sub-progress. Percentage ≥0.</param>
        /// <param name="message">Optional message that describes the sub-progress.</param>
        /// <returns><see cref="Progress"/> that is linked with and reports via this instance.</returns>
        public Progress Fork(double scale = 1D, string message = null)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Progress));
            if (scale < 0) throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale is smaller than 0");

            var status = (Status)Interlocked.CompareExchange(ref _status, (int)Status.Forked, (int)Status.None);
            if (status == Status.ProgressReported)
                throw new InvalidOperationException(
                    "Progress has been explicitly reported. It can no longer be forked.");

            _forkMessage = message != null ? new[] { message } : null;

            var subProgress = new Progress();
            subProgress._parentProgressChangedHandler =
                (progress, messages) => UpdateProgressOnSubProgressChanged(
                    progress,
                    messages,
                    ref subProgress._lastProgress,
                    scale);

            return subProgress;
        }

        /// <summary>
        /// Finish the current <see cref="Progress"/>, setting it to 100%. Disconnects all event handlers.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            OnProgressChanged(1, new string[0]);
            InternalProgressChanged = null;
            _parentProgressChangedHandler = null;
        }

        private void UpdateProgressOnSubProgressChanged(
            double subProgressNewProgress,
            IReadOnlyList<string> subProgressMessages,
            ref double subProgressLastProgress,
            double scale)
        {
            var previousProgress = Interlocked.Exchange(ref subProgressLastProgress, subProgressNewProgress);
            var progressDelta = (subProgressNewProgress - previousProgress) * scale;

            var currentProgress = _progress;
            bool valueReplaced;
            double newProgress;

            // Even if operations are out of order, the sum will remain the same
            do
            {
                newProgress = currentProgress + progressDelta;
                var comparand = currentProgress;
                currentProgress = Interlocked.CompareExchange(ref _progress, newProgress, comparand);

                // Implication: If comparand and returned original value are equal, the value has been exchanged
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                valueReplaced = currentProgress == comparand;
            } while (!valueReplaced);

            var messages = _forkMessage?.Concat(subProgressMessages).ToArray() ?? subProgressMessages;

            OnProgressChanged(newProgress, messages);
        }

        private void OnProgressChanged(double progress, IReadOnlyList<string> messages)
        {
            _parentProgressChangedHandler?.Invoke(progress, messages);

            var handler = InternalProgressChanged;
            if (handler != null)
            {
                var eventArgs = new ProgressChangedEventArgs(progress, messages);
                handler.Invoke(this, eventArgs);
            }
        }

        private enum Status
        {
            None,
            ProgressReported,
            Forked
        }
    }
}