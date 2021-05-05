﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using PostSharp.Backstage.Licensing.Evaluation;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Tests.Registration;
using PostSharp.Backstage.Licensing.Tests.Services;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Evaluation
{
    public abstract class EvaluationLicenseRegistrationTestsBase : LicenseRegistrationTestsBase
    {
        protected static readonly DateTime TestStart = new( 2020, 1, 1 );

        protected EvaluationLicenseManager Manager { get; }

        public EvaluationLicenseRegistrationTestsBase( ITestOutputHelper logger ) :
            base( logger )
        {
            this.Manager = new( this.Services, this.Trace );
        }

        protected void SetFlag( params string[] flag )
        {
            this.Services.FileSystem.Mock.AddFile( StandardLicenseFilesLocations.EvaluationLicenseFile, new MockFileDataEx( flag ) );
        }

        protected void AssertEvaluationElligible( string reason )
        {
            Assert.True( this.Manager.TryRegisterLicense() );

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

        protected void AssertEvaluationNotElligible( string reason )
        {
            Assert.False( this.Manager.TryRegisterLicense() );

            this.AssertEvaluationEligibilityReason( reason );
        }

        private void AssertEvaluationEligibilityReason(string reason)
        {
            Assert.Contains( "Checking for trial license eligibility.", this.Trace.Messages );
            Assert.Contains( reason, this.Trace.Messages );
            this.Trace.Clear();
        }
    }
}