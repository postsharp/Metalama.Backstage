// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics.CodeAnalysis;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing
{
    public interface ILicense
    {
        bool TryGetLicenseData( IDiagnosticsSink diagnosticsSink, [MaybeNullWhen( returnValue: false )] out LicenseData licenseData );
    }
}
