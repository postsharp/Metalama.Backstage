// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Utilities;
using Metalama.Backstage.Welcome;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Extensibility;

/// <summary>
/// Extension methods for setting up default services in an <see cref="ServiceProviderBuilder" />.
/// </summary>
public static class RegisterServiceExtensions
{
    public static ServiceProviderBuilder AddUntypedSingleton<T>(
        this ServiceProviderBuilder serviceProviderBuilder,
        T instance )
        where T : notnull
    {
        serviceProviderBuilder.AddService( typeof(T), instance );

        return serviceProviderBuilder;
    }

    public static ServiceProviderBuilder AddSingleton<T>(
        this ServiceProviderBuilder serviceProviderBuilder,
        T instance )
        where T : IBackstageService
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

        var configuration = serviceProviderBuilder.ServiceProvider.GetDiagnosticsConfiguration();

        DebuggerHelper.Launch( configuration, processKind );

        var applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
        var loggerFactory = new LoggerFactory( serviceProvider, configuration, applicationInfo.ProcessKind, projectName );

        return serviceProviderBuilder.AddSingleton<ILoggerFactory>( loggerFactory );
    }

    /// <summary>
    /// Adds the minimal set of services required by logging and telemetry.
    /// </summary>
    private static ServiceProviderBuilder AddDiagnosticsRequirements(
        this ServiceProviderBuilder serviceProviderBuilder,
        IApplicationInfo applicationInfo,
        string? dotNetSdkDirectory )
        => serviceProviderBuilder
            .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( applicationInfo ) )
            .AddCurrentDateTimeProvider()
            .AddFileSystem()
            .AddStandardDirectories()
            .AddConfigurationManager()
            .AddPlatformInfo( dotNetSdkDirectory );

    public static ServiceProviderBuilder AddConfigurationManager( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IConfigurationManager>( new ConfigurationManager( serviceProviderBuilder.ServiceProvider ) );

    private static ServiceProviderBuilder AddPlatformInfo(
        this ServiceProviderBuilder serviceProviderBuilder,
        string? dotnetSdkDirectory )
    {
        return serviceProviderBuilder.AddSingleton<IPlatformInfo>( new PlatformInfo( dotnetSdkDirectory ) );
    }

    private static DiagnosticsConfiguration GetDiagnosticsConfiguration( this IServiceProvider serviceProvider )
        => serviceProvider.GetRequiredBackstageService<IConfigurationManager>().Get<DiagnosticsConfiguration>();

    /// <summary>
    /// Adds the minimal backstage services, without diagnostics and telemetry.
    /// </summary>
    public static ServiceProviderBuilder AddMinimalBackstageServices(
        this ServiceProviderBuilder serviceProviderBuilder,
        IApplicationInfo applicationInfo,
        bool addSupportServices = false,
        string? projectName = null,
        string? dotnetSdkDirectory = null )
    {
        serviceProviderBuilder = serviceProviderBuilder
            .AddDiagnosticsRequirements( applicationInfo, dotnetSdkDirectory );

        if ( addSupportServices )
        {
            serviceProviderBuilder = serviceProviderBuilder
                .AddDiagnostics( applicationInfo.ProcessKind, projectName )
                .AddTelemetryServices();
        }

        return serviceProviderBuilder;
    }

    public static ServiceProviderBuilder AddLicensing(
        this ServiceProviderBuilder serviceProviderBuilder,
        bool considerUnattendedLicense = false,
        bool ignoreUserProfileLicenses = false,
        string[]? additionalLicenses = null )
    {
        var licenseSources = new List<ILicenseSource>();
        var serviceProvider = serviceProviderBuilder.ServiceProvider;

        if ( considerUnattendedLicense )
        {
            licenseSources.Add( new UnattendedLicenseSource( serviceProvider ) );
        }

        if ( !ignoreUserProfileLicenses )
        {
            licenseSources.Add( new UserProfileLicenseSource( serviceProvider ) );
        }

        if ( additionalLicenses is { Length: > 0 } )
        {
            licenseSources.Add( new ExplicitLicenseSource( additionalLicenses, serviceProvider ) );
        }

        if ( !ignoreUserProfileLicenses )
        {
            // Must be added last.
            licenseSources.Add( new PreviewLicenseSource( serviceProvider ) );
        }

        serviceProviderBuilder.AddSingleton<ILicenseConsumptionManager>( new LicenseConsumptionManager( serviceProvider, licenseSources ) );

        return serviceProviderBuilder;
    }

    public static ServiceProviderBuilder AddBackstageServices(
        this ServiceProviderBuilder serviceProviderBuilder,
        IApplicationInfo applicationInfo,
        string? projectName = null,
        bool considerUnattendedProcessLicense = false,
        bool ignoreUserProfileLicenses = false,
        string[]? additionalLicenses = null,
        string? dotNetSdkDirectory = null,
        bool openWelcomePage = false,
        bool addLicensing = true,
        bool addSupportServices = true )
    {
        // Add base services.
        serviceProviderBuilder = serviceProviderBuilder
            .AddDiagnosticsRequirements( applicationInfo, dotNetSdkDirectory );

        // Add temporary files.
        serviceProviderBuilder.AddService( typeof(ITempFileManager), new TempFileManager( serviceProviderBuilder.ServiceProvider ) );

        // Add diagnostics.
        if ( addSupportServices )
        {
            serviceProviderBuilder = serviceProviderBuilder
                .AddDiagnostics( applicationInfo.ProcessKind, projectName );

            var serviceProvider = serviceProviderBuilder.ServiceProvider;

            // First-run configuration. This must be done before initializing licensing and telemetry.
            var registerEvaluationLicense = !ignoreUserProfileLicenses && !applicationInfo.IsPrerelease
                                                                       && !applicationInfo.IsUnattendedProcess( serviceProvider.GetLoggerFactory() );

            var welcomeService = new WelcomeService( serviceProvider );
            welcomeService.ExecuteFirstStartSetup( registerEvaluationLicense );

            if ( openWelcomePage )
            {
                welcomeService.OpenWelcomePage();
            }
        }

        // Add file locking detection.
        if ( LockingProcessDetector.IsSupported )
        {
            serviceProviderBuilder.AddService( typeof(ILockingProcessDetector), new LockingProcessDetector() );
        }

        // Add mini-dump service.
        if ( MiniDumper.IsSupported )
        {
            serviceProviderBuilder.AddService( typeof(IMiniDumper), new MiniDumper( serviceProviderBuilder.ServiceProvider ) );
        }

        // Add licensing.
        if ( addLicensing )
        {
            serviceProviderBuilder.AddLicensing( considerUnattendedProcessLicense, ignoreUserProfileLicenses, additionalLicenses );
        }

        if ( addSupportServices )
        {
            serviceProviderBuilder = serviceProviderBuilder.AddTelemetryServices();
        }

        return serviceProviderBuilder;
    }

    public static ServiceProviderBuilder AddTelemetryServices( this ServiceProviderBuilder serviceProviderBuilder )
    {
        var serviceProvider = serviceProviderBuilder.ServiceProvider;

        // Add telemetry.
        var queue = new TelemetryQueue( serviceProviderBuilder.ServiceProvider );
        var uploader = new TelemetryUploader( serviceProviderBuilder.ServiceProvider );

        serviceProviderBuilder = serviceProviderBuilder
            .AddSingleton<IExceptionReporter>( new ExceptionReporter( queue, serviceProvider ) )
            .AddSingleton<IUsageReporter>( new UsageReporter( uploader, serviceProvider ) );

        return serviceProviderBuilder;
    }
}