// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.UserInterface;
using System;
using System.IO;

namespace Metalama.Backstage.Telemetry;

internal class LocalExceptionReporter : IBackstageService
{
    private readonly IStandardDirectories _standardDirectories;
    private readonly IApplicationInfoProvider _applicationInfoProvider;
    private readonly IToastNotificationService? _toastNotificationService;
    private readonly ILogger _logger;

    public LocalExceptionReporter( IServiceProvider serviceProvider )
    {
        this._standardDirectories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this._applicationInfoProvider = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>();
        this._toastNotificationService = serviceProvider.GetBackstageService<IToastNotificationService>();
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( this.GetType().Name );
    }

    public void ReportException( Exception exception, string? localReportPath )
    {
        this._logger.Error?.Log( exception.ToString() );
        
        try
        {
            if ( localReportPath != null )
            {
                // If the caller did not create the report file, create it ourselves.
                localReportPath = Path.Combine( this._standardDirectories.CrashReportsDirectory, $"exception-{Guid.NewGuid()}.txt" );

                File.WriteAllText( localReportPath, exception.ToString() );
                
                this._logger.Info?.Log( $"Creating an exception report in '{localReportPath}'." );
            }

            this._toastNotificationService?.Show(
                new ToastNotification(
                    ToastNotificationKinds.Exception,
                    Text: $"The process {this._applicationInfoProvider.CurrentApplication.Name} encountered "
                          + $"an unexpected exception: {exception.Message}",
                    Uri: "file:///" + localReportPath ) );
        }
        catch ( Exception e )
        {
            this._logger.Error?.Log( e.ToString() );
        }
    }
}