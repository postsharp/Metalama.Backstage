// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands.Licensing
{
    public class ShowCommandTests : CommandsTestsBase
    {
        public ShowCommandTests( ITestOutputHelper logger )
                    : base( logger )
        {
        }

        [Fact]
        public async Task ShowFailsWithZeroLicenses()
        {
            await this.TestCommandAsync( "license show 0", "", "Unknown ordinal." + Environment.NewLine, 1 );
            await this.TestCommandAsync( "license show 1", "", "Unknown ordinal." + Environment.NewLine, 1 );
            await this.TestCommandAsync( "license show 2", "", "Unknown ordinal." + Environment.NewLine, 1 );
        }

        [Fact]
        public async Task ShowWorksWithOneLicense()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.Key1}", "" );
            await this.TestCommandAsync( "license list", string.Format( TestLicenses.Format1, 1 ) );
            await this.TestCommandAsync( "license show 0", "", "Unknown ordinal." + Environment.NewLine, 1 );
            await this.TestCommandAsync( "license show 1", TestLicenses.Key1 + Environment.NewLine );
            await this.TestCommandAsync( "license show 2", "", "Unknown ordinal." + Environment.NewLine, 1 );
        }

        [Fact]
        public async Task ShowWorksWithMultipleLicenses()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.Key1}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.Key2}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.Key3}", "" );
            await this.TestCommandAsync(
                "license list",
                string.Format( TestLicenses.Format1, 1 ) + string.Format( TestLicenses.Format2, 2 ) + string.Format( TestLicenses.Format3, 3 ) );
            await this.TestCommandAsync( "license show 0", "", "Unknown ordinal." + Environment.NewLine, 1 );
            await this.TestCommandAsync( "license show 1", TestLicenses.Key1 + Environment.NewLine );
            await this.TestCommandAsync( "license show 2", TestLicenses.Key2 + Environment.NewLine );
            await this.TestCommandAsync( "license show 3", TestLicenses.Key3 + Environment.NewLine );
            await this.TestCommandAsync( "license show 4", "", "Unknown ordinal." + Environment.NewLine, 1 );
        }
    }
}
