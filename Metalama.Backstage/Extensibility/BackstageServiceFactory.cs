﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption;
using System;

namespace Metalama.Backstage.Extensibility;

public static class BackstageServiceFactory
{
    private static readonly object _initializeSync = new();

    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider ServiceProvider
        => _serviceProvider ?? throw new InvalidOperationException( "BackstageServiceFactory has not been initialized." );

    public static bool Initialize( Func<IApplicationInfo> applicationInfoFactory, string caller, string? projectName = null )
    {
        lock ( _initializeSync )
        {
            if ( _serviceProvider != null )
            {
                _serviceProvider.GetLoggerFactory()
                    .GetLogger( "BackstageServiceFactory" )
                    .Trace?.Log( $"Support services initialization requested from {caller}. The services are initialized already." );

                return false;
            }

            _serviceProvider = new ServiceProviderBuilder()
                .AddBackstageServices( applicationInfo: applicationInfoFactory(), projectName: projectName, addLicenseConsumption: false )
                .ServiceProvider;

            _serviceProvider.GetLoggerFactory()
                .GetLogger( "BackstageServiceFactory" )
                .Trace?.Log( $"Support services initialized upon a request from {caller}." );

            return true;
        }
    }

    public static ILicenseConsumptionManager CreateLicenseConsumptionManager(
        bool considerUnattendedLicense = false,
        bool ignoreUserProfileLicenses = false,
        string? projectLicense = null )
        => LicenseConsumptionManagerFactory.Create( ServiceProvider, considerUnattendedLicense, ignoreUserProfileLicenses, projectLicense );
}