// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Evaluation
{
    public class EvaluationLicenseRegistrationTests : EvaluationLicenseRegistrationTestsBase
    {
        public EvaluationLicenseRegistrationTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void MissingFlagOfRunningEvaluationPasses()
        {
            this.Time.Set( TestStart );
            var (evaluationLicenseKey, _) = this.SelfSignedLicenseFactory.CreateEvaluationLicense();
            this.SetStoredLicenseStrings( evaluationLicenseKey );
            Assert.True( this.Registrar.TryRegisterLicense() );
            Assert.Single( this.Log.LogEntries, x => x.Message == "Failed to register evaluation license: A valid evaluation license is registered already." );
        }

        [Fact]
        public async Task ConcurrentStoredLicenseSavingPassesWhenReadFails()
        {
            var (evaluationLicenseKey, _) = this.SelfSignedLicenseFactory.CreateEvaluationLicense();
            this.SetStoredLicenseStrings( evaluationLicenseKey );

            var readEvent = this.FileSystem.BlockRead( this.LicenseFiles.UserLicenseFile );
            var registration = Task.Run( this.Registrar.TryRegisterLicense );
            readEvent.Wait();

            this.FileSystem.Unblock( this.LicenseFiles.UserLicenseFile );

            Assert.True( await registration );

            var messages = this.Log.LogEntries.Select( e => e.Message ).ToArray();

            Assert.Single(
                messages,
                $"Failed to register evaluation license: Attempt #1: ReadAllLines failed. File '{this.LicenseFiles.UserLicenseFile}' in use. Retrying." );

            Assert.Single( messages, "Failed to register evaluation license: A valid evaluation license is registered already." );
            Assert.Single( this.FileSystem.FailedFileAccesses, $"ReadAllLines({this.LicenseFiles.UserLicenseFile})" );
        }

        [Fact]
        public async Task ConcurrentStoredLicenseSavingPassesWhenWriteFails()
        {
            var writeEvent = this.FileSystem.BlockWrite( this.LicenseFiles.UserLicenseFile );
            var registration = Task.Run( this.Registrar.TryRegisterLicense );
            writeEvent.Wait();

            var (evaluationLicenseKey, _) = this.SelfSignedLicenseFactory.CreateEvaluationLicense();
            this.SetStoredLicenseStrings( evaluationLicenseKey );

            this.FileSystem.Unblock( this.LicenseFiles.UserLicenseFile );

            Assert.True( await registration );

            var messages = this.Log.LogEntries.Select( e => e.Message ).ToArray();

            Assert.Single(
                messages,
                $"Failed to register evaluation license: Attempt #1: WriteAllLines failed. File '{this.LicenseFiles.UserLicenseFile}' in use. Retrying." );

            Assert.Single( messages, "Failed to register evaluation license: A valid evaluation license is registered already." );
            Assert.Single( this.FileSystem.FailedFileAccesses, $"WriteAllLines({this.LicenseFiles.UserLicenseFile})" );
        }

        [Fact]
        public async Task ConcurrentEvaluationLicenseFlagSavingPassesWhenWriteFails()
        {
            var writeEvent = this.FileSystem.BlockWrite( this.EvaluationFiles.EvaluationLicenseFile );
            var registration = Task.Run( this.Registrar.TryRegisterLicense );
            writeEvent.Wait();

            this.FileSystem.Unblock( this.EvaluationFiles.EvaluationLicenseFile );
            Assert.True( await registration );

            var messages = this.Log.LogEntries.Select( e => e.Message ).ToArray();

            Assert.Single( messages, $"Failed to store evaluation license information: System.IO.IOException" );
            Assert.Single( this.FileSystem.FailedFileAccesses, $"WriteAllLines({this.EvaluationFiles.EvaluationLicenseFile})" );

            // In a real concurrent case, the flag would be written by another instance.
            Assert.False( this.FileSystem.FileExists( this.EvaluationFiles.EvaluationLicenseFile ) );
        }
    }
}