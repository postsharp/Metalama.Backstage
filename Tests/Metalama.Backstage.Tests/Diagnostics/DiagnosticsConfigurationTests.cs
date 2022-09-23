﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using Xunit;

namespace Metalama.Backstage.Licensing.Tests.Diagnostics;

/// <summary>
/// This tests class works with predefined default configuration set in constructor.
/// </summary>
public class DiagnosticsConfigurationTests
{
    [Fact]
    public void OutdatedConfiguration_DisablesLogging()
    {
        var testFileSystem = new TestFileSystem();

        ( IServiceProvider ServiceProvider, string FileName ) BuildServiceProvider( Action<Configuration.ConfigurationManager>? configure = null )
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IStandardDirectories>( new StandardDirectories() )
                .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() ) )
                .AddSingleton<IDateTimeProvider>( new CurrentDateTimeProvider() )
                .AddSingleton<IFileSystem>( testFileSystem )
                .AddSingleton<IEnvironmentVariableProvider>( new TestEnvironmentVariableProvider() );

            var configurationManager = new Configuration.ConfigurationManager( serviceCollection.BuildServiceProvider() );
            serviceCollection.AddSingleton<IConfigurationManager>( configurationManager );

            serviceCollection
                .AddSingleton<ITempFileManager>( new TempFileManager( serviceCollection.BuildServiceProvider() ) );

            configure?.Invoke( configurationManager );

            var serviceProviderBuilder =
                new ServiceProviderBuilder(
                    ( type, instance ) => serviceCollection.AddSingleton( type, instance ),
                    () => serviceCollection.BuildServiceProvider() );

            serviceProviderBuilder.AddDiagnostics( ProcessKind.Other );

            return (serviceCollection.BuildServiceProvider(), configurationManager.GetFileName( typeof(DiagnosticsConfiguration) ));
        }

        // First: configure the logging.
        var (serviceProvider1, fileName) = BuildServiceProvider(
            configurationManager => configurationManager.Update<DiagnosticsConfiguration>(
                _ => new DiagnosticsConfiguration()
                {
                    Logging = new LoggingConfiguration()
                    {
                        Categories = ImmutableDictionary<string, bool>.Empty.Add( "*", true ),
                        Processes = ImmutableDictionary<ProcessKind, bool>.Empty.Add( ProcessKind.Other, true )
                    }
                } ) );

        // Make sure it actually logs.
        var logger1 = serviceProvider1.GetRequiredBackstageService<ILoggerFactory>().GetLogger( "Foo" );
        Assert.NotNull( logger1.Trace );

        // Manually simulate the last modification of configuration happened before 3 hours.
        testFileSystem.SetLastWriteTime( fileName, DateTime.Now.AddHours( -3 ) );
        var (serviceProvider2, _) = BuildServiceProvider();

        var logger2 = serviceProvider2.GetRequiredBackstageService<ILoggerFactory>().GetLogger( "Foo" );
        Assert.Null( logger2.Trace );
    }
}