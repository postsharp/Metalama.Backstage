// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Welcome;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Extensibility;

/// <summary>
/// Extension methods for setting up default services in an <see cref="ServiceProviderBuilder" />.
/// </summary>
public static class RegisterServiceExtensions
{
    public static ServiceProviderBuilder AddSingleton<T>(
        this ServiceProviderBuilder serviceProviderBuilder,
        T instance )
        where T : notnull
    {
        serviceProviderBuilder.AddService( typeof(T), instance );

        return serviceProviderBuilder;
    }

    /// <summary>
    /// Adds a service providing current date and time using <see cref="DateTime.Now" /> to the specified <see cref="ServiceProviderBuilder" />.
    /// </summary>
    /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
    /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
    private static ServiceProviderBuilder AddCurrentDateTimeProvider( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IDateTimeProvider>( new CurrentDateTimeProvider() );

    /// <summary>
    /// Adds a service providing access to file system using API in <see cref="System.IO" /> namespace to the specified <see cref="ServiceProviderBuilder" />.
    /// </summary>
    /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
    /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
    private static ServiceProviderBuilder AddFileSystem( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IFileSystem>( new FileSystem() );

    /// <summary>
    /// Adds a service providing paths of standard directories to the specified <see cref="ServiceProviderBuilder" />.
    /// </summary>
    /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
    /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
    internal static ServiceProviderBuilder AddStandardDirectories( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IStandardDirectories>( new StandardDirectories() );

    private static ServiceProviderBuilder AddDiagnostics(
        this ServiceProviderBuilder serviceProviderBuilder,
        ProcessKind processKind,
        string? projectName = null )
    {
        var serviceProvider = serviceProviderBuilder.ServiceProvider;
        var service = DiagnosticsService.GetInstance( serviceProvider, processKind, projectName );

        return serviceProviderBuilder.AddSingleton<ILoggerFactory>( service );
    }

    /// <summary>
    /// Adds the minimal set of services required to run the <see cref="DiagnosticsService"/>.
    /// </summary>
    internal static ServiceProviderBuilder AddDiagnosticServiceRequirements( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder
            .AddCurrentDateTimeProvider()
            .AddFileSystem()
            .AddStandardDirectories()
            .AddConfigurationManager();

    public static ServiceProviderBuilder AddConfigurationManager( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IConfigurationManager>( new ConfigurationManager( serviceProviderBuilder.ServiceProvider ) );

    /// <summary>
    /// Adds the minimal backstage services, without diagnostics and telemetry.
    /// </summary>
    public static ServiceProviderBuilder AddMinimalBackstageServices( this ServiceProviderBuilder serviceProviderBuilder )
    {
        return serviceProviderBuilder.AddDiagnosticServiceRequirements();
    }

    public static ServiceProviderBuilder AddLicensing(
        this ServiceProviderBuilder serviceProviderBuilder,
        bool ignoreUserProfileLicenses = false,
        string[]? additionalLicenses = null )
    {
        var licenseSources = new List<ILicenseSource>();
        var serviceProvider = serviceProviderBuilder.ServiceProvider;

        if ( !ignoreUserProfileLicenses )
        {
            licenseSources.Add( new UserProfileLicenseSource( serviceProvider ) );
            licenseSources.Add( new PreviewLicense( serviceProvider ) );
        }

        if ( additionalLicenses is { Length: > 0 } )
        {
            licenseSources.Add( new ExplicitLicenseSource( additionalLicenses, serviceProvider ) );
        }
        
        serviceProviderBuilder.AddSingleton<ILicenseConsumptionManager>(
            new LicenseConsumptionManager( serviceProvider, licenseSources ) );

        return serviceProviderBuilder;
    }

    public static ServiceProviderBuilder AddBackstageServices(
        this ServiceProviderBuilder serviceProviderBuilder,
        IApplicationInfo applicationInfo,
        string? projectName = null,
        bool ignoreUserProfileLicenses = false,
        string[]? additionalLicenses = null,
        bool addSupportServices = true )
    {
        // Add base services.
        serviceProviderBuilder = serviceProviderBuilder
            .AddSingleton( applicationInfo )
            .AddDiagnosticServiceRequirements();

        // Add diagnostics.
        if ( addSupportServices )
        {
            serviceProviderBuilder = serviceProviderBuilder.AddDiagnostics( applicationInfo.ProcessKind, projectName );

            // First-run configuration. This must be done before initializing licensing and telemetry.
            WelcomeService.Execute( serviceProviderBuilder.ServiceProvider, !ignoreUserProfileLicenses );
        }

        // Add licensing.
        serviceProviderBuilder.AddLicensing( ignoreUserProfileLicenses, additionalLicenses );

        if ( addSupportServices )
        {
            // Add telemetry.
            var uploadManager = new UploadManager( serviceProviderBuilder.ServiceProvider );

            serviceProviderBuilder = serviceProviderBuilder
                .AddSingleton<IExceptionReporter>( new ExceptionReporter( uploadManager, serviceProviderBuilder.ServiceProvider ) )
                .AddSingleton<IUsageReporter>( new UsageReporter( uploadManager, serviceProviderBuilder.ServiceProvider ) );
        }

        return serviceProviderBuilder;
    }
}