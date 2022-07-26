// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Registration.Evaluation;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Evaluation
{
    public class EvaluationLicenseEligibilityTests : EvaluationLicenseRegistrationTestsBase
    {
        public EvaluationLicenseEligibilityTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void EvaluationLicenseRegistersInCleanEnvironment()
        {
            this.Time.Set( TestStart );
            this.AssertEvaluationEligible();
        }

        private void TestRepetitiveRegistration( TimeSpan retryAfter, bool expectedEligibility )
        {
            this.Time.Set( TestStart );
            this.AssertEvaluationEligible();

            this.Time.Set( this.Time.Now + retryAfter );

            if ( expectedEligibility )
            {
                this.AssertEvaluationEligible();
            }
            else
            {
                this.AssertEvaluationNotEligible( "Evaluation license requested recently." );
            }
        }

        [Fact]
        public void RepetitiveEvaluationLicenseRegistrationSucceeds()
        {
            this.TestRepetitiveRegistration( TimeSpan.Zero, true );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinRunningEvaluationFails()
        {
            this.TestRepetitiveRegistration( new TimeSpan( EvaluationLicenseRegistrar.EvaluationPeriod.Ticks / 2 ), false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinNoEvaluationPeriodFails()
        {
            this.TestRepetitiveRegistration(
                EvaluationLicenseRegistrar.EvaluationPeriod + new TimeSpan( EvaluationLicenseRegistrar.NoEvaluationPeriod.Ticks / 2 ),
                false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAtTheEndOfNoEvaluationPeriodFails()
        {
            this.TestRepetitiveRegistration( EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAfterNoEvaluationPeriodSucceeds()
        {
            this.TestRepetitiveRegistration(
                EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod +
                TimeSpan.FromDays( 1 ),
                true );
        }
    }
}