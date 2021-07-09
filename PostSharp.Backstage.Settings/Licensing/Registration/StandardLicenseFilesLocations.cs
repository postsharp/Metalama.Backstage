// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using System;
using System.IO;

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <inheritdoc />    
    internal class StandardLicenseFilesLocations : IStandardLicenseFileLocations
    {
        /// <inheritdoc />
        public string UserLicenseFile { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardLicenseFilesLocations"/> class.
        /// </summary>
        /// <param name="standardDirectories">An object providing paths of standard directories.</param>
        public StandardLicenseFilesLocations( IServiceProvider services )
        {
            var standardDirectories = services.GetRequiredService<IStandardDirectories>();
            this.UserLicenseFile = Path.Combine( standardDirectories.ApplicationDataDirectory, "postsharp.lic" );
        }
    }
}