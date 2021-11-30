using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration.Evaluation;
using PostSharp.Backstage.Licensing.Tests.Registration;
using PostSharp.Backstage.Testing.Services;
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
            Registrar = new EvaluationLicenseRegistrar( Services );
            EvaluationFiles = Services.GetRequiredService<IEvaluationLicenseFilesLocations>();
        }

        protected void SetFlag( params string[] flag )
        {
            FileSystem.Mock.AddFile( EvaluationFiles.EvaluationLicenseFile, new MockFileDataEx( flag ) );
        }

        protected void AssertEvaluationEligible( string reason )
        {
            Assert.True( Registrar.TryRegisterLicense() );

            var registeredLicenses = FileSystem.ReadAllLines( LicenseFiles.UserLicenseFile );
            var evaluationLicenseFlags = FileSystem.ReadAllLines( EvaluationFiles.EvaluationLicenseFile );
            var registeredLicense = evaluationLicenseFlags.Single();

            Assert.Contains( registeredLicense, registeredLicenses );

            Assert.True( LicenseFactory.TryCreate( registeredLicense, out var license ) );
            Assert.True( license!.TryGetLicenseRegistrationData( out var data ) );
            Assert.Equal( LicenseType.Evaluation, data!.LicenseType );

            var expectedStart = Time.Now;
            var expectedEnd = expectedStart + EvaluationLicenseRegistrar.EvaluationPeriod;

            Assert.Equal( expectedStart, data.ValidFrom );
            Assert.Equal( expectedEnd, data.ValidTo );
            Assert.Equal( expectedEnd, data.SubscriptionEndDate );

            AssertEvaluationEligibilityReason( reason );
        }

        protected void AssertEvaluationNotEligible( string reason )
        {
            Assert.False( Registrar.TryRegisterLicense() );

            AssertEvaluationEligibilityReason( reason );
        }

        private void AssertEvaluationEligibilityReason( string reason )
        {
            Assert.Single( Log.LogEntries, x => x.Message == "Checking for trial license eligibility." );
            Assert.Single( Log.LogEntries, x => x.Message == reason );
            Log.Clear();
        }
    }
}