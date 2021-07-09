// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands.Licensing
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
            await this.TestCommandAsync( $"license register {TestLicenses.Key1}", "" );
            await this.TestCommandAsync( "license list", string.Format( TestLicenses.Format1, 1 ) );
        }

        [Fact]
        public async Task MultipleLicenseKeysListedAfterRegistration()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.Key1}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.Key2}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.Key3}", "" );

            await this.TestCommandAsync(
                "license list",
                string.Format( TestLicenses.Format1, 1 ) + string.Format( TestLicenses.Format2, 2 ) + string.Format( TestLicenses.Format3, 3 ) );
        }
    }
}