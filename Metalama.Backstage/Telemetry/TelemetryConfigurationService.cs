// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Telemetry;

internal class TelemetryConfigurationService : ITelemetryConfigurationService
{
    private readonly Lazy<bool> _isEnabled;

    public bool IsEnabled => this._isEnabled.Value;
    
    public TelemetryConfigurationService( IServiceProvider serviceProvider )
    {
        this._isEnabled = new Lazy<bool>(
            () =>
            {
                var loggerFactory = serviceProvider.GetLoggerFactory();
                var logger = loggerFactory.Telemetry();

                var applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
                var isApplicationTelemetryEnabled = applicationInfo.IsTelemetryEnabled;

                if ( !isApplicationTelemetryEnabled )
                {
                    logger.Trace?.Log( $"Telemetry is disabled for '{applicationInfo.Name} {applicationInfo.Version}'." );

                    return false;
                }

                var telemetryOptOutEnvironmentVariableValue = serviceProvider.GetRequiredBackstageService<IEnvironmentVariableProvider>()
                    .GetEnvironmentVariable( "METALAMA_TELEMETRY_OPT_OUT" );

                var isTelemetryOptedOut = !string.IsNullOrEmpty( telemetryOptOutEnvironmentVariableValue );

                if ( isTelemetryOptedOut )
                {
                    logger.Trace?.Log( $"Telemetry is disabled by the opt-out environment variable." );

                    return false;
                }

                if ( applicationInfo.IsUnattendedProcess( loggerFactory ) )
                {
                    logger.Trace?.Log( $"Telemetry is disabled because the current process is unattended." );
                    
                    return false;
                }

                return true;
            } );
    }
}