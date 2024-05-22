// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Telemetry;
using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace Metalama.Backstage.Worker.Upload
{
    internal class UploadCommand : AsyncCommand<UploadCommandSettings>
    {
        public override async Task<int> ExecuteAsync( CommandContext context, UploadCommandSettings settings )
        {
            var appData = (AppData) context.Data!;
            var serviceProvider = appData.ServiceProvider;

            var logger = serviceProvider.GetLoggerFactory().GetLogger( "Worker" );
            var usageReporter = serviceProvider.GetBackstageService<IUsageReporter>();

            try
            {
                logger.Trace?.Log( "Job started." );
                usageReporter?.StartSession( "CompilerUsage" );

                // Clean-up. Scheduled automatically by telemetry.
                logger.Trace?.Log( "Starting temporary directories cleanup." );
                var tempFileManager = new TempFileManager( serviceProvider );
                tempFileManager.CleanTempDirectories();

                // Telemetry.
                logger.Trace?.Log( "Starting telemetry upload." );
                var uploader = serviceProvider.GetRequiredBackstageService<ITelemetryUploader>();
                await uploader.UploadAsync();

                logger.Trace?.Log( "Job done." );

                return 0;
            }
            finally
            {
                // Report usage.
                usageReporter?.StopSession();
            }
        }
    }
}