// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Consumption.Sources;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.LicenseSources
{
    public class LicenseStringsLicenseSourceTests : LicensingTestsBase
    {
        public LicenseStringsLicenseSourceTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void NoLicenseStringPasses()
        {
            ExplicitLicenseSource source = new( Enumerable.Empty<string>(), this.ServiceProvider );

            Assert.Empty( source.GetLicenses( _ => { } ) );
        }

        [Fact]
        public void OneLicenseStringPasses()
        {
            ExplicitLicenseSource source = new( new[] { TestLicenseKeys.Ultimate }, this.ServiceProvider );

            Assert.Equal( TestLicenseKeys.Ultimate, source.GetLicenses( _ => { } ).Single().ToString() );
        }

        [Fact]
        public void EmptyLicenseStringsSkipped()
        {
            ExplicitLicenseSource source =
                new( new[] { "", TestLicenseKeys.Ultimate, "", "", TestLicenseKeys.Logging, "" }, this.ServiceProvider );

            var licenses = source.GetLicenses( _ => { } ).ToArray();
            Assert.Equal( 2, licenses.Length );
            Assert.Equal( TestLicenseKeys.Ultimate, licenses[0].ToString() );
            Assert.Equal( TestLicenseKeys.Logging, licenses[1].ToString() );
        }
    }
}