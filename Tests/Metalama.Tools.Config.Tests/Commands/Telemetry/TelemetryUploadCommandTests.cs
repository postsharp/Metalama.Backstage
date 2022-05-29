// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Telemetry
{
    public class TelemetryUploadCommandTests : CommandsTestsBase
    {
        private readonly IStandardDirectories _standardDirectories;

        public TelemetryUploadCommandTests( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base( logger, serviceBuilder )
        {
            this._standardDirectories = this.ServiceProvider.GetRequiredService<IStandardDirectories>();
        }

        private void AddFilesToUpload()
        {
            this.FileSystem.CreateDirectory( this._standardDirectories.TelemetryUploadQueueDirectory );
            this.FileSystem.WriteAllText( Path.Combine( this._standardDirectories.TelemetryUploadQueueDirectory, "test.txt" ), "test" );
        }

        private static void AssertEmpty( IEnumerable collection, bool shouldBeEmpty )
        {
            if ( shouldBeEmpty )
            {
                Assert.Empty( collection );
            }
            else
            {
                Assert.NotEmpty( collection );
            }
        }

        private void AssertDirectoryEmpty( string path, bool shouldBeEmpty )
        {
            void AssertExistingDirectoryEmpty()
            {
                AssertEmpty( this.FileSystem.GetDirectories( path, searchOption: SearchOption.AllDirectories ), shouldBeEmpty );
                AssertEmpty( this.FileSystem.GetFiles( path, searchOption: SearchOption.AllDirectories ), shouldBeEmpty );
            }

            var directoryExists = this.FileSystem.DirectoryExists( path );

            if ( shouldBeEmpty )
            {
                if ( directoryExists )
                {
                    AssertExistingDirectoryEmpty();
                }
            }
            else
            {
                Assert.True( directoryExists );
                AssertExistingDirectoryEmpty();
            }
        }

        private void AssertFilesToUploadEmpty( bool shouldBeEmpty )
            => this.AssertDirectoryEmpty( this._standardDirectories.TelemetryUploadQueueDirectory, shouldBeEmpty );

        private void AssertEnquedPackagesEmpty( bool shouldBeEmpty )
            => this.AssertDirectoryEmpty( this._standardDirectories.TelemetryUploadPackagesDirectory, shouldBeEmpty );

        private void AssertFilesUploaded( bool shouldBeUploaded )
            => AssertEmpty( this.ReceivedHttpContent, !shouldBeUploaded );

        [Fact]
        public async Task UploadUploadsTelemetryImmediately()
        {
            this.AddFilesToUpload();
            this.AssertFilesUploaded( false );
            this.AssertEnquedPackagesEmpty( true );
            await this.TestCommandAsync( "telemetry upload", "" );
            this.AssertFilesUploaded( true );
            this.AssertFilesToUploadEmpty( true );
            this.AssertEnquedPackagesEmpty( true );
        }
    }
}