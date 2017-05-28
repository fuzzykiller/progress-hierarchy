using System;
using System.Collections.Generic;

namespace ConsoleProgressBar
{
    /// <summary>
    /// Provides data for the <see cref="ConsoleProgressBar.Progress.ProgressChanged"/> event.
    /// </summary>
    /// <remarks>Uses fields for performance reasons</remarks>
    public class ProgressChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressChangedEventArgs"/> class.
        /// </summary>
        /// <param name="progress">Current progress percentage.</param>
        /// <param name="messages">Collection of messages associated with current progress, ordered by specifity.</param>
        public ProgressChangedEventArgs(double progress, IReadOnlyList<string> messages)
        {
            Progress = progress;
            Messages = messages;
        }

        /// <summary>
        /// Current progress percentage.
        /// </summary>
        public readonly double Progress;

        /// <summary>
        /// Messages associated with current progress, ordered by specifity.
        /// </summary>
        public readonly IReadOnlyList<string> Messages;
    }
}