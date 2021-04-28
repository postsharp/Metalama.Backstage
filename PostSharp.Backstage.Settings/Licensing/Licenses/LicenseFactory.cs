// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics.CodeAnalysis;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Licenses
{
    public class LicenseFactory
    {
        private readonly IServiceProvider _services;
        private readonly IDiagnosticsSink _diagnostics;
        private readonly ITrace _trace;

        public LicenseFactory( IServiceProvider services, ITrace trace )
        {
            this._services = services;
            this._diagnostics = services.GetService<IDiagnosticsSink>();
            this._trace = trace;
        }

        public bool TryCreate( string? licenseString, [MaybeNullWhen( returnValue: false )] out ILicense license )
        {
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
                license = new License( licenseString, this._services, this._trace );
                return true;
            }
        }
    }
}
