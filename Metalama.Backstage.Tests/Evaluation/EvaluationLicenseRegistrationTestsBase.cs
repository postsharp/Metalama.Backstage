// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Licensing.Registration.Evaluation;
using Metalama.Backstage.Licensing.Tests.Registration;
using Metalama.Backstage.Testing.Services;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Evaluation
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
            Assert.True( this.Registrar.TryActivateLicense() );
            var expectedStart = this.Time.Now;
            var expectedEnd = expectedStart + EvaluationLicenseRegistrar.EvaluationPeriod;

            var licenses = ParsedLicensingConfiguration.OpenOrCreate( this.Services );

            var registeredLicense = licenses.Licenses
                .Single( x => x.LicenseData is { LicenseType: LicenseType.Evaluation } && x.LicenseData.ValidFrom == expectedStart )
                .LicenseData!;

            Assert.Equal( expectedEnd, registeredLicense.ValidTo );
            Assert.Equal( expectedEnd, registeredLicense.SubscriptionEndDate );
        }

        protected void AssertEvaluationNotEligible( string reason )
        {
            Assert.False( this.Registrar.TryActivateLicense() );
        }
    }
}