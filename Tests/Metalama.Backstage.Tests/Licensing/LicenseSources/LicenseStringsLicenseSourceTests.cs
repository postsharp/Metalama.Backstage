// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

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
            ExplicitLicenseSource source = new( Enumerable.Empty<string>(), true, this.ServiceProvider );

            Assert.Empty( source.GetLicenses( _ => { } ) );
        }

        [Fact]
        public void OneLicenseStringPasses()
        {
            ExplicitLicenseSource source = new( new[] { TestLicenseKeys.PostSharpUltimate }, true, this.ServiceProvider );

            Assert.Equal( TestLicenseKeys.PostSharpUltimate, source.GetLicenses( _ => { } ).Single().ToString() );
        }

        [Fact]
        public void EmptyLicenseStringsSkipped()
        {
            ExplicitLicenseSource source =
                new( new[] { "", TestLicenseKeys.PostSharpUltimate, "", "", TestLicenseKeys.Logging, "" }, true, this.ServiceProvider );

            var licenses = source.GetLicenses( _ => { } ).ToArray();
            Assert.Equal( 2, licenses.Length );
            Assert.Equal( TestLicenseKeys.PostSharpUltimate, licenses[0].ToString() );
            Assert.Equal( TestLicenseKeys.Logging, licenses[1].ToString() );
        }
    }
}