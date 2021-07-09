// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands.Licensing
{
    public class RegisterCommunityCommandTests : LicensingCommandsTestsBase
    {
        public RegisterCommunityCommandTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public async Task CommunityRegistersInEmptyEnvironment()
        {
            await this.TestCommandAsync( "license register community", "" );
            await this.TestCommandAsync( "license list", string.Format( TestLicenses.CommunityFormat, 1 ) );
        }

        [Fact]
        public async Task RepetitiveCommunityRegistrationKeepsOneCommunityLicenseRegistered()
        {
            await this.TestCommandAsync( "license register community", "" );
            await this.TestCommandAsync( "license register community", "" );
            await this.TestCommandAsync( "license list", string.Format( TestLicenses.CommunityFormat, 1 ) );
        }
    }
}