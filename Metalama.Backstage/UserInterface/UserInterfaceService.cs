// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Tools;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Metalama.Backstage.UserInterface;

public abstract class UserInterfaceService : IUserInterfaceService
{
    private readonly IProcessExecutor _processExecutor;

    protected ILogger Logger { get; }

    private readonly IBackstageToolsExecutor _backstageToolExecutor;
    private readonly bool _canIgnoreRecoverableExceptions;

    protected UserInterfaceService( IServiceProvider serviceProvider )
    {
        this._processExecutor = serviceProvider.GetRequiredBackstageService<IProcessExecutor>();
        this.Logger = serviceProvider.GetLoggerFactory().GetLogger( this.GetType().Name );
        this._backstageToolExecutor = serviceProvider.GetRequiredBackstageService<IBackstageToolsExecutor>();
        this._canIgnoreRecoverableExceptions = serviceProvider.GetRequiredBackstageService<IRecoverableExceptionService>().CanIgnore;
    }

    public abstract void ShowToastNotification( ToastNotification notification, ref bool notificationReported );

    protected virtual ProcessStartInfo GetProcessStartInfoForUrl( string url, BrowserMode browserMode ) => new( url ) { UseShellExecute = true };

    public void OpenExternalWebPage( string url, BrowserMode browserMode )
    {
        try
        {
            this.Logger.Trace?.Log( $"Opening '{url}'." );

            this._processExecutor.Start( this.GetProcessStartInfoForUrl( url, browserMode ) );
        }
        catch ( Exception e )
        {
            try
            {
                this.Logger.Error?.Log( $"Cannot start the welcome web page: {e.Message}" );
            }
            catch when ( this._canIgnoreRecoverableExceptions ) { }

            if ( !this._canIgnoreRecoverableExceptions )
            {
                throw;
            }
        }
    }

    public async Task OpenConfigurationWebPageAsync( string path )
    {
        // TODO: Find a free port. 
        const int port = 5252;

        using var webServerProcess = this._backstageToolExecutor.Start( BackstageTool.Worker, $"web --port {port} " );

        // Wait until the server has started.
        var baseAddress = new Uri( $"http://localhost:{port}/" );

        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = ( _, _, _, _ ) => true;
        var httpClient = new HttpClient( handler ) { Timeout = TimeSpan.FromSeconds( 1 ) };

        var stopwatch = Stopwatch.StartNew();

        this.Logger.Info?.Log( "Waiting for the HTTP server." );

        while ( true )
        {
            try
            {
                if ( webServerProcess.HasExited )
                {
                    this.Logger.Error?.Log( "The server process has exited prematurely." );

                    return;
                }

                var response = await httpClient.GetAsync( baseAddress );

                if ( response.IsSuccessStatusCode )
                {
                    break;
                }
            }
            catch ( TaskCanceledException )
            {
                // This happens because of the timeout.
            }
            catch ( HttpRequestException e )
            {
                this.Logger.Warning?.Log( e.Message );
            }

            if ( stopwatch.Elapsed.TotalSeconds > 30 )
            {
                this.Logger.Error?.Log( $"Timeout while waiting for {baseAddress}." );

                return;
            }
        }

        var url = new Uri( baseAddress, path ).ToString();

        this.OpenExternalWebPage( url, BrowserMode.Application );
    }
}