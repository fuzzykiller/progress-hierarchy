namespace ProgressHierarchy
{
    /// <summary>
    /// Represents the method that will handle the <see cref="HierarchicalProgress.ProgressChanged"/> event raised when the progress changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">A <see cref="HierarchicalProgressChangedEventArgs"/> that contains the event data.</param>
    public delegate void HierarchicalProgressChangedEventHandler(object sender, HierarchicalProgressChangedEventArgs eventArgs);
}