// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Licensing.Registration.Evaluation;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Evaluation
{
    public abstract class EvaluationLicenseRegistrationTestsBase : LicensingTestsBase
    {
        protected static readonly DateTime TestStart = new( 2020, 1, 1 );

        private EvaluationLicenseRegistrar Registrar { get; }

        private protected EvaluationLicenseRegistrationTestsBase(
            ITestOutputHelper logger,
            Action<ServiceProviderBuilder>? serviceBuilder = null ) :
            base(
                logger,
                serviceCollection => serviceBuilder?.Invoke( serviceCollection ) )
        {
            this.Registrar = new EvaluationLicenseRegistrar( this.ServiceProvider );
        }

        protected void AssertEvaluationEligible()
        {
            Assert.True( this.Registrar.TryActivateLicense() );
            var expectedStart = this.Time.Now.Date;
            var expectedEnd = expectedStart + EvaluationLicenseRegistrar.EvaluationPeriod;

            var licenses = ParsedLicensingConfiguration.OpenOrCreate( this.ServiceProvider );

            Assert.NotNull( licenses.LicenseData );
            Assert.Equal( LicenseType.Evaluation, licenses.LicenseData!.LicenseType );
            Assert.Equal( expectedStart, licenses.LicenseData!.ValidFrom!.Value.Date );
            Assert.Equal( expectedEnd, licenses.LicenseData!.ValidTo!.Value.Date );
            Assert.Equal( expectedEnd, licenses.LicenseData!.SubscriptionEndDate );
        }

        protected void AssertEvaluationNotEligible( string reason )
        {
            Assert.False( this.Registrar.TryActivateLicense() );
        }
    }
}