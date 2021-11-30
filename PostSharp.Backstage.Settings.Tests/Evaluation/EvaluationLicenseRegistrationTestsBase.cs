// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration.Evaluation;
using PostSharp.Backstage.Licensing.Tests.Registration;
using PostSharp.Backstage.Testing.Services;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Evaluation
{
    public abstract class EvaluationLicenseRegistrationTestsBase : LicenseRegistrationTestsBase
    {
        protected static readonly DateTime TestStart = new( 2020, 1, 1 );

        private protected EvaluationLicenseRegistrar Registrar { get; }

        private protected IEvaluationLicenseFilesLocations EvaluationFiles { get; }

        private protected EvaluationLicenseRegistrationTestsBase(
            ITestOutputHelper logger,
            Action<BackstageServiceCollection>? serviceBuilder = null ) :
            base(
                logger,
                serviceCollection => serviceBuilder?.Invoke( serviceCollection ) )
        {
            this.Registrar = new EvaluationLicenseRegistrar( this.Services );
            this.EvaluationFiles = this.Services.GetRequiredService<IEvaluationLicenseFilesLocations>();
        }

        protected void SetFlag( params string[] flag )
        {
            this.FileSystem.Mock.AddFile( this.EvaluationFiles.EvaluationLicenseFile, new MockFileDataEx( flag ) );
        }

        protected void AssertEvaluationEligible( string reason )
        {
            Assert.True( this.Registrar.TryRegisterLicense() );

            var registeredLicenses = this.FileSystem.ReadAllLines( this.LicenseFiles.UserLicenseFile );
            var evaluationLicenseFlags = this.FileSystem.ReadAllLines( this.EvaluationFiles.EvaluationLicenseFile );
            var registeredLicense = evaluationLicenseFlags.Single();

            Assert.Contains( registeredLicense, registeredLicenses );

            Assert.True( this.LicenseFactory.TryCreate( registeredLicense, out var license ) );
            Assert.True( license!.TryGetLicenseRegistrationData( out var data ) );
            Assert.Equal( LicenseType.Evaluation, data!.LicenseType );

            var expectedStart = this.Time.Now;
            var expectedEnd = expectedStart + EvaluationLicenseRegistrar.EvaluationPeriod;

            Assert.Equal( expectedStart, data.ValidFrom );
            Assert.Equal( expectedEnd, data.ValidTo );
            Assert.Equal( expectedEnd, data.SubscriptionEndDate );

            this.AssertEvaluationEligibilityReason( reason );
        }

        protected void AssertEvaluationNotEligible( string reason )
        {
            Assert.False( this.Registrar.TryRegisterLicense() );

            this.AssertEvaluationEligibilityReason( reason );
        }

        // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
        private void AssertEvaluationEligibilityReason( string reason )
        {
            Assert.Single( this.Log.LogEntries, x => x.Message == "Checking for trial license eligibility." );
            Assert.Single( this.Log.LogEntries, x => x.Message == reason );
            this.Log.Clear();
        }
        
        // ReSharper restore ParameterOnlyUsedForPreconditionCheck.Local
    }
}