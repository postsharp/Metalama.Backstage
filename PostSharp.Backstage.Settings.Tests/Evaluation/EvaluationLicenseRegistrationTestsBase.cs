// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration.Evaluation;
using PostSharp.Backstage.Licensing.Tests.Registration;
using PostSharp.Backstage.Testing.Services;
using System;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Evaluation
{
    public abstract class EvaluationLicenseRegistrationTestsBase : LicenseRegistrationTestsBase
    {
        protected static readonly DateTime TestStart = new( 2020, 1, 1 );

        private protected EvaluationLicenseRegistrar Registrar { get; }

        private protected EvaluationLicenseRegistrationTestsBase(
            ITestOutputHelper logger,
            Action<ServiceProviderBuilder>? serviceBuilder = null ) :
            base(
                logger,
                serviceCollection => serviceBuilder?.Invoke( serviceCollection ) )
        {
            this.Registrar = new EvaluationLicenseRegistrar( this.Services );
        }

        protected void SetLicenses( params string[] licenses )
        {
            LicensingConfiguration configuration = new() { Licenses = licenses };

            this.FileSystem.Mock.AddFile(
                this.LicensingConfigurationFile,
                new MockFileDataEx( configuration.ToJson() ) );
        }

        protected void AssertEvaluationEligible()
        {
            Assert.True( this.Registrar.TryRegisterLicense() );

            /*
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
            */
        }

        protected void AssertEvaluationNotEligible( string reason )
        {
            Assert.False( this.Registrar.TryRegisterLicense() );
        }
    }
}