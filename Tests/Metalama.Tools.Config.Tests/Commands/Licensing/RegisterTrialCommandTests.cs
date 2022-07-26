// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Licensing
{
    public class RegisterTrialCommandTests : LicensingCommandsTestsBase
    {
        public RegisterTrialCommandTests( ITestOutputHelper logger )
            : base( logger )
        {
            this.Time.Set( TestLicenses.EvaluationStart );
        }

        [Fact]
        public async Task TrialRegistersInEmptyEnvironment()
        {
            await this.TestCommandAsync( "license register trial", "" );

            await this.TestCommandAsync(
                "license list",
                string.Format( CultureInfo.InvariantCulture, TestLicenses.EvaluationFormat, 1 ) );
        }

        [Fact]
        public async Task TrialRegistrationFailsWithinNoEvaluationPeriod()
        {
            await this.TestCommandAsync( "license register trial", "" );

            this.Time.Set( TestLicenses.InvalidNextEvaluationStart );

            await this.TestCommandAsync(
                "license register trial",
                "",
                "Cannot start the trial period. Use --verbose (-v) flag for details." + Environment.NewLine,
                1 );

            await this.TestCommandAsync(
                "license list",
                string.Format( CultureInfo.InvariantCulture, TestLicenses.EvaluationFormat, 1 ) );
        }

        [Fact]
        public async Task TrialRegistersAfterNoEvaluationPeriod()
        {
            await this.TestCommandAsync( "license register trial", "" );

            this.Time.Set( TestLicenses.ValidNextEvaluationStart );
            await this.TestCommandAsync( "license register trial", "" );

            await this.TestCommandAsync(
                "license list",
                string.Format( CultureInfo.InvariantCulture, TestLicenses.EvaluationFormat, 1 ) + string.Format(
                    CultureInfo.InvariantCulture,
                    TestLicenses.NextEvaluationFormat,
                    2 ) );
        }
    }
}