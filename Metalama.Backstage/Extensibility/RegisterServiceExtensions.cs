// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Audit;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Utilities;
using Metalama.Backstage.Welcome;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace Metalama.Backstage.Extensibility;

/// <summary>
/// Extension methods for setting up default services in an <see cref="ServiceProviderBuilder" />.
/// </summary>
public static class RegisterServiceExtensions
{
    internal static ServiceProviderBuilder AddSingleton<T>(
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
    /// Adds a service providing access to environment using API in <see cref="System" /> namespace to the specified <see cref="ServiceProviderBuilder" />.
    /// </summary>
    /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
    /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
    private static ServiceProviderBuilder AddEnvironmentVariableProvider( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IEnvironmentVariableProvider>( new EnvironmentVariableProvider() );

    /// <summary>
    /// Adds a service providing information if a recoverable exception can be ignored.
    /// </summary>
    /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
    /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
    private static ServiceProviderBuilder AddRecoverableExceptionService( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IRecoverableExceptionService>( new RecoverableExceptionService( serviceProviderBuilder.ServiceProvider ) );

    /// <summary>
    /// Adds a service providing paths of standard directories to the specified <see cref="ServiceProviderBuilder" />.
    /// </summary>
    /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
    /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
    internal static ServiceProviderBuilder AddStandardDirectories( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IStandardDirectories>( new StandardDirectories() );

    internal static ServiceProviderBuilder AddDiagnostics(
        this ServiceProviderBuilder serviceProviderBuilder,
        ProcessKind processKind,
        string? projectName = null )
    {
        var serviceProvider = serviceProviderBuilder.ServiceProvider;

        var dateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();

        var configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        var configuration = configurationManager.Get<DiagnosticsConfiguration>();

        DebuggerHelper.Launch( configuration, processKind );

        // Automatically stop logging after a while.
        if ( configuration.LastModified != null &&
             configuration.LastModified < dateTimeProvider.Now.AddHours( -configuration.Logging.StopLoggingAfterHours ) )
        {
            configurationManager.UpdateIf<DiagnosticsConfiguration>(
                c => c.Logging.Processes.Any( p => p.Value ),
                c => c with { Logging = c.Logging with { Processes = c.Logging.Processes.ToImmutableDictionary( x => x.Key, _ => false ) } } );

            configuration = configurationManager.Get<DiagnosticsConfiguration>();
        }

        var applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
        var loggerFactory = new LoggerFactory( serviceProvider, configuration, applicationInfo.ProcessKind, projectName );

        serviceProviderBuilder.AddSingleton<ILoggerFactory>( loggerFactory );

        (configurationManager as ConfigurationManager)?.SetLoggerFactory( loggerFactory );

        new ProfilingService( serviceProviderBuilder.ServiceProvider, processKind ).Initialize();

        return serviceProviderBuilder;
    }

    /// <summary>
    /// Adds the minimal set of services required by logging and telemetry.
    /// </summary>
    private static ServiceProviderBuilder AddDiagnosticsRequirements(
        this ServiceProviderBuilder serviceProviderBuilder,
        IApplicationInfo applicationInfo )
    {
        serviceProviderBuilder = serviceProviderBuilder
            .AddEnvironmentVariableProvider()
            .AddRecoverableExceptionService()
            .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( applicationInfo ) )
            .AddSingleton<IUserInteractionService>( new UserInteractionService() )
            .AddCurrentDateTimeProvider()
            .AddFileSystem()
            .AddStandardDirectories()
            .AddSingleton<IProcessExecutor>( new ProcessExecutor() )
            .AddSingleton<IHttpClientFactory>( new HttpClientFactory() )
            .AddConfigurationManager();

        serviceProviderBuilder.AddSingleton<ITempFileManager>( new TempFileManager( serviceProviderBuilder.ServiceProvider ) );

        return serviceProviderBuilder;
    }

    internal static ServiceProviderBuilder AddConfigurationManager( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IConfigurationManager>( new ConfigurationManager( serviceProviderBuilder.ServiceProvider ) );

    private static void AddPlatformInfo(
        this ServiceProviderBuilder serviceProviderBuilder,
        string? dotnetSdkDirectory )
    {
        serviceProviderBuilder.AddSingleton<IPlatformInfo>( new PlatformInfo( serviceProviderBuilder.ServiceProvider, dotnetSdkDirectory ) );
    }

    private static void AddLicensing(
        this ServiceProviderBuilder serviceProviderBuilder,
        LicensingInitializationOptions options )
    {
        if ( !options.DisableLicenseAudit )
        {
            serviceProviderBuilder.AddSingleton<ILicenseAuditManager>( new LicenseAuditManager( serviceProviderBuilder.ServiceProvider ) );
        }

        var licenseConsumptionManager = LicenseConsumptionServiceFactory.Create(
            serviceProviderBuilder.ServiceProvider,
            options );

        serviceProviderBuilder.AddSingleton( licenseConsumptionManager );
    }

    public static ServiceProviderBuilder AddBackstageServices(
        this ServiceProviderBuilder serviceProviderBuilder,
        BackstageInitializationOptions options )
    {
        // Add base services.
        var applicationInfo = options.ApplicationInfo;

        serviceProviderBuilder = serviceProviderBuilder
            .AddDiagnosticsRequirements( applicationInfo );

        // Add diagnostics.
        if ( options.AddSupportServices )
        {
            if ( options.AddLoggerFactoryAction == null )
            {
                serviceProviderBuilder = serviceProviderBuilder
                    .AddDiagnostics( applicationInfo.ProcessKind, options.ProjectName );

                var serviceProvider = serviceProviderBuilder.ServiceProvider;

                // First-run configuration. This must be done before initializing licensing and telemetry.
                var welcomeService = new WelcomeService( serviceProvider );
                welcomeService.ExecuteFirstStartSetup( options );
            }
            else
            {
                options.AddLoggerFactoryAction( serviceProviderBuilder );

                var configurationManager = serviceProviderBuilder.ServiceProvider.GetBackstageService<IConfigurationManager>();

                (configurationManager as ConfigurationManager)?.SetLoggerFactory(
                    serviceProviderBuilder.ServiceProvider.GetRequiredBackstageService<ILoggerFactory>() );
            }
        }

        // Add platform info.
        serviceProviderBuilder.AddPlatformInfo( options.DotNetSdkDirectory );

        // Add file locking detection.
        if ( LockingProcessDetector.IsSupported )
        {
            serviceProviderBuilder.AddService( typeof(ILockingProcessDetector), new LockingProcessDetector() );
        }

        // Add support services.
        if ( options.AddSupportServices )
        {
            serviceProviderBuilder.AddService( typeof(IMiniDumper), new MiniDumper( serviceProviderBuilder.ServiceProvider ) );

            serviceProviderBuilder.AddTelemetryServices();
        }

        // Add process management service.
        serviceProviderBuilder.TryAddProcessManagerService();

        // Add licensing.
        if ( options.AddLicensing )
        {
            if ( !options.LicensingOptions.DisableLicenseAudit && !options.AddSupportServices )
            {
                throw new InvalidOperationException( "License audit requires support services." );
            }

            serviceProviderBuilder.AddLicensing( options.LicensingOptions );
        }

        return serviceProviderBuilder;
    }

    internal static void AddTelemetryServices( this ServiceProviderBuilder serviceProviderBuilder )
    {
        // Add telemetry.
        var queue = new TelemetryQueue( serviceProviderBuilder.ServiceProvider );

        serviceProviderBuilder
            .AddSingleton<IExceptionReporter>( new ExceptionReporter( queue, serviceProviderBuilder.ServiceProvider ) )
            .AddSingleton<ITelemetryUploader>( new TelemetryUploader( serviceProviderBuilder.ServiceProvider ) )
            .AddSingleton<IUsageReporter>( new UsageReporter( serviceProviderBuilder.ServiceProvider ) );
    }

    private static void TryAddProcessManagerService( this ServiceProviderBuilder serviceProviderBuilder )
    {
        if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
        {
            serviceProviderBuilder.AddSingleton<IProcessManager>( new WindowsProcessManager( serviceProviderBuilder.ServiceProvider ) );
        }
        else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
        {
            serviceProviderBuilder.AddSingleton<IProcessManager>( new LinuxProcessManager( serviceProviderBuilder.ServiceProvider ) );
        }
        else if ( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
        {
            serviceProviderBuilder.AddSingleton<IProcessManager>( new MacProcessManager( serviceProviderBuilder.ServiceProvider ) );
        }
        else
        {
            // Not supported.
        }
    }
}