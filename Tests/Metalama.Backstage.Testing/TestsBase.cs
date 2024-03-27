// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Tools;
using Metalama.Backstage.UserInterface;
using Metalama.Backstage.Welcome;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Xunit.Abstractions;
using ILoggerFactory = Metalama.Backstage.Diagnostics.ILoggerFactory;

namespace Metalama.Backstage.Testing
{
    public abstract class TestsBase
    {
        private readonly IServiceCollection _serviceCollection;

        protected ITestOutputHelper Logger { get; }

        protected TestDateTimeProvider Time { get; } = new();

        protected TestFileSystem FileSystem { get; }

        protected TestEnvironmentVariableProvider EnvironmentVariableProvider { get; } = new();

        protected TestLoggerFactory Log { get; }

        private NullTelemetryUploader TelemetryUploader { get; } = new();

        private TestUsageReporter UsageReporter { get; } = new();

        protected IServiceProvider ServiceProvider { get; }

        // May be null if the different implementation of IConfigurationManager is used.
        protected InMemoryConfigurationManager? ConfigurationManager { get; }

        protected TestProcessExecutor ProcessExecutor { get; } = new();

        protected TestUserDeviceDetectionService UserDeviceDetection { get; } = new();

        protected TestUserInterfaceService UserInterface { get; }

        protected BackstageBackgroundTasksService BackgroundTasks { get; } = new();

        protected TestHttpClientFactory HttpClientFactory { get; }

        protected ILicenseRegistrationService LicenseRegistrationService { get; }

        protected ITelemetryConfigurationService TelemetryConfigurationService { get; set; }

        protected IServiceCollection CloneServiceCollection()
        {
            var services = new ServiceCollection();

            foreach ( var service in this._serviceCollection )
            {
                services.Add( service );
            }

            return services;
        }

        protected TestsBase( ITestOutputHelper logger, IApplicationInfo? applicationInfo = null )
            : this( logger, new BackstageInitializationOptions( applicationInfo ?? new TestApplicationInfo() ) ) { }

        protected virtual void ConfigureServices( ServiceProviderBuilder services ) { }

        internal TestsBase( ITestOutputHelper logger, BackstageInitializationOptions? options )
        {
            this.Logger = logger;

            this.Log = new TestLoggerFactory( logger );

            // ReSharper disable RedundantTypeArgumentsOfMethod

            this._serviceCollection = this.CreateServiceCollection( this.ConfigureServices, options );

            this.ServiceProvider = this._serviceCollection.BuildServiceProvider();
            this.ConfigurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>() as InMemoryConfigurationManager;
            this.FileSystem = (TestFileSystem) this.ServiceProvider.GetRequiredBackstageService<IFileSystem>();
            this.UserInterface = (TestUserInterfaceService) this.ServiceProvider.GetRequiredBackstageService<IUserInterfaceService>();
            this.HttpClientFactory = (TestHttpClientFactory) this.ServiceProvider.GetRequiredBackstageService<IHttpClientFactory>();
            this.LicenseRegistrationService = this.ServiceProvider.GetRequiredBackstageService<ILicenseRegistrationService>();
            this.TelemetryConfigurationService = this.ServiceProvider.GetRequiredBackstageService<ITelemetryConfigurationService>();
        }

        private ServiceCollection CreateServiceCollection(
            Action<ServiceProviderBuilder>? serviceBuilder = null,
            BackstageInitializationOptions? options = null )
        {
            var serviceCollection = new ServiceCollection();
            options ??= new BackstageInitializationOptions( new TestApplicationInfo() );

            serviceCollection
                .AddSingleton( new EarlyLoggerFactory( this.Log ) )
                .AddSingleton<ILoggerFactory>( this.Log )
                .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( options.ApplicationInfo ) )
                .AddSingleton<BackstageInitializationOptionsProvider>( new BackstageInitializationOptionsProvider( options ) )
                .AddSingleton<IDateTimeProvider>( this.Time )
                .AddSingleton<IProcessExecutor>( this.ProcessExecutor )
                .AddSingleton<IPlatformInfo>( serviceProvider => new PlatformInfo( serviceProvider, null ) )
                .AddSingleton( this.BackgroundTasks )
                .AddSingleton<IHttpClientFactory>( _ => new TestHttpClientFactory() )
                .AddSingleton<WebLinks>( _ => new WebLinks() )
                .AddSingleton( _ => new RandomNumberGenerator( 0 ) )

                // We must always have a single instance of the file system even if we use CloneServiceCollection.
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                .AddSingleton<IFileSystem>( serviceProvider => this.FileSystem ?? new TestFileSystem( serviceProvider ) )
                .AddSingleton<IEnvironmentVariableProvider>( this.EnvironmentVariableProvider )
                .AddSingleton<IRecoverableExceptionService>( new TestRecoverableExceptionService() )
                .AddSingleton<IUserDeviceDetectionService>( this.UserDeviceDetection )
                .AddSingleton<ITelemetryUploader>( this.TelemetryUploader )
                .AddSingleton<IUsageReporter>( this.UsageReporter )
                .AddSingleton<IConfigurationManager>( serviceProvider => new InMemoryConfigurationManager( serviceProvider ) )
                .AddSingleton<ITempFileManager>( serviceProvider => new TempFileManager( serviceProvider ) )
                .AddSingleton<ILicenseRegistrationService>( serviceProvider => new LicenseRegistrationService( serviceProvider ) )
                .AddSingleton<IBackstageToolsExecutor>( serviceProvider => new BackstageToolsExecutor( serviceProvider ) )
                .AddSingleton<IBackstageToolsLocator>( serviceProvider => new BackstageToolsLocator( serviceProvider ) )
                .AddSingleton<IUserInterfaceService>( serviceProvider => new TestUserInterfaceService( serviceProvider ) )
                .AddSingleton<IToastNotificationService>( serviceProvider => new ToastNotificationService( serviceProvider ) )
                .AddSingleton<IToastNotificationStatusService>( serviceProvider => new ToastNotificationStatusService( serviceProvider ) )
                .AddSingleton<BackstageServicesInitializer>( serviceProvider => new BackstageServicesInitializer( serviceProvider ) )
                .AddSingleton<WelcomeService>( serviceProvider => new WelcomeService( serviceProvider ) )
                .AddSingleton<IIdeExtensionStatusService>( serviceProvider => new IdeExtensionStatusService( serviceProvider ) )
                .AddSingleton<IToastNotificationDetectionService>( serviceProvider => new ToastNotificationDetectionService( serviceProvider ) )
                .AddSingleton<IStandardDirectories>( serviceProvider => new StandardDirectories( serviceProvider ) )
                .AddSingleton<IBackstageToolsExtractor>( serviceProvider => new BackstageToolsExtractor( serviceProvider ) )
                .AddSingleton<ITelemetryConfigurationService>( serviceProvider => new TelemetryConfigurationService( serviceProvider, Guid.Empty ) );

            var serviceProviderBuilder =
                new ServiceProviderBuilder( ( type, instance ) => serviceCollection.AddSingleton( type, instance ) );

            // The test implementation may replace some services.
            serviceBuilder?.Invoke( serviceProviderBuilder );

            return serviceCollection;
        }
    }
}