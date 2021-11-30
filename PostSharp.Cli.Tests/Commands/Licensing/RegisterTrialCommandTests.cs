// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands.Licensing
{
    public class RegisterTrialCommandTests : LicensingCommandsTestsBase
    {
        public RegisterTrialCommandTests( ITestOutputHelper logger )
            : base( logger )
        {
            Time.Set( TestLicenses.EvaluationStart );
        }

        [Fact]
        public async Task TrialRegistersInEmptyEnvironment()
        {
            await TestCommandAsync( "license register trial", "" );
            await TestCommandAsync( "license list", string.Format( CultureInfo.InvariantCulture, TestLicenses.EvaluationFormat, 1 ) );
        }

        [Fact]
        public async Task TrialRegistrationFailsWithinNoEvaluationPeriod()
        {
            await TestCommandAsync( "license register trial", "" );

            Time.Set( TestLicenses.InvalidNextEvaluationStart );

            await TestCommandAsync(
                "license register trial",
                "",
                "Cannot start the trial period. Use --verbose (-v) flag for details." + Environment.NewLine,
                1 );

            await TestCommandAsync( "license list", string.Format( CultureInfo.InvariantCulture, TestLicenses.EvaluationFormat, 1 ) );
        }

        [Fact]
        public async Task TrialRegistersAfterNoEvaluationPeriod()
        {
            await TestCommandAsync( "license register trial", "" );

            Time.Set( TestLicenses.ValidNextEvaluationStart );
            await TestCommandAsync( "license register trial", "" );

            await TestCommandAsync(
                "license list",
                string.Format( CultureInfo.InvariantCulture, TestLicenses.EvaluationFormat, 1 ) + string.Format(
                    CultureInfo.InvariantCulture,
                    TestLicenses.NextEvaluationFormat,
                    2 ) );
        }
    }
}