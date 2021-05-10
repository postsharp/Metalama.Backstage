// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Backstage.Licensing.Sources
{
    public class FileLicenseSource : ILicenseSource
    {
        private readonly string _path;
        private readonly IServiceProvider _services;
        private readonly ITrace _trace;

        public FileLicenseSource( string path, IServiceProvider services, ITrace trace )
        {
            this._path = path;
            this._services = services;
            this._trace = trace;
        }

        public IEnumerable<ILicense> GetLicenses()
        {
            this._trace.WriteLine( "Loading licenses from '{0}'.", this._path );

            var diagnosticsSink = this._services.GetService<IDiagnosticsSink>();
            var fileSystem = this._services.GetService<IFileSystem>();

            string[] licenseStrings;

            try
            {
                licenseStrings = fileSystem.ReadAllLines( this._path );
            }
            catch ( Exception e )
            {
                const string messageFormat = "Failed to load licenses from '{0}': {1}";
                this._trace.WriteLine( messageFormat, this._path, e );
                diagnosticsSink.ReportWarning( messageFormat, this._path, e.Message );
                yield break;
            }

            var licenseFactory = new LicenseFactory( this._services, this._trace );

            foreach ( var licenseString in licenseStrings )
            {
                if ( string.IsNullOrWhiteSpace( licenseString ) )
                {
                    continue;
                }

                if ( licenseFactory.TryCreate( licenseString, out var license ) )
                {
                    this._trace.WriteLine( "{0} loaded from '{1}'.", license, this._path );
                    yield return license;
                }
            }
        }
    }
}
