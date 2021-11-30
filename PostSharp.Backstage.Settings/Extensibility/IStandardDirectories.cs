namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Provides paths of standard directories.
    /// </summary>
    public interface IStandardDirectories
    {
        /// <summary>
        /// Gets the directory that serves as a common repository for application-specific data for the current roaming user.
        /// </summary>
        string ApplicationDataDirectory { get; }

        /// <summary>
        /// Gets the path of the current user's application temporary folder.
        /// </summary>
        string TempDirectory { get; }
    }
}