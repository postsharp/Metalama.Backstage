// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
using System;
using System.Collections.Generic;
using Xunit;

namespace PostSharp.Backstage.Licensing.Tests.Licenses
{
    internal class TestLicense : ILicense, IUsable
    {
        public ILicense License { get; }

        public bool IsUsed { get; private set; }

        public TestLicense( ILicense license )
        {
            this.License = license;
        }

        // MaybeNullWhenAttribute cannot be used here since the PostSharp.Backstage.Settings assembly shares internals with this assembly.
        // That causes CS0433 error. (Same type defined in two referenced assemblies.)
        public bool TryGetLicenseConsumptionData( /* [MaybeNullWhenAttribute( false )] */
            out LicenseConsumptionData licenseData )
        {
            // Each license should always be used only once.
            Assert.False( this.IsUsed );

            this.IsUsed = true;

            return this.License.TryGetLicenseConsumptionData( out licenseData! );
        }

        // MaybeNullWhenAttribute cannot be used here since the PostSharp.Backstage.Settings assembly shares internals with this assembly.
        // That causes CS0433 error. (Same type defined in two referenced assemblies.)
        public bool TryGetLicenseRegistrationData( /* [MaybeNullWhenAttribute( false )] */
            out LicenseRegistrationData licenseRegistrationData )
        {
            throw new NotImplementedException();
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