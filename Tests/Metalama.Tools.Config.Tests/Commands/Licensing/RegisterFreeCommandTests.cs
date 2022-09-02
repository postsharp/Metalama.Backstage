// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Licensing
{
    public class RegisterFreeCommandTests : LicensingCommandsTestsBase
    {
        public RegisterFreeCommandTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public async Task FreeRegistersInEmptyEnvironment()
        {
            await this.TestCommandAsync( "license register free", "" );

            await this.TestCommandAsync( "license list", TestLicenses.FreeOutput );
        }

        [Fact]
        public async Task RepetitiveFreeRegistrationKeepsOneFreeLicenseRegistered()
        {
            await this.TestCommandAsync( "license register free", "" );
            await this.TestCommandAsync( "license register free", "" );

            await this.TestCommandAsync( "license list", TestLicenses.FreeOutput );
        }
    }
}