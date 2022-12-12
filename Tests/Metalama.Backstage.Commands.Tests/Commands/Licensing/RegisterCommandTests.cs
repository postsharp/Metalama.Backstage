// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Licensing
{
    public class RegisterCommandTests : LicensingCommandsTestsBase
    {
        public RegisterCommandTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public async Task CleanEnvironmentListsNoLicenses()
        {
            await this.TestCommandAsync( "license list", "" );
        }

        [Fact]
        public async Task OneLicenseKeyListedAfterOneRegistration()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.MetalamaStarterBusinessKey}", "" );

            await this.TestCommandAsync( "license list", TestLicenses.MetalamaStarterBusinessOutput );
        }

        [Fact]
        public async Task OneLicenseKeyListedAfterMultipleLicenseKeysRegistered()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.MetalamaStarterBusinessKey}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.MetalamaProfessionalPersonalKey}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.MetalamaUltimateOpenSourceRedistributionKey}", "" );

            await this.TestCommandAsync( "license list", TestLicenses.MetalamaUltimateOpenSourceRedistributionOutput );
        }
    }
}