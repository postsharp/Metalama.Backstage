// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration.Evaluation;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Evaluation
{
    public class EvaluationLicenseEligibilityTests : EvaluationLicenseRegistrationTestsBase
    {
        public EvaluationLicenseEligibilityTests( ITestOutputHelper logger )
            : base( logger )
        {
        }

        [Fact]
        public void EvaluationLicenseRegistersInCleanEnvironment()
        {
            this.Time.Set( TestStart );
            this.AssertEvaluationElligible( reason: "No trial license found." );
        }

        [Fact]
        public void MultipleEvaluationLicenseFlagsDisableEvaluationLicenseRegistration()
        {
            // Equal license keys would fail the test because the internal storage uses dictionary indexed by license string.
            (var evaluationLicenseKey1, _) = this.SelfSignedLicenseFactory.CreateEvaluationLicense();
            (var evaluationLicenseKey2, _) = this.SelfSignedLicenseFactory.CreateEvaluationLicense();
            this.SetFlag( evaluationLicenseKey1, evaluationLicenseKey2 );
            this.AssertEvaluationNotElligible( "Failed to find the latest trial license: Invalid count." );
        }

        [Fact]
        public void InvalidEvaluationLicenseFlagDisablesEvaluationLicenseRegistration()
        {
            this.SetFlag( "dummy" );
            this.AssertEvaluationNotElligible( "Failed to find the latest trial license: Invalid data." );
        }

        [Fact]
        public void InvalidEvaluationLicenseFlagTypeDisablesEvaluationLicenseRegistration()
        {
            this.SetFlag( TestLicenseKeys.Ultimate );
            this.AssertEvaluationNotElligible( "Failed to find the latest trial license: Invalid license type." );
        }

        [Fact]
        public void MissingEvaluationLicenseValidityDisablesEvaluationLicenseRegistration()
        {
            var licenseKeyData = new LicenseKeyData
            {
                MinPostSharpVersion = LicenseKeyData.MinPostSharpVersionValidationRemovedPostSharpVersion,
                LicenseGuid = Guid.NewGuid(),
                Product = LicensedProduct.Caravela,
                LicenseType = LicenseType.Evaluation,
            };

            var evaluationLicenseKeyWithMissingValidity = licenseKeyData.Serialize();

            this.SetFlag( evaluationLicenseKeyWithMissingValidity );
            this.AssertEvaluationNotElligible( "Failed to find the latest trial license: Invalid validity." );
        }

        private void TestRepetitiveRegistration( TimeSpan retryAfter, bool expectedElligibility )
        {
            this.Time.Set( TestStart );
            this.AssertEvaluationElligible( "No trial license found." );

            this.Time.Set( this.Time.Now + retryAfter );

            if ( expectedElligibility )
            {
                this.AssertEvaluationElligible( "Evaluation license registration can be repeated." );
            }
            else
            {
                this.AssertEvaluationNotElligible( "Evaluation license requested recently." );
            }
        }

        [Fact]
        public void RepetitiveEvaluationLicenseRegistrationFails()
        {
            this.TestRepetitiveRegistration( TimeSpan.Zero, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinRunningEvaluationFails()
        {
            this.TestRepetitiveRegistration( EvaluationLicenseRegistrar.EvaluationPeriod / 2, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinNoEvaluationPeriodFails()
        {
            this.TestRepetitiveRegistration( EvaluationLicenseRegistrar.EvaluationPeriod + (EvaluationLicenseRegistrar.NoEvaluationPeriod / 2), false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAtTheEndOfNoEvaluationPeriodFails()
        {
            this.TestRepetitiveRegistration( EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAfterNoEvaluationPeriodSucceeds()
        {
            this.TestRepetitiveRegistration( EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod + TimeSpan.FromDays( 1 ), true );
        }
    }
}
