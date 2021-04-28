// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Licenses;
using Xunit;

namespace PostSharp.Backstage.Licensing.Tests.Licenses
{
    internal class TestLicense : ILicense, IUsable
    {
        public ILicense License { get; }

        public bool Used { get; private set; }

        public TestLicense( ILicense license )
        {
            this.License = license;
        }

        // MaybeNullWhenAttribute cannot be used here since the PostSharp.Backstage.Settings assembly shares internals with this assembly.
        // That causes CS0433 error. (Same type defined in two referenced assemblies.)
        public bool TryGetLicenseData( /* [MaybeNullWhenAttribute( false )] */ out LicenseData licenseData )
        {
            // Each license should always be used only once.
            Assert.False( this.Used );

            this.Used = true;
            return this.License.TryGetLicenseData( out licenseData! );
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
