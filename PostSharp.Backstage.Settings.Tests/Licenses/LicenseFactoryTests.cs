// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Licenses
{
    public class LicenseFactoryTests : LicensingTestsBase
    {
        public LicenseFactoryTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void NullLicenseStringFails()
        {
            Assert.False( this.LicenseFactory.TryCreate( "", out _ ) );
        }

        [Fact]
        public void EmptyLicenseStringFails()
        {
            Assert.False( this.LicenseFactory.TryCreate( string.Empty, out _ ) );
        }

        [Fact]
        public void WhitespaceLicenseStringFails()
        {
            Assert.False( this.LicenseFactory.TryCreate( " ", out _ ) );
        }

        [Fact]
        public void InvalidLicenseStringCreatesInvalidLicense()
        {
            const string invalidLicenseString = "SomeInvalidLicenseString";
            Assert.True( this.LicenseFactory.TryCreate( invalidLicenseString, out var license ) );
            Assert.True( license is License );
            Assert.False( license!.TryGetLicenseConsumptionData( out _ ) );
        }

        [Fact]
        public void ValidLicenseKeyCreatesValidLicense()
        {
            Assert.True( this.LicenseFactory.TryCreate( TestLicenseKeys.Ultimate, out var license ) );
            Assert.True( license is License );
            Assert.True( license!.TryGetLicenseConsumptionData( out var licenseData ) );
            Assert.NotNull( licenseData );
        }

        [Fact]
        public void UrlCreatesLicenseLease()
        {
            // TODO

            // Assert.True( this._licenseFactory.TryCreate( "http://hello.world", out var license ) );
            // Assert.True( license is LicenseLease );

            Assert.False( this.LicenseFactory.TryCreate( "http://hello.world", out _ ) );
        }
    }
}