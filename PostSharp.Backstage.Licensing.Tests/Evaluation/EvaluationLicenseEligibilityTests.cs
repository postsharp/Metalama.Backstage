// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration.Evaluation;
using System;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Evaluation
{
    public class EvaluationLicenseEligibilityTests : EvaluationLicenseRegistrationTestsBase
    {
        public EvaluationLicenseEligibilityTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void EvaluationLicenseRegistersInCleanEnvironment()
        {
            Time.Set( TestStart );
            AssertEvaluationEligible( "No trial license found." );
        }

        [Fact]
        public void MultipleEvaluationLicenseFlagsDisableEvaluationLicenseRegistration()
        {
            // Equal license keys would fail the test because the internal storage uses dictionary indexed by license string.
            var (evaluationLicenseKey1, _) = SelfSignedLicenseFactory.CreateEvaluationLicense();
            var (evaluationLicenseKey2, _) = SelfSignedLicenseFactory.CreateEvaluationLicense();
            SetFlag( evaluationLicenseKey1, evaluationLicenseKey2 );
            AssertEvaluationNotEligible( "Failed to find the latest trial license: Invalid count." );
        }

        [Fact]
        public void InvalidEvaluationLicenseFlagDisablesEvaluationLicenseRegistration()
        {
            SetFlag( "dummy" );
            AssertEvaluationNotEligible( "Failed to find the latest trial license: Invalid data." );
        }

        [Fact]
        public void InvalidEvaluationLicenseFlagTypeDisablesEvaluationLicenseRegistration()
        {
            SetFlag( TestLicenseKeys.Ultimate );
            AssertEvaluationNotEligible( "Failed to find the latest trial license: Invalid license type." );
        }

        [Fact]
        public void MissingEvaluationLicenseValidityDisablesEvaluationLicenseRegistration()
        {
            var licenseKeyData = new LicenseKeyData
            {
                MinPostSharpVersion = LicenseKeyData.MinPostSharpVersionValidationRemovedPostSharpVersion,
                LicenseGuid = Guid.NewGuid(),
                Product = LicensedProduct.Caravela,
                LicenseType = LicenseType.Evaluation
            };

            var evaluationLicenseKeyWithMissingValidity = licenseKeyData.Serialize();

            SetFlag( evaluationLicenseKeyWithMissingValidity );
            AssertEvaluationNotEligible( "Failed to find the latest trial license: Invalid validity." );
        }

        private void TestRepetitiveRegistration( TimeSpan retryAfter, bool expectedEligibility )
        {
            Time.Set( TestStart );
            AssertEvaluationEligible( "No trial license found." );

            Time.Set( Time.Now + retryAfter );

            if (expectedEligibility)
            {
                AssertEvaluationEligible( "Evaluation license registration can be repeated." );
            }
            else
            {
                AssertEvaluationNotEligible( "Evaluation license requested recently." );
            }
        }

        [Fact]
        public void RepetitiveEvaluationLicenseRegistrationFails()
        {
            TestRepetitiveRegistration( TimeSpan.Zero, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinRunningEvaluationFails()
        {
            TestRepetitiveRegistration( EvaluationLicenseRegistrar.EvaluationPeriod / 2, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinNoEvaluationPeriodFails()
        {
            TestRepetitiveRegistration( EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod / 2, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAtTheEndOfNoEvaluationPeriodFails()
        {
            TestRepetitiveRegistration( EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAfterNoEvaluationPeriodSucceeds()
        {
            TestRepetitiveRegistration(
                EvaluationLicenseRegistrar.EvaluationPeriod + EvaluationLicenseRegistrar.NoEvaluationPeriod + TimeSpan.FromDays( 1 ),
                true );
        }
    }
}