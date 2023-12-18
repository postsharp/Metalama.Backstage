// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
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

    public UserInterfaceService( IServiceProvider serviceProvider )
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

    protected abstract void Notify( ToastNotificationKind kind, ref bool notificationReported );

    private void ValidateRegisteredLicense( LicenseProperties? license, ref bool notificationReported )
    {
        if ( license == null )
        {
            this.Notify( ToastNotificationKinds.RequiresLicense, ref notificationReported );
        }
        else
        {
            if ( license is { ValidTo: not null }
                 && license.ValidTo.Value.Subtract( LicensingConstants.LicenseExpirationWarningPeriod ) < this._dateTimeProvider.Now )
            {
                this.Notify(
                    license.LicenseType == LicenseType.Evaluation ? ToastNotificationKinds.TrialExpiring : ToastNotificationKinds.LicenseExpiring,
                    ref notificationReported );
            }
            else if ( license is { SubscriptionEndDate: not null }
                      && license.SubscriptionEndDate.Value.Subtract( LicensingConstants.SubscriptionExpirationWarningPeriod ) < this._dateTimeProvider.Now )
            {
                this.Notify( ToastNotificationKinds.SubscriptionExpiring, ref notificationReported );
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
                this.Notify( ToastNotificationKinds.VsxNotInstalled, ref notificationReported );
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

        this._backstageToolExecutor.Start( BackstageTool.Worker, $"web --port {port} " );

        // Wait until the server has started.
        var baseAddress = new Uri( $"https://localhost:{port}/" );
        Console.WriteLine( baseAddress );
        var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds( 1 ) };

        var stopwatch = Stopwatch.StartNew();

        while ( !(await httpClient.GetAsync( baseAddress )).IsSuccessStatusCode )
        {
            this.Logger.Info?.Log( "Waiting for the HTTP server." );

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