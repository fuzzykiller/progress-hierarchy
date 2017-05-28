namespace ConsoleProgressBar
{
    /// <summary>
    /// Represents the method that will handle the <see cref="Progress.ProgressChanged"/> event raised when the progress changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">A <see cref="ProgressChangedEventArgs"/> that contains the event data.</param>
    public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs eventArgs);
}