// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using System;

namespace Metalama.Backstage.Licensing.Audit;

internal class LicenseAuditManager : ILicenseAuditManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationManager _configurationManager;
    private readonly IApplicationInfo _applicationInfo;
    private readonly IDateTimeProvider _time;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public LicenseAuditManager( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
        this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._loggerFactory = serviceProvider.GetLoggerFactory();
        this._logger = this._loggerFactory.Licensing();
    }

    public void ReportLicense( LicenseConsumptionData license )
    {
        if ( !license.IsAuditable )
        {
            this._logger.Trace?.Log( $"License audit disabled because the license '{license.DisplayName}' is not auditable." );

            return;
        }

        if ( string.IsNullOrEmpty( license.LicenseString ) )
        {
            this._logger.Trace?.Log( $"License audit disabled because the license string is empty." );

            return;
        }

        if ( this._applicationInfo.IsUnattendedProcess( this._loggerFactory ) )
        {
            this._logger.Trace?.Log( "License audit disabled because the current process is unattended." );

            return;
        }

        if ( !this._applicationInfo.IsTelemetryEnabled )
        {
            this._logger.Trace?.Log( $"License audit disabled because telemetry is disabled for the current build." );

            return;
        }

        var report = new LicenseAuditReport( this._serviceProvider, license.LicenseString! );

        if ( report.ReportedComponent.Version == null )
        {
            throw new InvalidOperationException( $"Version of '{report.ReportedComponent.Name}' application is unknown." );
        }

        var updated = this._configurationManager.UpdateIf<LicenseAuditConfiguration>(
            c => !c.LastAuditTimes.TryGetValue( report.AuditHashCode, out var lastReportTime )
                 || lastReportTime <= this._time.Now.AddDays( -1 ),
            c => c with { LastAuditTimes = c.LastAuditTimes.SetItem( report.AuditHashCode, this._time.Now ) } );

        if ( !updated )
        {
            this._logger.Trace?.Log( $"License audit disabled because the license '{license.DisplayName}' has been recently audited." );
        }
        else
        {
            this._logger.Trace?.Log( $"Uploading license audit report." );
            report.Upload();
        }
    }
}