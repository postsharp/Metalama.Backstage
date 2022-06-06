﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Licensing
{
    public class RegisterEssentialsCommandTests : LicensingCommandsTestsBase
    {
        public RegisterEssentialsCommandTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public async Task EssentialsRegistersInEmptyEnvironment()
        {
            await this.TestCommandAsync( "license register essentials", "" );

            await this.TestCommandAsync(
                "license list",
                string.Format( CultureInfo.InvariantCulture, TestLicenses.EssentialsFormat, 1 ) );
        }

        [Fact]
        public async Task RepetitiveEssentialsRegistrationKeepsOneEssentialsLicenseRegistered()
        {
            await this.TestCommandAsync( "license register essentials", "" );
            await this.TestCommandAsync( "license register essentials", "" );

            await this.TestCommandAsync(
                "license list",
                string.Format( CultureInfo.InvariantCulture, TestLicenses.EssentialsFormat, 1 ) );
        }
    }
}