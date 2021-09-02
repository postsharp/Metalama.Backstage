// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands.Licensing
{
    public class UnregisterCommandTests : LicensingCommandsTestsBase
    {
        public UnregisterCommandTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public async Task UnknownOrdinalFailsToUnregisterInCleanEnvironment()
        {
            await this.TestCommandAsync( $"license unregister 1", "", "Invalid ordinal." + Environment.NewLine, 1 );
        }

        [Fact]
        public async Task UnknownLicenseKeyFailsToUnregisterInCleanEnvironment()
        {
            await this.TestCommandAsync( $"license unregister {TestLicenses.Key1}", "", $"This license is not registered." + Environment.NewLine, 2 );
        }

        [Fact]
        public async Task OneOrdinalUnregisters()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.Key1}", "" );
            await this.TestCommandAsync( "license list", string.Format( CultureInfo.InvariantCulture, TestLicenses.Format1, 1 ) );
            await this.TestCommandAsync( $"license unregister 1", $"{TestLicenses.Key1} unregistered." + Environment.NewLine );
            await this.TestCommandAsync( "license list", "" );
        }

        [Fact]
        public async Task OneLicenseKeyUnregisters()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.Key1}", "" );
            await this.TestCommandAsync( $"license unregister {TestLicenses.Key1}", $"{TestLicenses.Key1} unregistered." + Environment.NewLine );
            await this.TestCommandAsync( "license list", "" );
        }

        [Fact]
        public async Task MultipleLicenseKeysListedAfterRegistration()
        {
            await this.TestCommandAsync( $"license register {TestLicenses.Key1}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.Key2}", "" );
            await this.TestCommandAsync( $"license register {TestLicenses.Key3}", "" );

            await this.TestCommandAsync(
                "license list",
                string.Format( CultureInfo.InvariantCulture, TestLicenses.Format1, 1 ) + string.Format( CultureInfo.InvariantCulture, TestLicenses.Format2, 2 ) + string.Format( CultureInfo.InvariantCulture, TestLicenses.Format3, 3 ) );
        }
    }
}