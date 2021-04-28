// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using PostSharp.Backstage.Licensing.Licenses;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Licenses
{
    public class LicenseFactoryTests : LicensingTestsBase
    {
        private readonly LicenseFactory _licenseFactory;

        public LicenseFactoryTests( ITestOutputHelper logger ) : base( logger )
        {
            this._licenseFactory = new( this.Services, this.Trace );
        }

        [Fact]
        public void NullLicenseStringFails()
        {
            Assert.False( this._licenseFactory.TryCreate( "", out _ ) );
            this.Services.Diagnostics.AssertNoErrors();
            Assert.Equal( "Empty license string provided.", this.Services.Diagnostics.Warnings.Single() );
        }

        [Fact]
        public void EmptyLicenseStringFails()
        {
            Assert.False( this._licenseFactory.TryCreate( string.Empty, out _ ) );
            this.Services.Diagnostics.AssertNoErrors();
            Assert.Equal( "Empty license string provided.", this.Services.Diagnostics.Warnings.Single() );
        }

        [Fact]
        public void WhitespaceLicenseStringFails()
        {
            Assert.False( this._licenseFactory.TryCreate( " ", out _ ) );
            this.Services.Diagnostics.AssertNoErrors();
            Assert.Equal( "Empty license string provided.", this.Services.Diagnostics.Warnings.Single() );
        }

        [Fact]
        public void InvalidLicenseStringCreatesInvalidLicense()
        {
            Assert.True( this._licenseFactory.TryCreate( "SomeInvalidLicenseString", out var license ) );
            Assert.True( license is License );
            Assert.False( license!.TryGetLicenseData( out _ ) );
            this.Services.Diagnostics.AssertNoErrors();
            Assert.Equal(
                "Cannot parse license key SOMEINVALIDLICENSESTRING: License header not found for license {SOMEINVALIDLICENSESTRING}.",
                this.Services.Diagnostics.Warnings.Single() );
        }

        [Fact]
        public void ValidLicenseKeyCreatesValidLicense()
        {
            Assert.True( this._licenseFactory.TryCreate( TestLicenseKeys.Ultimate, out var license ) );
            Assert.True( license is License );
            Assert.True( license!.TryGetLicenseData( out var licenseData ) );
            Assert.NotNull( licenseData );
            this.Services.Diagnostics.AssertClean();
        }

        [Fact]
        public void UrlCreatesLicenseLease()
        {
            // TODO

            // Assert.True( this._licenseFactory.TryCreate( "http://hello.world", out var license ) );
            // Assert.True( license is LicenseLease );

            Assert.False( this._licenseFactory.TryCreate( "http://hello.world", out _ ) );
            this.Services.Diagnostics.AssertNoErrors();
            Assert.Equal( "License server is not yet supported.", this.Services.Diagnostics.Warnings.Single() );
        }
    }
}
