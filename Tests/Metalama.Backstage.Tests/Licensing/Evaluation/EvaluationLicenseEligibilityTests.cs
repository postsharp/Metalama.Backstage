// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Licensing.Registration.Evaluation;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing.Evaluation
{
    public class EvaluationLicenseEligibilityTests : EvaluationLicenseRegistrationTestsBase
    {
        public EvaluationLicenseEligibilityTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void EvaluationLicenseRegistersInCleanEnvironment()
        {
            this.Time.Set( TestStart, true );
            this.AssertEvaluationEligible();
        }

        private void TestRepetitiveRegistration( TimeSpan retryAfter, bool unregisterBeforeRetry, bool expectedEligibility )
        {
            this.Time.Set( TestStart, true );
            this.AssertEvaluationEligible();

            if ( unregisterBeforeRetry )
            {
                ParsedLicensingConfiguration.OpenOrCreate( this.ServiceProvider ).RemoveLicense();
            }

            var license = new UserProfileLicenseSource( this.ServiceProvider ).GetLicense(
                m => Assert.False( true, $"Unexpected message from license provider: '{m}'" ) );

            if ( unregisterBeforeRetry )
            {
                Assert.Null( license );
            }
            else
            {
                Assert.NotNull( license );
            }

            this.Time.Set( this.Time.Now + retryAfter, true );

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
        public void ImmediateRepetitiveEvaluationLicenseRegistrationSucceeds()
        {
            // This is allowed to avoid race conditions first time user experience, where the evaluation license is registered automatically.
            this.TestRepetitiveRegistration( TimeSpan.Zero, false, true );
        }

        [Fact]
        public void ImmediateRepetitiveEvaluationLicenseRegistrationFailsAfterUnregistration()
        {
            this.TestRepetitiveRegistration( TimeSpan.Zero, true, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinRunningEvaluationFails()
        {
            this.TestRepetitiveRegistration( new TimeSpan( EvaluationLicenseRegistrar.EvaluationPeriod.Ticks / 2 ), false, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinRunningEvaluationFailsAfterUnregistration()
        {
            this.TestRepetitiveRegistration( new TimeSpan( EvaluationLicenseRegistrar.EvaluationPeriod.Ticks / 2 ), true, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinNoEvaluationPeriodFails()
        {
            this.TestRepetitiveRegistration(
                EvaluationLicenseRegistrar.EvaluationPeriod + new TimeSpan( EvaluationLicenseRegistrar.NoEvaluationPeriod.Ticks / 2 ),
                false,
                false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinNoEvaluationPeriodFailsAfterUnregistration()
        {
            this.TestRepetitiveRegistration(
                EvaluationLicenseRegistrar.EvaluationPeriod + new TimeSpan( EvaluationLicenseRegistrar.NoEvaluationPeriod.Ticks / 2 ),
                true,
                false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAtTheEndOfNoEvaluationPeriodFails()
        {
            this.TestRepetitiveRegistration(
                EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod.Subtract( TimeSpan.FromMinutes( 1 ) ),
                false,
                false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAtTheEndOfNoEvaluationPeriodFailsAfterUnregistration()
        {
            this.TestRepetitiveRegistration(
                EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod.Subtract( TimeSpan.FromMinutes( 1 ) ),
                true,
                false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAfterNoEvaluationPeriodSucceeds()
        {
            this.TestRepetitiveRegistration(
                EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod +
                TimeSpan.FromDays( 1 ),
                false,
                true );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAfterNoEvaluationPeriodSucceedsAfterUnregistration()
        {
            this.TestRepetitiveRegistration(
                EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod +
                TimeSpan.FromDays( 1 ),
                true,
                true );
        }
    }
}