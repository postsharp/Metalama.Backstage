// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Audit;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Utilities;
using Metalama.Backstage.Welcome;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Backstage.Extensibility;

/// <summary>
/// Extension methods for setting up default services in an <see cref="ServiceProviderBuilder" />.
/// </summary>
public static class RegisterServiceExtensions
{
    internal static ServiceProviderBuilder AddUntypedSingleton<T>(
        this ServiceProviderBuilder serviceProviderBuilder,
        T instance )
        where T : notnull
    {
        serviceProviderBuilder.AddService( typeof(T), instance );

        return serviceProviderBuilder;
    }

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
    /// Adds a service providing paths of standard directories to the specified <see cref="ServiceProviderBuilder" />.
    /// </summary>
    /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
    /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
    internal static ServiceProviderBuilder AddStandardDirectories( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IStandardDirectories>( new StandardDirectories() );

    // Internal for test only.
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
                c =>
                {
                    return c with { Logging = c.Logging with { Processes = c.Logging.Processes.ToImmutableDictionary( x => x.Key, _ => false ) } };
                } );

            configuration = configurationManager.Get<DiagnosticsConfiguration>();
        }

        var applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
        var loggerFactory = new LoggerFactory( serviceProvider, configuration, applicationInfo.ProcessKind, projectName );

        return serviceProviderBuilder.AddSingleton<ILoggerFactory>( loggerFactory );
    }

    /// <summary>
    /// Adds the minimal set of services required by logging and telemetry.
    /// </summary>
    private static ServiceProviderBuilder AddDiagnosticsRequirements(
        this ServiceProviderBuilder serviceProviderBuilder,
        IApplicationInfo applicationInfo )
    {
        serviceProviderBuilder = serviceProviderBuilder
            .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( applicationInfo ) )
            .AddCurrentDateTimeProvider()
            .AddFileSystem()
            .AddEnvironmentVariableProvider()
            .AddStandardDirectories()
            .AddConfigurationManager();

        serviceProviderBuilder.AddService( typeof(ITempFileManager), new TempFileManager( serviceProviderBuilder.ServiceProvider ) );

        return serviceProviderBuilder;
    }

    internal static ServiceProviderBuilder AddConfigurationManager( this ServiceProviderBuilder serviceProviderBuilder )
        => serviceProviderBuilder.AddSingleton<IConfigurationManager>( new ConfigurationManager( serviceProviderBuilder.ServiceProvider ) );

    private static ServiceProviderBuilder AddPlatformInfo(
        this ServiceProviderBuilder serviceProviderBuilder,
        string? dotnetSdkDirectory )
    {
        return serviceProviderBuilder.AddSingleton<IPlatformInfo>( new PlatformInfo( serviceProviderBuilder.ServiceProvider, dotnetSdkDirectory ) );
    }

    private static ServiceProviderBuilder AddLicensing(
        this ServiceProviderBuilder serviceProviderBuilder,
        bool considerUnattendedLicense = false,
        bool ignoreUserProfileLicenses = false,
        string? projectLicense = null )
    {
        var licenseConsumptionManager = LicenseConsumptionManagerFactory.Create(
            serviceProviderBuilder.ServiceProvider,
            considerUnattendedLicense,
            ignoreUserProfileLicenses,
            projectLicense );

        serviceProviderBuilder.AddSingleton( licenseConsumptionManager );

        return serviceProviderBuilder;
    }

    public static ServiceProviderBuilder AddBackstageServices(
        this ServiceProviderBuilder serviceProviderBuilder,
        IApplicationInfo applicationInfo,
        string? projectName = null,
        bool considerUnattendedProcessLicense = false,
        bool ignoreUserProfileLicenses = false,
        string? projectLicense = null,
        string? dotNetSdkDirectory = null,
        bool openWelcomePage = false,
        bool addLicenseConsumption = true,
        bool addSupportServices = true,
        bool addLicenseAudit = true,
        Action<ServiceProviderBuilder>? addLoggerFactoryAction = null )
    {
        // Add base services.
        serviceProviderBuilder = serviceProviderBuilder
            .AddDiagnosticsRequirements( applicationInfo );

        // Add diagnostics.
        if ( addSupportServices )
        {
            if ( addLoggerFactoryAction == null )
            {
                serviceProviderBuilder = serviceProviderBuilder
                    .AddDiagnostics( applicationInfo.ProcessKind, projectName );

                var serviceProvider = serviceProviderBuilder.ServiceProvider;

                // First-run configuration. This must be done before initializing licensing and telemetry.
                var registerEvaluationLicense =
                    !ignoreUserProfileLicenses
                    && !applicationInfo.IsPreviewLicenseEligible()
                    && !applicationInfo.IsUnattendedProcess( serviceProvider.GetLoggerFactory() );

                var welcomeService = new WelcomeService( serviceProvider );
                welcomeService.ExecuteFirstStartSetup( registerEvaluationLicense );

                if ( openWelcomePage )
                {
                    welcomeService.OpenWelcomePage();
                }
            }
            else
            {
                addLoggerFactoryAction( serviceProviderBuilder );
            }
        }

        // Add platform info.
        serviceProviderBuilder.AddPlatformInfo( dotNetSdkDirectory );

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

        // Add support services.
        if ( addSupportServices )
        {
            serviceProviderBuilder.AddTelemetryServices();
        }

        // Add license audit
        if ( addLicenseAudit )
        {
            if ( !addSupportServices )
            {
                throw new ArgumentException( "Support services are required for license audit.", nameof(addSupportServices) );
            }

            // License audit requires support services. 
            serviceProviderBuilder.AddSingleton<ILicenseAuditManager>( new LicenseAuditManager( serviceProviderBuilder.ServiceProvider ) );
        }

        // Add licensing.
        if ( addLicenseConsumption )
        {
            serviceProviderBuilder.AddLicensing( considerUnattendedProcessLicense, ignoreUserProfileLicenses, projectLicense );
        }

        return serviceProviderBuilder;
    }

    private static ServiceProviderBuilder AddTelemetryServices( this ServiceProviderBuilder serviceProviderBuilder )
    {
        // Add telemetry.
        var queue = new TelemetryQueue( serviceProviderBuilder.ServiceProvider );

        return serviceProviderBuilder
            .AddSingleton<ITelemetryUploader>( new TelemetryUploader( serviceProviderBuilder.ServiceProvider ) )
            .AddSingleton<IExceptionReporter>( new ExceptionReporter( queue, serviceProviderBuilder.ServiceProvider ) )
            .AddSingleton<IUsageReporter>( new UsageReporter( serviceProviderBuilder.ServiceProvider ) );
    }
}