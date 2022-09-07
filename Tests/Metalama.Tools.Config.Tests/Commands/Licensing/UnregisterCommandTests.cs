// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Licensing
{
    public class UnregisterCommandTests : LicensingCommandsTestsBase
    {
        public UnregisterCommandTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public async Task LicenseKeyFailsToUnregisterInCleanEnvironment()
        {
            await this.TestCommandAsync(
                $"license unregister",
                "",
                $"A license is not registered." + Environment.NewLine,
                2 );
        }

        [Fact]
        public async Task RegisteredLicenseUnregisters()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.MetalamaStarterBusinessKey}", "" );

            await this.TestCommandAsync( "license list", TestLicenses.MetalamaStarterBusinessOutput );

            await this.TestCommandAsync( "license unregister", "The license has been unregistered." + Environment.NewLine );

            await this.TestCommandAsync( "license list", "" );
        }
    }
}