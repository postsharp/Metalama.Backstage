// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

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
        public async Task OneLicenseKeyListedAfterRegistration()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.MetalamaStarterBusinessKey}", "" );

            await this.TestCommandAsync( "license list", TestLicenses.MetalamaStarterBusinessOutput );
        }

        [Fact]
        public async Task MultipleLicenseKeysListedAfterRegistration()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.MetalamaStarterBusinessKey}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.MetalamaProfessionalPersonalKey}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.MetalamaUltimateOpenSourceRedistributionKey}", "" );

            await this.TestCommandAsync( "license list", TestLicenses.MetalamaUltimateOpenSourceRedistributionOutput );
        }
    }
}