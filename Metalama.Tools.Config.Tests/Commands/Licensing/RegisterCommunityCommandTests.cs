// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Licensing
{
    public class RegisterCommunityCommandTests : LicensingCommandsTestsBase
    {
        public RegisterCommunityCommandTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public async Task CommunityRegistersInEmptyEnvironment()
        {
            await this.TestCommandAsync( "license register essentials", "" );

            await this.TestCommandAsync(
                "license list",
                string.Format( CultureInfo.InvariantCulture, TestLicenses.CommunityFormat, 1 ) );
        }

        [Fact]
        public async Task RepetitiveCommunityRegistrationKeepsOneCommunityLicenseRegistered()
        {
            await this.TestCommandAsync( "license register essentials", "" );
            await this.TestCommandAsync( "license register essentials", "" );

            await this.TestCommandAsync(
                "license list",
                string.Format( CultureInfo.InvariantCulture, TestLicenses.CommunityFormat, 1 ) );
        }
    }
}