// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Diagnostics;

public static class DiagnosticsService
{
    private static readonly object _initializeSync = new();

    private static bool _initialized;

    public static IServiceProvider ServiceProvider { get; private set; } = new ServiceProviderBuilder().ServiceProvider;

    public static bool Initialize( Func<IApplicationInfo> applicationInfoFactory, string caller, string? projectName = null )
    {
        static ILogger GetLogger() => GetRequiredService<ILoggerFactory>().Telemetry();

        lock ( _initializeSync )
        {
            if ( _initialized )
            {
                GetLogger().Trace?.Log( $"Support services initialization requested from {caller}. The services are initialized already." );

                return false;
            }

            ServiceProvider = new ServiceProviderBuilder()
                .AddBackstageServices( applicationInfo: applicationInfoFactory(), projectName: projectName, addLicensing: false )
                .ServiceProvider;

            GetLogger().Trace?.Log( $"Support services initialized upon a request from {caller}." );

            _initialized = true;

            return true;
        }
    }

    public static T? GetOptionalService<T>() => (T) ServiceProvider.GetService( typeof(T) );

    public static T GetRequiredService<T>()
    {
        var service = (T) ServiceProvider.GetService( typeof(T) );

        if ( service == null )
        {
            throw new InvalidOperationException( $"Failed to get service of type {typeof(T)}." );
        }

        return service;
    }
}