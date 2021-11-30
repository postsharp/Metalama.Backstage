namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Provides standard paths of license files.
    /// </summary>
    public interface IStandardLicenseFileLocations
    {
        /// <summary>
        /// Gets the path to the license file containing user-wise registered licenses.
        /// </summary>
        string UserLicenseFile { get; }
    }
}