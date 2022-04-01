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
            this.Handler = CommandHandler.Create<bool, IConsole>( this.ExecuteAsync );
        }

        private async Task<int> ExecuteAsync( bool verbose, IConsole console )
        {
            var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );

            var uploader = new TelemetryUploader( services );

            await uploader.UploadAsync();

            return 0;
        }
    }
}