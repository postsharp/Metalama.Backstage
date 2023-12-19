// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
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
    private readonly ILicenseRegistrationService? _licenseRegistrationService;
    private readonly IUserDeviceDetectionService _userDeviceDetectionService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIdeExtensionStatusService? _ideExtensionStatusService;

    protected UserInterfaceService( IServiceProvider serviceProvider )
    {
        this._processExecutor = serviceProvider.GetRequiredBackstageService<IProcessExecutor>();
        this.Logger = serviceProvider.GetLoggerFactory().GetLogger( this.GetType().Name );
        this._backstageToolExecutor = serviceProvider.GetRequiredBackstageService<IBackstageToolsExecutor>();
        this._canIgnoreRecoverableExceptions = serviceProvider.GetRequiredBackstageService<IRecoverableExceptionService>().CanIgnore;
        this._licenseRegistrationService = serviceProvider.GetBackstageService<ILicenseRegistrationService>();
        this._userDeviceDetectionService = serviceProvider.GetRequiredBackstageService<IUserDeviceDetectionService>();
        this._dateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._ideExtensionStatusService = serviceProvider.GetBackstageService<IIdeExtensionStatusService>();
    }

    public abstract void ShowToastNotification( ToastNotification notification, ref bool notificationReported );

    private string FormatExpiration( DateTime expiration )
    {
        var daysToExpiration = (int) Math.Floor( (expiration - this._dateTimeProvider.Now).TotalDays );

        return daysToExpiration switch
        {
            0 => "today",
            1 => "tomorrow",
            _ => $"in {daysToExpiration} days"
        };
    }

    private void ValidateRegisteredLicense( LicenseProperties? license, ref bool notificationReported )
    {
        if ( license == null )
        {
            this.ShowToastNotification( new ToastNotification( ToastNotificationKinds.RequiresLicense ), ref notificationReported );
        }
        else
        {
            if ( license is { ValidTo: not null }
                 && license.ValidTo.Value.Subtract( LicensingConstants.LicenseExpirationWarningPeriod ) < this._dateTimeProvider.Now )
            {
                if ( license.LicenseType == LicenseType.Evaluation )
                {
                    this.ShowToastNotification(
                        new ToastNotification(
                            ToastNotificationKinds.TrialExpiring,
                            $"Your Metalama trial expires {this.FormatExpiration( license.ValidTo.Value )}",
                            "Switch to Metalama [Free] or register a license key to avoid loosing functionality." ),
                        ref notificationReported );
                }
                else
                {
                    this.ShowToastNotification(
                        new ToastNotification(
                            ToastNotificationKinds.LicenseExpiring,
                            $"Your Metalama license expires {this.FormatExpiration( license.ValidTo.Value )}",
                            "Register a new license license key  to avoid loosing functionality." ),
                        ref notificationReported );
                }
            }
            else if ( license is { SubscriptionEndDate: not null }
                      && license.SubscriptionEndDate.Value.Subtract( LicensingConstants.SubscriptionExpirationWarningPeriod ) < this._dateTimeProvider.Now )
            {
                this.ShowToastNotification(
                    new ToastNotification(
                        ToastNotificationKinds.SubscriptionExpiring,
                        $"Your Metalama subscription expires {this.FormatExpiration( license.SubscriptionEndDate.Value )}",
                        "Renew your subscription and register a new license key to continue benefiting from updates." ),
                    ref notificationReported );
            }
        }
    }

    public virtual void Initialize()
    {
        var notificationReported = false;

        // Validate the current license.
        if ( this._userDeviceDetectionService.IsInteractiveDevice )
        {
            if ( this._licenseRegistrationService != null )
            {
                this.ValidateRegisteredLicense( this._licenseRegistrationService.RegisteredLicense, ref notificationReported );
            }

            if ( !notificationReported && this._ideExtensionStatusService?.ShouldRecommendToInstallVisualStudioExtension == true )
            {
                this.ShowToastNotification( new ToastNotification( ToastNotificationKinds.VsxNotInstalled ), ref notificationReported );
            }
        }
    }

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

        var processHasExited = false;
        using var webServerProcess = this._backstageToolExecutor.Start( BackstageTool.Worker, $"web --port {port} " );
        webServerProcess.Exited += () => processHasExited = true;

        // Wait until the server has started.
        var baseAddress = new Uri( $"https://localhost:{port}/" );
        Console.WriteLine( baseAddress );
        var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds( 1 ) };

        var stopwatch = Stopwatch.StartNew();

        this.Logger.Info?.Log( "Waiting for the HTTP server." );

        while ( true )
        {
            try
            {
                if ( processHasExited )
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