// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Evaluation;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Tests.Services;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Evaluation
{
    public class EvaluationLicenseManagerTests : LicensingTestsBase
    {
        private static readonly DateTime _evaluationStart = new( 2020, 1, 1 );
        private static readonly DateTime _evaluationEnd = new( 2020, 2, 15 );

        public EvaluationLicenseManagerTests( ITestOutputHelper logger ) :
            base( logger )
        {
        }

        private void AsserEvaluationRegistered()
        {
            var registeredLicenses = this.Services.FileSystem.ReadAllLines( StandardLicenseFilesLocations.UserLicenseFile );
            var registeredLicense = registeredLicenses.Single();

            var evaluationLicenseFlags = this.Services.FileSystem.ReadAllLines( StandardLicenseFilesLocations.EvaluationLicenseFile );
            var evaluationLicenseFlag = evaluationLicenseFlags.Single();

            Assert.Equal( evaluationLicenseFlag, registeredLicense );

            Assert.True( this.LicenseFactory.TryCreate( registeredLicense, out var license ) );
            Assert.True( license!.TryGetLicenseRegistrationData( out var data ) );
            Assert.Equal( LicenseType.Evaluation, data!.LicenseType );
            Assert.Equal( _evaluationStart, data.ValidFrom );
            Assert.Equal( _evaluationEnd, data.ValidTo );
            Assert.Equal( _evaluationEnd, data.SubscriptionEndDate );
        }

        [Fact]
        public void EvaluationLicenseRegistersInCleanEnvironment()
        {
            EvaluationLicenseManager manager = new( this.Services, this.Trace );

            this.Services.Time.Set( _evaluationStart );
            Assert.True( manager.TryRegisterLicense() );
            this.AsserEvaluationRegistered();
        }
    }
}
