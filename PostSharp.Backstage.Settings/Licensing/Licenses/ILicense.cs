// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics.CodeAnalysis;
using PostSharp.Backstage.Licensing.Consumption;

namespace PostSharp.Backstage.Licensing.Licenses
{
    public interface ILicense
    {
        bool TryGetLicenseData( [MaybeNullWhen( returnValue: false )] out LicenseData licenseData );
    }
}
