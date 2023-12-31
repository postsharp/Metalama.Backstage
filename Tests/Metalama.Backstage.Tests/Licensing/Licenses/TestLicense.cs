﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using System;
using System.Collections.Generic;
using Xunit;

namespace Metalama.Backstage.Tests.Licensing.Licenses
{
    internal class TestLicense : ILicense, IUsable
    {
        public ILicense License { get; }

        public bool IsUsed { get; private set; }

        public TestLicense( ILicense license )
        {
            this.License = license;
        }

        // MaybeNullWhenAttribute cannot be used here since the Metalama.Backstage assembly shares internals with this assembly.
        // That causes CS0433 error. (Same type defined in two referenced assemblies.)
        public bool TryGetLicenseConsumptionData( /* [MaybeNullWhenAttribute( false )] */
            out LicenseConsumptionData licenseData,
            out string errorMessage )
        {
            // Each license should always be used only once.
            Assert.False( this.IsUsed );

            this.IsUsed = true;

            return this.License.TryGetLicenseConsumptionData( out licenseData!, out errorMessage! );
        }

        // MaybeNullWhenAttribute cannot be used here since the Metalama.Backstage assembly shares internals with this assembly.
        // That causes CS0433 error. (Same type defined in two referenced assemblies.)
        public bool TryGetLicenseRegistrationData( /* [MaybeNullWhenAttribute( false )] */
            out LicenseRegistrationData licenseRegistrationData,
            out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public void ResetUsage()
        {
            this.IsUsed = false;
        }

        public override bool Equals( object? obj )
        {
            return obj is TestLicense license &&
                   EqualityComparer<ILicense>.Default.Equals( this.License, license.License );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( this.License );
        }
    }
}