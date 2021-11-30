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
            _path = path;
            _services = services;
            _logger = services.GetOptionalTraceLogger<FileLicenseSource>();
        }

        protected override IEnumerable<string> GetLicenseStrings()
        {
            _logger?.LogTrace( $"Loading licenses from '{_path}'." );

            var diagnosticsSink = _services.GetRequiredService<IDiagnosticsSink>();
            var fileSystem = _services.GetRequiredService<IFileSystem>();

            string[] licenseStrings;

            try
            {
                licenseStrings = fileSystem.ReadAllLines( _path );
            }
            catch (Exception e)
            {
                const string messageFormat = "Failed to load licenses from '{0}': {1}";
                _logger?.LogTrace( string.Format( CultureInfo.InvariantCulture, messageFormat, _path, e ) );
                diagnosticsSink.ReportWarning( string.Format( CultureInfo.InvariantCulture, messageFormat, _path, e.Message ) );

                yield break;
            }

            foreach (var licenseString in licenseStrings)
            {
                yield return licenseString;
            }
        }
    }
}