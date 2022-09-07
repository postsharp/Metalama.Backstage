// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption.Sources;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.LicenseSources
{
    public class LicenseStringsLicenseSourceTests : LicensingTestsBase
    {
        public LicenseStringsLicenseSourceTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void OneLicenseStringPasses()
        {
            ExplicitLicenseSource source = new( TestLicenses.MetalamaUltimateBusiness, this.ServiceProvider );

            var license = source.GetLicense( _ => { } );
            Assert.NotNull( license );

            var dataParsed = license!.TryGetLicenseConsumptionData( out var data );
            Assert.True( dataParsed );
            Assert.Equal( TestLicenses.MetalamaUltimateBusiness, data!.LicenseString );
        }
    }
}