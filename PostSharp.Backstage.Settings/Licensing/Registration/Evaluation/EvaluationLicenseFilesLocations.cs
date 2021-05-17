// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Registration.Evaluation
{
    /// <inheritdoc />
    internal class EvaluationLicenseFilesLocations : IEvaluationLicenseFilesLocations
    {
        /// <inheritdoc />
        public string EvaluationLicenseFile { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationLicenseFilesLocations"/> class.
        /// </summary>
        /// <param name="standardDirectories">An object providing paths of standard directories.</param>
        public EvaluationLicenseFilesLocations( IServiceProvider services )
        {
            var standardDirectories = services.GetRequiredService<IStandardDirectories>();
            this.EvaluationLicenseFile = Path.Combine( standardDirectories.ApplicationDataDirectory, "licenseregistration.cnf" );
        }
    }
}
