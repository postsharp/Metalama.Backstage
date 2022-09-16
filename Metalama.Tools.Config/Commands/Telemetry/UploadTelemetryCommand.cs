﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Metalama.DotNetTools.Commands.Telemetry
{
    internal class UploadTelemetryCommand : CommandBase
    {
        public UploadTelemetryCommand( ICommandServiceProviderProvider commandServiceProvider )
            : base( commandServiceProvider, "upload", "Uploads the telemetry" )
        {
            this.AddOption( new Option( new[] { "--async", "-a" }, "Run the upload asynchronously in a background process." ) );
            this.AddOption( new Option( new[] { "--force", "-f" }, "Force the upload even if another upload has been performed recently." ) );

            this.Handler = CommandHandler.Create<bool, bool, bool, IConsole>( this.ExecuteAsync );
        }

        private async Task<int> ExecuteAsync( bool async, bool force, bool verbose, IConsole console )
        {
            this.CommandServices.Initialize( console, verbose );

            var uploader = this.CommandServices.ServiceProvider.GetRequiredBackstageService<ITelemetryUploader>();

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