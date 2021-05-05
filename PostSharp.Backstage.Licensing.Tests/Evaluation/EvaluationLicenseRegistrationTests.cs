// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Registration;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Evaluation
{
    public class EvaluationLicenseRegistrationTests : EvaluationLicenseRegistrationTestsBase
    {
        public EvaluationLicenseRegistrationTests( ITestOutputHelper logger )
            : base( logger )
        {
        }

        [Fact]
        public void MissingFlagOfRunningEvaluationPasses()
        {
            this.Services.Time.Set( TestStart );
            (var evaluationLicenseKey, _) = this.SelfSignedLicenseFactory.CreateEvaluationLicense();
            this.SetStoredLicenseStrings( evaluationLicenseKey );
            Assert.True( this.Manager.TryRegisterLicense() );
            Assert.Contains( "Failed to register evaluation license: A valid evaluation license is registered already.", this.Trace.Messages );
        }

        [Fact]
        public void ConcurrentStoredLicenseSavingPassesWhenReadFails()
        {
            (var evaluationLicenseKey, _) = this.SelfSignedLicenseFactory.CreateEvaluationLicense();
            this.SetStoredLicenseStrings( evaluationLicenseKey );

            var readEvent = this.Services.FileSystem.BlockRead( StandardLicenseFilesLocations.UserLicenseFile );
            var registration = Task.Run( this.Manager.TryRegisterLicense );
            readEvent.Wait();

            this.Services.FileSystem.Unblock( StandardLicenseFilesLocations.UserLicenseFile );

            Assert.True( registration.GetAwaiter().GetResult() );
            Assert.Contains( $"Failed to register evaluation license: Attempt #1: ReadAllLines failed. File '{StandardLicenseFilesLocations.UserLicenseFile}' in use. Retrying.", this.Trace.Messages );
            Assert.Contains( "Failed to register evaluation license: A valid evaluation license is registered already.", this.Trace.Messages );
            Assert.Contains( $"ReadAllLines({StandardLicenseFilesLocations.UserLicenseFile})", this.Services.FileSystem.FailedFileAccesses );
        }

        [Fact]
        public void ConcurrentStoredLicenseSavingPassesWhenWriteFails()
        {
            var writeEvent = this.Services.FileSystem.BlockWrite( StandardLicenseFilesLocations.UserLicenseFile );
            var registration = Task.Run( this.Manager.TryRegisterLicense );
            writeEvent.Wait();

            (var evaluationLicenseKey, _) = this.SelfSignedLicenseFactory.CreateEvaluationLicense();
            this.SetStoredLicenseStrings( evaluationLicenseKey );

            this.Services.FileSystem.Unblock( StandardLicenseFilesLocations.UserLicenseFile );

            Assert.True( registration.GetAwaiter().GetResult() );
            Assert.Contains( $"Failed to register evaluation license: Attempt #1: WriteAllLines failed. File '{StandardLicenseFilesLocations.UserLicenseFile}' in use. Retrying.", this.Trace.Messages );
            Assert.Contains( "Failed to register evaluation license: A valid evaluation license is registered already.", this.Trace.Messages );
            Assert.Contains( $"WriteAllLines({StandardLicenseFilesLocations.UserLicenseFile})", this.Services.FileSystem.FailedFileAccesses );
        }

        [Fact]
        public void ConcurrentEvaluationLicenseFlagSavingPassesWhenWriteFails()
        {
            var writeEvent = this.Services.FileSystem.BlockWrite( StandardLicenseFilesLocations.EvaluationLicenseFile );
            var registration = Task.Run( this.Manager.TryRegisterLicense );
            writeEvent.Wait();

            this.Services.FileSystem.Unblock( StandardLicenseFilesLocations.EvaluationLicenseFile );
            Assert.True( registration.GetAwaiter().GetResult() );
            Assert.Contains( $"Failed to store evaluation license information: System.IO.IOException", this.Trace.Messages );
            Assert.Contains( $"WriteAllLines({StandardLicenseFilesLocations.EvaluationLicenseFile})", this.Services.FileSystem.FailedFileAccesses );

            // In a real concurrent case, the flag would be written by another instance.
            Assert.False( this.Services.FileSystem.FileExists( StandardLicenseFilesLocations.EvaluationLicenseFile ) );
        }
    }
}
