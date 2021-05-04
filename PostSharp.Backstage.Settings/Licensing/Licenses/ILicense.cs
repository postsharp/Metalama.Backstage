// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics.CodeAnalysis;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Licenses
{
    public interface ILicense
    {
        bool TryGetLicenseConsumptionData( [MaybeNullWhen( returnValue: false )] out LicenseConsumptionData licenseConsumptionData );

        bool TryGetLicenseRegistrationData( [MaybeNullWhen( returnValue: false )] out LicenseRegistrationData licenseRegistrationData );
    }
}
