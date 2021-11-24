// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Extensibility.Extensions;
using PostSharp.Backstage.Licensing.Registration;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace PostSharp.Backstage.Licensing.Consumption.Sources
{
    /// <summary>
    /// License source providing licenses from a license file.
    /// </summary>
    public class FileLicenseSource : LicenseStringsLicenseSourceBase
    {
        private readonly string _path;
        private readonly IServiceProvider _services;
        private readonly ILogger? _logger;

        public static FileLicenseSource CreateUserLicenseFileLicenseSource( IServiceProvider services )
        {
            var standardLicenseFileLocations = services.GetRequiredService<IStandardLicenseFileLocations>();

            return new FileLicenseSource( standardLicenseFileLocations.UserLicenseFile, services );
        }

        public FileLicenseSource( string path, IServiceProvider services )
            : base( services )
        {
            this._path = path;
            this._services = services;
            this._logger = services.GetOptionalTraceLogger<FileLicenseSource>();
        }

        protected override IEnumerable<string> GetLicenseStrings()
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

            foreach ( var licenseString in licenseStrings )
            {
                yield return licenseString;
            }
        }
    }
}