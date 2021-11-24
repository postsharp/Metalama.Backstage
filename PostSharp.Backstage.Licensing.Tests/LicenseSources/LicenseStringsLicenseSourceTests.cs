// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Testing.Services;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.LicenseSources
{
    public class LicenseStringsLicenseSourceTests : LicensingTestsBase
    {
        public LicenseStringsLicenseSourceTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void NoLicenseStringPasses()
        {
            LicenseStringsLicenseSource source = new( Enumerable.Empty<string>(), this.Services );

            Assert.Empty( source.GetLicenses() );
            this.Diagnostics.AssertClean();
        }

        [Fact]
        public void OneLicenseStringPasses()
        {
            LicenseStringsLicenseSource source = new( new[] { TestLicenseKeys.Ultimate }, this.Services );

            Assert.Equal( $"License '{TestLicenseKeys.Ultimate}'", source.GetLicenses().Single().ToString() );
            this.Diagnostics.AssertClean();
        }

        [Fact]
        public void EmptyLicenseStringsSkipped()
        {
            LicenseStringsLicenseSource source = new( new[] { "", TestLicenseKeys.Ultimate, "", "", TestLicenseKeys.Logging, "" }, this.Services );

            var licenses = source.GetLicenses().ToArray();
            Assert.Equal( 2, licenses.Length );
            Assert.Equal( $"License '{TestLicenseKeys.Ultimate}'", licenses[0].ToString() );
            Assert.Equal( $"License '{TestLicenseKeys.Logging}'", licenses[1].ToString() );

            this.Diagnostics.AssertClean();
        }
    }
}