// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using PostSharp.Backstage.Licensing.Evaluation;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Tests.Services;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Evaluation
{
    public class EvaluationLicenseManagerTests : LicensingTestsBase
    {
        private static readonly DateTime _testStart = new( 2020, 1, 1 );

        private readonly EvaluationLicenseManager _manager;

        public EvaluationLicenseManagerTests( ITestOutputHelper logger ) :
            base( logger )
        {
            this._manager = new( this.Services, this.Trace );
        }

        private void SetFlag( params string[] flag )
        {
            this.Services.FileSystem.Mock.AddFile( StandardLicenseFilesLocations.EvaluationLicenseFile, new MockFileDataEx( flag ) );
        }

        private void AssertEvaluationElligible( string reason )
        {
            Assert.True( this._manager.TryRegisterLicense() );

            var registeredLicenses = this.Services.FileSystem.ReadAllLines( StandardLicenseFilesLocations.UserLicenseFile );
            var evaluationLicenseFlags = this.Services.FileSystem.ReadAllLines( StandardLicenseFilesLocations.EvaluationLicenseFile );
            var registeredLicense = evaluationLicenseFlags.Single();

            Assert.Contains( registeredLicense, registeredLicenses );

            Assert.True( this.LicenseFactory.TryCreate( registeredLicense, out var license ) );
            Assert.True( license!.TryGetLicenseRegistrationData( out var data ) );
            Assert.Equal( LicenseType.Evaluation, data!.LicenseType );

            var expectedStart = this.Services.Time.Now;
            var expectedEnd = expectedStart + EvaluationLicenseManager.EvaluationPeriod;

            Assert.Equal( expectedStart, data.ValidFrom );
            Assert.Equal( expectedEnd, data.ValidTo );
            Assert.Equal( expectedEnd, data.SubscriptionEndDate );

            this.AssertEvaluationEligibilityReason( reason );
        }

        private void AssertEvaluationNotElligible( string reason )
        {
            Assert.False( this._manager.TryRegisterLicense() );

            this.AssertEvaluationEligibilityReason( reason );
        }

        private void AssertEvaluationEligibilityReason(string reason)
        {
            Assert.Contains( "Checking for trial license eligibility.", this.Trace.Messages );
            Assert.Contains( reason, this.Trace.Messages );
            this.Trace.Clear();
        }

        [Fact]
        public void EvaluationLicenseRegistersInCleanEnvironment()
        {
            this.Services.Time.Set( _testStart );
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
            this.Services.Time.Set( _testStart );
            this.AssertEvaluationElligible( "No trial license found." );

            this.Services.Time.Set( this.Services.Time.Now + retryAfter );

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
            this.TestRepetitiveRegistration( EvaluationLicenseManager.EvaluationPeriod / 2, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationWithinNoEvaluationPeriodFails()
        {
            this.TestRepetitiveRegistration( EvaluationLicenseManager.EvaluationPeriod + (EvaluationLicenseManager.NoEvaluationPeriod / 2), false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAtTheEndOfNoEvaluationPeriodFails()
        {
            this.TestRepetitiveRegistration( EvaluationLicenseManager.EvaluationPeriod + EvaluationLicenseManager.NoEvaluationPeriod, false );
        }

        [Fact]
        public void EvaluationLicenseRegistrationAfterNoEvaluationPeriodSucceeds()
        {
            this.TestRepetitiveRegistration( EvaluationLicenseManager.EvaluationPeriod + EvaluationLicenseManager.NoEvaluationPeriod + TimeSpan.FromDays( 1 ), true );
        }
    }
}
