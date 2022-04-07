// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Telemetry;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Metalama.DotNetTools.Commands.Telemetry
{
    internal class UploadTelemetryCommand : CommandBase
    {
        public UploadTelemetryCommand( ICommandServiceProvider commandServiceProvider )
            : base( commandServiceProvider, "upload", "Uploads the telemetry" )
        {
            this.AddOption( new Option( new[] { "--async", "-a" }, "Run the upload asynchronously in a background process." ) );
            this.AddOption( new Option( new[] { "--force", "-f" }, "Force the upload even if another upload has been performed recently." ) );

            this.Handler = CommandHandler.Create<bool, bool, bool, IConsole>( this.ExecuteAsync );
        }

        private async Task<int> ExecuteAsync( bool async, bool force, bool verbose, IConsole console )
        {
            var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );

            var uploader = new TelemetryUploader( services );

            if ( async )
            {
                uploader.StartUpload( force );
            }
            else
            {
                await uploader.UploadAsync();
            }

            return 0;
        }
    }
}