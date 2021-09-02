// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace PostSharp.Backstage.Licensing.Consumption.Sources
{
    /// <summary>
    /// License source providing licenses from a license file.
    /// </summary>
    public class FileLicenseSource : ILicenseSource
    {
        private readonly string _path;
        private readonly IServiceProvider _services;
        private readonly ILogger? _logger;

        public FileLicenseSource( string path, IServiceProvider services )
        {
            this._path = path;
            this._services = services;
            this._logger = services.GetOptionalTraceLogger<FileLicenseSource>();
        }

        /// <inheritdoc />
        public IEnumerable<ILicense> GetLicenses()
        {
            this._logger?.LogTrace( $"Loading licenses from '{this._path}'." );

            var diagnosticsSink = this._services.GetRequiredService<IDiagnosticsSink>();
            var fileSystem = this._services.GetRequiredService<IFileSystem>();

            string[] licenseStrings;

            try
            {
                licenseStrings = fileSystem.ReadAllLines( this._path );
            }
            catch ( Exception e )
            {
                const string messageFormat = "Failed to load licenses from '{0}': {1}";
                this._logger?.LogTrace( string.Format( CultureInfo.InvariantCulture, messageFormat, this._path, e ) );
                diagnosticsSink.ReportWarning( string.Format( CultureInfo.InvariantCulture, messageFormat, this._path, e.Message ) );

                yield break;
            }

            var licenseFactory = new LicenseFactory( this._services );

            foreach ( var licenseString in licenseStrings )
            {
                if ( string.IsNullOrWhiteSpace( licenseString ) )
                {
                    continue;
                }

                if ( licenseFactory.TryCreate( licenseString, out var license ) )
                {
                    this._logger?.LogTrace( $"{license} loaded from '{this._path}'." );

                    yield return license;
                }
            }
        }
    }
}