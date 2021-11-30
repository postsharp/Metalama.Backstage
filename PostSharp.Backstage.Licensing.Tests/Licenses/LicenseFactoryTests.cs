// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Testing.Services;
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
            Assert.False( LicenseFactory.TryCreate( "", out _ ) );
            Diagnostics.AssertNoErrors();
            Diagnostics.AssertSingleWarning( "Empty license string provided." );
        }

        [Fact]
        public void EmptyLicenseStringFails()
        {
            Assert.False( LicenseFactory.TryCreate( string.Empty, out _ ) );
            Diagnostics.AssertNoErrors();
            Diagnostics.AssertSingleWarning( "Empty license string provided." );
        }

        [Fact]
        public void WhitespaceLicenseStringFails()
        {
            Assert.False( LicenseFactory.TryCreate( " ", out _ ) );
            Diagnostics.AssertNoErrors();
            Diagnostics.AssertSingleWarning( "Empty license string provided." );
        }

        [Fact]
        public void InvalidLicenseStringCreatesInvalidLicense()
        {
            const string invalidLicenseString = "SomeInvalidLicenseString";
            Assert.True( LicenseFactory.TryCreate( invalidLicenseString, out var license ) );
            Assert.True( license is License );
            Assert.False( license!.TryGetLicenseConsumptionData( out _ ) );
            Diagnostics.AssertNoErrors();

            Diagnostics.AssertSingleWarning(
                $"Cannot parse license key {invalidLicenseString.ToUpperInvariant()}: License header not found for license {{{invalidLicenseString.ToUpperInvariant()}}}." );
        }

        [Fact]
        public void ValidLicenseKeyCreatesValidLicense()
        {
            Assert.True( LicenseFactory.TryCreate( TestLicenseKeys.Ultimate, out var license ) );
            Assert.True( license is License );
            Assert.True( license!.TryGetLicenseConsumptionData( out var licenseData ) );
            Assert.NotNull( licenseData );
            Diagnostics.AssertClean();
        }

        [Fact]
        public void UrlCreatesLicenseLease()
        {
            // TODO

            // Assert.True( this._licenseFactory.TryCreate( "http://hello.world", out var license ) );
            // Assert.True( license is LicenseLease );

            Assert.False( LicenseFactory.TryCreate( "http://hello.world", out _ ) );
            Diagnostics.AssertNoErrors();
            Diagnostics.AssertSingleWarning( "License server is not yet supported." );
        }
    }
}