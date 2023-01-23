// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
            await this.TestCommandAsync( "license try" );

            await this.TestCommandAsync( "license list", "Evaluation License" );
        }

        [Fact]
        public async Task TrialRegistrationFailsWithinNoEvaluationPeriod()
        {
            await this.TestCommandAsync( "license try" );

            this.Time.Set( TestLicenses.InvalidNextEvaluationStart );

            await this.TestCommandAsync(
                "license try",
                null,
                "Cannot start the trial period.",
                1 );

            await this.TestCommandAsync( "license list", "Evaluation License" );
        }

        [Fact]
        public async Task TrialRegistersAfterNoEvaluationPeriod()
        {
            await this.TestCommandAsync( "license try" );

            this.Time.Set( TestLicenses.ValidNextEvaluationStart );
            await this.TestCommandAsync( "license try" );

            await this.TestCommandAsync( "license list", "Evaluation License" );
        }
    }
}