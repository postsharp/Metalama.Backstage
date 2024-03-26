// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Telemetry;

internal class TelemetryConfigurationService : ITelemetryConfigurationService
{
    private readonly IConfigurationManager _configurationManager;
    private readonly Guid _newDeviceId;

    internal TelemetryConfigurationService( IServiceProvider serviceProvider, Guid? newDeviceId = null )
        : this( serviceProvider, newDeviceId ?? Guid.NewGuid() ) { }

    // Tests use this constructor to supply a constant Guid.
    internal TelemetryConfigurationService( IServiceProvider serviceProvider, Guid newDeviceId )
    {
        this._newDeviceId = newDeviceId;
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
    }

    public void SetStatus( bool? enabled )
    {
        var reportAction = enabled switch
        {
            null => ReportingAction.Ask,
            true => ReportingAction.Yes,
            false => ReportingAction.No
        };

        this._configurationManager.Update<TelemetryConfiguration>(
            c => c with { UsageReportingAction = reportAction, ExceptionReportingAction = reportAction, PerformanceProblemReportingAction = reportAction } );
    }

    public Guid DeviceId
    {
        get
        {
            var configuration = this._configurationManager.Get<TelemetryConfiguration>();

            if ( configuration.DeviceId == null )
            {
                this._configurationManager.UpdateIf<TelemetryConfiguration>( c => c.DeviceId == null, c => c with { DeviceId = this._newDeviceId } );
                configuration = this._configurationManager.Get<TelemetryConfiguration>();
            }

            return configuration.DeviceId!.Value;
        }
    }

    public void ResetDeviceId()
    {
        this._configurationManager.Update<TelemetryConfiguration>( c => c with { DeviceId = Guid.NewGuid() } );
    }
}