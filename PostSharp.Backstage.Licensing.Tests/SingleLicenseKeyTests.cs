// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Tests.Services;
using Xunit;

namespace PostSharp.Backstage.Licensing.Tests
{
    public class SingleLicenseKeyTests
    {
        private static LicenseManager CreateLicenseManager( string licenseKey )
        {
            var services = new TestServices();
            var trace = new NullTrace();
            var licenseSource = new MemoryLicenseSource( "test", licenseKey );
            return new( services, trace, licenseSource );
        }

        [Fact]
        public void TestUltimate()
        {
            var licenseManager = CreateLicenseManager( TestLicenseKeys.Ultimate );
            Assert.True( licenseManager.IsRequirementSatisfied( LicensedPackages.Caravela ) );
        }

        [Fact]
        public void TestDiagnostics()
        {
            var licenseManager = CreateLicenseManager( TestLicenseKeys.Diagnostics );
            Assert.False( licenseManager.IsRequirementSatisfied( LicensedPackages.Caravela ) );
        }
    }
}
