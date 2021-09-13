﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Backstage.Licensing.Licenses
{
    /// <summary>
    /// Creates an <see cref="ILicense" /> object from a license string.
    /// </summary>
    public class LicenseFactory
    {
        private readonly IServiceProvider _services;
        private readonly IDiagnosticsSink _diagnostics;
        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseFactory"/> class.
        /// </summary>
        /// <param name="services">Services.</param>
        public LicenseFactory( IServiceProvider services )
        {
            this._services = services;
            this._diagnostics = services.GetRequiredService<IDiagnosticsSink>();
            this._logger = services.GetOptionalTraceLogger<LicenseFactory>();
        }

        /// <summary>
        /// Attempts to create an <see cref="ILicense" /> object from a license string.
        /// </summary>
        /// <param name="licenseString">The license string. E.g. license key or license server URL.</param>
        /// <param name="license">The <see cref="ILicense" /> object represented by the <paramref name="licenseString"/>.</param>
        /// <returns>A value indicating if the <paramref name="licenseString"/> represents a license.</returns>
        public bool TryCreate( string? licenseString, [MaybeNullWhen( returnValue: false )] out ILicense license )
        {
            // TODO: trace
            this._logger.LogInformation( "TODO: trace" );

            licenseString = licenseString?.Trim();

            if ( licenseString == null || licenseString == "" )
            {
                this._diagnostics.ReportWarning( "Empty license string provided." );
                license = null;

                return false;
            }

            if ( Uri.IsWellFormedUriString( licenseString, UriKind.Absolute ) )
            {
                // TODO License Server Support
                this._diagnostics.ReportWarning( "License server is not yet supported." );
                license = null;

                return false;
            }
            else
            {
                license = new License( licenseString, this._services );

                return true;
            }
        }
    }
}