namespace PostSharp.Backstage.Licensing.Registration.Evaluation
{
    /// <summary>
    /// Provides locations of evaluation-license-related files used internally to handle product evaluation eligibility.
    /// </summary>
    internal interface IEvaluationLicenseFilesLocations
    {
        /// <summary>
        /// Gets the path of the file storing information about product evaluation eligibility.
        /// </summary>
        string EvaluationLicenseFile { get; }
    }
}