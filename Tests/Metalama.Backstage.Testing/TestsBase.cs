﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

        public ITestOutputHelper Logger { get; }

        public TestDateTimeProvider Time { get; } = new();

        public TestFileSystem FileSystem { get; }

        public TestEnvironmentVariableProvider EnvironmentVariableProvider { get; } = new();

        public TestLoggerFactory Log { get; }

        public NullTelemetryUploader TelemetryUploader { get; } = new();

        public TestUsageReporter UsageReporter { get; } = new();

        public IServiceProvider ServiceProvider { get; }

        // May be null if the different implementation of IConfigurationManager is used.
        public InMemoryConfigurationManager? ConfigurationManager { get; }

        public TestProcessExecutor ProcessExecutor { get; } = new();

        public TestUserDeviceDetectionService UserDeviceDetection { get; } = new();

        public TestUserInterfaceService UserInterface { get; }

        public TestToastNotificationService ToastNotifications { get; }

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

        protected TestsBase( ITestOutputHelper logger, BackstageInitializationOptions? options )
        {
            this.Logger = logger;

            this.Log = new TestLoggerFactory( logger );

            // ReSharper disable RedundantTypeArgumentsOfMethod

            this._serviceCollection = this.CreateServiceCollection( this.ConfigureServices, options );

            this.ServiceProvider = this._serviceCollection.BuildServiceProvider();
            this.ConfigurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>() as InMemoryConfigurationManager;
            this.FileSystem = (TestFileSystem) this.ServiceProvider.GetRequiredBackstageService<IFileSystem>();
            this.UserInterface = (TestUserInterfaceService) this.ServiceProvider.GetRequiredBackstageService<IUserInterfaceService>();
            this.ToastNotifications = (TestToastNotificationService) this.ServiceProvider.GetRequiredBackstageService<IToastNotificationService>();
        }

        protected ServiceCollection CreateServiceCollection(
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

                // We must always have a single instance of the file system even if we use CloneServiceCollection.
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
                .AddSingleton<IToastNotificationService>( serviceProvider => new TestToastNotificationService( serviceProvider ) )
                .AddSingleton<BackstageServicesInitializer>( serviceProvider => new BackstageServicesInitializer( serviceProvider ) )
                .AddSingleton<WelcomeService>( serviceProvider => new WelcomeService( serviceProvider ) )
                .AddSingleton<IIdeExtensionStatusService>( serviceProvider => new IdeExtensionStatusService( serviceProvider ) );

            var serviceProviderBuilder =
                new ServiceProviderBuilder( ( type, instance ) => serviceCollection.AddSingleton( type, instance ) );

            serviceProviderBuilder.AddStandardDirectories();

            // The test implementation may replace some services.
            serviceBuilder?.Invoke( serviceProviderBuilder );

            return serviceCollection;
        }
    }
}