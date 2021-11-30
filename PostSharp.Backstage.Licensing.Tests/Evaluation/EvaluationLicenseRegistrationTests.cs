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
            Time.Set( TestStart );
            var (evaluationLicenseKey, _) = SelfSignedLicenseFactory.CreateEvaluationLicense();
            SetStoredLicenseStrings( evaluationLicenseKey );
            Assert.True( Registrar.TryRegisterLicense() );
            Assert.Single( Log.LogEntries, x => x.Message == "Failed to register evaluation license: A valid evaluation license is registered already." );
        }

        [Fact]
        public async Task ConcurrentStoredLicenseSavingPassesWhenReadFails()
        {
            var (evaluationLicenseKey, _) = SelfSignedLicenseFactory.CreateEvaluationLicense();
            SetStoredLicenseStrings( evaluationLicenseKey );

            var readEvent = FileSystem.BlockRead( LicenseFiles.UserLicenseFile );
            var registration = Task.Run( Registrar.TryRegisterLicense );
            readEvent.Wait();

            FileSystem.Unblock( LicenseFiles.UserLicenseFile );

            Assert.True( await registration );

            var messages = Log.LogEntries.Select( e => e.Message ).ToArray();

            Assert.Single(
                messages,
                $"Failed to register evaluation license: Attempt #1: ReadAllLines failed. File '{LicenseFiles.UserLicenseFile}' in use. Retrying." );

            Assert.Single( messages, "Failed to register evaluation license: A valid evaluation license is registered already." );
            Assert.Single( FileSystem.FailedFileAccesses, $"ReadAllLines({LicenseFiles.UserLicenseFile})" );
        }

        [Fact]
        public async Task ConcurrentStoredLicenseSavingPassesWhenWriteFails()
        {
            var writeEvent = FileSystem.BlockWrite( LicenseFiles.UserLicenseFile );
            var registration = Task.Run( Registrar.TryRegisterLicense );
            writeEvent.Wait();

            var (evaluationLicenseKey, _) = SelfSignedLicenseFactory.CreateEvaluationLicense();
            SetStoredLicenseStrings( evaluationLicenseKey );

            FileSystem.Unblock( LicenseFiles.UserLicenseFile );

            Assert.True( await registration );

            var messages = Log.LogEntries.Select( e => e.Message ).ToArray();

            Assert.Single(
                messages,
                $"Failed to register evaluation license: Attempt #1: WriteAllLines failed. File '{LicenseFiles.UserLicenseFile}' in use. Retrying." );

            Assert.Single( messages, "Failed to register evaluation license: A valid evaluation license is registered already." );
            Assert.Single( FileSystem.FailedFileAccesses, $"WriteAllLines({LicenseFiles.UserLicenseFile})" );
        }

        [Fact]
        public async Task ConcurrentEvaluationLicenseFlagSavingPassesWhenWriteFails()
        {
            var writeEvent = FileSystem.BlockWrite( EvaluationFiles.EvaluationLicenseFile );
            var registration = Task.Run( Registrar.TryRegisterLicense );
            writeEvent.Wait();

            FileSystem.Unblock( EvaluationFiles.EvaluationLicenseFile );
            Assert.True( await registration );

            var messages = Log.LogEntries.Select( e => e.Message ).ToArray();

            Assert.Single( messages, $"Failed to store evaluation license information: System.IO.IOException" );
            Assert.Single( FileSystem.FailedFileAccesses, $"WriteAllLines({EvaluationFiles.EvaluationLicenseFile})" );

            // In a real concurrent case, the flag would be written by another instance.
            Assert.False( FileSystem.FileExists( EvaluationFiles.EvaluationLicenseFile ) );
        }
    }
}