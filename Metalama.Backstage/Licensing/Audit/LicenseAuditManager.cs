// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Utilities;
using System;

namespace Metalama.Backstage.Licensing.Audit;

internal class LicenseAuditManager
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
        void LogDisabledAudit( string reason )
        {
            this._logger.Info?.Log( $"License audit disabled because the license '{license.DisplayName}' {reason}." );
        }
        
        if ( !license.IsAuditable )
        {
            LogDisabledAudit( "is not auditable" );

            return;
        }

        if ( string.IsNullOrEmpty( license.LicenseString ) )
        {
            LogDisabledAudit( "has no license string" );

            return;
        }

        if ( this._applicationInfo.IsUnattendedProcess( this._loggerFactory ) )
        {
            this._logger.Info?.Log( "License audit disabled because the current process is unattended." );
            
            return;
        }
        
        var report = new LicenseAuditReport( this._serviceProvider, license.LicenseString! );

        if ( report.ReportedComponent.Version == null )
        {
            throw new InvalidOperationException( $"Version of '{report.ReportedComponent.Name}' application is unknown." );
        }

        if ( VersionExtensions.IsDevelopmentVersion( report.ReportedComponent.Version ) )
        {
            this._logger.Info?.Log(
                $"License audit disabled because the '{report.ReportedComponent.Name}' application version '{report.ReportedComponent.Version}' is a development version." );
        
            return;
        }

        bool HasBeenReportedRecently()
        {
            var configuration = this._configurationManager.Get<LicenseAuditConfiguration>();

            if ( configuration.LastAuditTimes.TryGetValue( report.AuditHashCode, out var lastReportTime )
                 && lastReportTime >= this._time.Now.AddDays( -1 ) )
            {
                LogDisabledAudit( "has been reported recently" );
                
                return true;
            }
            else
            {
                return false;
            }
        }

        if ( HasBeenReportedRecently() )
        {
            return;
        }

        if ( !MutexHelper.WithGlobalLock( $"LicenseAuditManager-{report.AuditHashCode}", TimeSpan.FromMilliseconds( 1 ), out var mutex ) )
        {
            LogDisabledAudit( "is just being audited by another audit manager" );

            return;
        }

        using ( mutex )
        {
            if ( HasBeenReportedRecently() )
            {
                return;
            }

            report.Flush();

            this._configurationManager.Update<LicenseAuditConfiguration>(
                c => c.LastAuditTimes = c.LastAuditTimes.SetItem( report.AuditHashCode, this._time.Now ) );
        }
    }
}