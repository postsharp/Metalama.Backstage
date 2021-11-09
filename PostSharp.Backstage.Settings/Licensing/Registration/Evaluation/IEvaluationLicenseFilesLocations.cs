// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Registration.Evaluation
{
    /// <summary>
    /// Provides locations of evaluation-license-related files used internally to handle product evaluation eligibility.
    /// </summary>
    internal interface IEvaluationLicenseFilesLocations : IService
    {
        /// <summary>
        /// Gets the path of the file storing information about product evaluation eligibility.
        /// </summary>
        string EvaluationLicenseFile { get; }
    }
}