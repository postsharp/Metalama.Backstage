// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Audit;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Tools;
using Metalama.Backstage.UserInterface;
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

    internal static ServiceProviderBuilder AddSingleton<T>(
        this ServiceProviderBuilder serviceProviderBuilder,
        Func<IServiceProvider, T> func )
        where T : IBackstageService
    {
        serviceProviderBuilder.AddService( typeof(T), serviceProvider => func( serviceProvider ) );

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
        => serviceProviderBuilder.AddSingleton<IRecoverableExceptionService>( serviceProvider => new RecoverableExceptionService( serviceProvider ) );

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
        serviceProviderBuilder.AddSingleton<ILoggerFactory>(
            serviceProvider =>
            {
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

                serviceProvider.GetBackstageService<EarlyLoggerFactory>()?.Replace( loggerFactory );

                return loggerFactory;
            } );

        serviceProviderBuilder.AddSingleton( serviceProvider => new ProfilingService( serviceProvider ) );

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
            .AddSingleton<IUserDeviceDetectionService>( serviceProvider => new WindowsUserDeviceDetectionService( serviceProvider ) )
            .AddCurrentDateTimeProvider()
            .AddFileSystem()
            .AddStandardDirectories()
            .AddSingleton<IProcessExecutor>( new ProcessExecutor() )
            .AddSingleton<IHttpClientFactory>( new HttpClientFactory() )
            .AddConfigurationManager();

        serviceProviderBuilder.AddSingleton<ITempFileManager>( serviceProvider => new TempFileManager( serviceProvider ) );

        return serviceProviderBuilder;
    }

    internal static ServiceProviderBuilder AddConfigurationManager( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IConfigurationManager>( serviceProvider => new ConfigurationManager( serviceProvider ) );

    private static void AddPlatformInfo(
        this ServiceProviderBuilder serviceProviderBuilder,
        string? dotnetSdkDirectory )
    {
        serviceProviderBuilder.AddSingleton<IPlatformInfo>( serviceProvider => new PlatformInfo( serviceProvider, dotnetSdkDirectory ) );
    }

    private static void AddLicensing(
        this ServiceProviderBuilder serviceProviderBuilder,
        LicensingInitializationOptions options )
    {
        if ( !options.DisableLicenseAudit )
        {
            serviceProviderBuilder.AddSingleton<ILicenseAuditManager>( serviceProvider => new LicenseAuditManager( serviceProvider ) );
        }

        serviceProviderBuilder.AddSingleton( serviceProvider => LicenseConsumptionServiceFactory.Create( serviceProvider, options ) );
        serviceProviderBuilder.AddSingleton<ILicenseRegistrationService>( serviceProvider => new LicenseRegistrationService( serviceProvider ) );
    }

    public static ServiceProviderBuilder AddBackstageServices( this ServiceProviderBuilder serviceProviderBuilder, BackstageInitializationOptions options )
    {
        // Add base services.
        var applicationInfo = options.ApplicationInfo;

        serviceProviderBuilder.AddSingleton( new BackstageInitializationOptionsProvider( options ) );
        serviceProviderBuilder.AddSingleton( new EarlyLoggerFactory() );

        serviceProviderBuilder = serviceProviderBuilder
            .AddDiagnosticsRequirements( applicationInfo );

        // Add diagnostics.
        if ( options.AddSupportServices )
        {
            if ( options.CreateLoggingFactory == null )
            {
                serviceProviderBuilder = serviceProviderBuilder
                    .AddDiagnostics( applicationInfo.ProcessKind, options.ProjectName );
            }
            else
            {
                serviceProviderBuilder.AddSingleton<ILoggerFactory>(
                    serviceProvider =>
                    {
                        var loggerFactory = options.CreateLoggingFactory( serviceProvider );
                        serviceProvider.GetBackstageService<EarlyLoggerFactory>()?.Replace( loggerFactory );

                        return loggerFactory;
                    } );
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
            serviceProviderBuilder.AddService( typeof(IMiniDumper), serviceProvider => new MiniDumper( serviceProvider ) );
            serviceProviderBuilder.AddTelemetryServices();
        }

        // Add tools.
        if ( options.AddSupportServices || options.AddUserInterface )
        {
            if ( options.IsDevelopmentEnvironment )
            {
                serviceProviderBuilder.AddService( typeof(IBackstageToolsLocator), serviceProvider => new DevBackstageToolsLocator() );
            }
            else
            {
                serviceProviderBuilder.AddService( typeof(IBackstageToolsLocator), serviceProvider => new BackstageToolsLocator( serviceProvider ) );
            }

            serviceProviderBuilder.AddService( typeof(IBackstageToolsExecutor), serviceProvider => new BackstageToolsExecutor( serviceProvider ) );
            options.AddToolsExtractor?.Invoke( serviceProviderBuilder );
        }

        // Add user interface.
        if ( options.AddUserInterface )
        {
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                serviceProviderBuilder.AddService(
                    typeof(IToastNotificationService),
                    serviceProvider => new WindowsToastNotificationService( serviceProvider ) );

                serviceProviderBuilder.AddService( typeof(IUserInterfaceService), serviceProvider => new WindowsUserInterfaceService( serviceProvider ) );
            }
            else
            {
                // TODO: on other OSes we may support a purely browser-based UI.
            }
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

        // Add initialization services.
        serviceProviderBuilder.AddSingleton( serviceProvider => new WelcomeService( serviceProvider ) );
        serviceProviderBuilder.AddSingleton( serviceProvider => new BackstageServicesInitializer( serviceProvider ) );

        return serviceProviderBuilder;
    }

    internal static void AddTelemetryServices( this ServiceProviderBuilder serviceProviderBuilder )
    {
        // Add telemetry.
        serviceProviderBuilder
            .AddSingleton<IExceptionReporter>( serviceProvider => new ExceptionReporter( new TelemetryQueue( serviceProvider ), serviceProvider ) )
            .AddSingleton<ITelemetryUploader>( serviceProvider => new TelemetryUploader( serviceProvider ) )
            .AddSingleton<IUsageReporter>( serviceProvider => new UsageReporter( serviceProvider ) )
            .AddSingleton<ITelemetryConfigurationService>( serviceProvider => new TelemetryConfigurationService( serviceProvider ) );
    }

    private static void TryAddProcessManagerService( this ServiceProviderBuilder serviceProviderBuilder )
    {
        if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
        {
            serviceProviderBuilder.AddSingleton<IProcessManager>( serviceProvider => new WindowsProcessManager( serviceProvider ) );
        }
        else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
        {
            serviceProviderBuilder.AddSingleton<IProcessManager>( serviceProvider => new LinuxProcessManager( serviceProvider ) );
        }
        else if ( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
        {
            serviceProviderBuilder.AddSingleton<IProcessManager>( serviceProvider => new MacProcessManager( serviceProvider ) );
        }
        else
        {
            // Not supported.
        }
    }
}