// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.MicrosoftLogging;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.Globalization;
using System.Reflection;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Metalama.DotNetTools
{
    internal class CommandServiceProvider : ICommandServiceProvider
    {
        public IServiceProvider? ServiceProvider { get; private set; }
        
        public IServiceProvider Initialize( IConsole console, bool addTrace )
        {
            if ( this.ServiceProvider != null )
            {
                throw new InvalidOperationException( "Service provider is initialized already." );
            }
            
            // ReSharper disable RedundantTypeArgumentsOfMethod

            var serviceCollection = new ServiceCollection();

            ILoggerFactory? loggerFactory = null;

            if ( addTrace )
            {
                serviceCollection

                    // https://docs.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
                    .AddLogging( builder => builder.AddConsole() )

                    // https://www.blinkingcaret.com/2018/02/14/net-core-console-logging/
                    .Configure<LoggerFilterOptions>( options => options.MinLevel = LogLevel.Trace );

                loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            }

            var serviceProviderBuilder = new ServiceProviderBuilder(
                ( type, instance ) => serviceCollection.AddSingleton( type, instance ),
                () => serviceCollection.BuildServiceProvider() );

            serviceProviderBuilder
                .AddSingleton<IConsole>( console )
                .AddMinimalBackstageServices( new ApplicationInfo() );

            if ( loggerFactory != null )
            {
                serviceProviderBuilder.AddMicrosoftLoggerFactory( loggerFactory );
            }

            serviceProviderBuilder.AddTelemetryServices();

            var usageSample = serviceProviderBuilder.ServiceProvider.GetService<IUsageReporter>()?.CreateSample( "CompilerUsage" );

            if ( usageSample != null )
            {
                serviceProviderBuilder.AddSingleton<IUsageSample>( usageSample );
            }

            return this.ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private sealed class ApplicationInfo : IApplicationInfo
        {
            public ApplicationInfo()
            {
                var metadataAttributes =
                    typeof(ApplicationInfo).Assembly.GetCustomAttributes(
                        typeof(AssemblyMetadataAttribute),
                        inherit: false );

                string? version = null;
                bool? isPrerelease = null;
                DateTime? buildDate = null;

                bool AllMetadataFound() => version != null && isPrerelease != null && buildDate != null;

                foreach ( var metadataAttributeObject in metadataAttributes )
                {
                    var metadataAttribute = (AssemblyMetadataAttribute) metadataAttributeObject;

                    switch ( metadataAttribute.Key )
                    {
                        case "PackageVersion":
                            if ( !string.IsNullOrEmpty( metadataAttribute.Value ) )
                            {
                                version = metadataAttribute.Value;
                                isPrerelease = version.Contains( '-', StringComparison.Ordinal );
                            }

                            break;

                        case "PackageBuildDate":
                            if ( !string.IsNullOrEmpty( metadataAttribute.Value ) )
                            {
                                buildDate = DateTime.Parse( metadataAttribute.Value, CultureInfo.InvariantCulture );
                            }

                            break;
                    }

                    if ( AllMetadataFound() )
                    {
                        break;
                    }
                }

                if ( !AllMetadataFound() )
                {
                    throw new InvalidOperationException( $"{nameof(ApplicationInfo)} has failed to find some of the required assembly metadata." );
                }

                this.Version = version!;
                this.IsPrerelease = isPrerelease!.Value;
                this.BuildDate = buildDate!.Value;
            }

            public string Name => "Metalama Config";

            public string Version { get; }

            public bool IsPrerelease { get; }

            public DateTime BuildDate { get; }

            public ProcessKind ProcessKind => ProcessKind.MetalamaConfig;

            public bool IsUnattendedProcess( Backstage.Diagnostics.ILoggerFactory loggerFactory )
                => ProcessUtilities.IsCurrentProcessUnattended( loggerFactory );

            public bool IsLongRunningProcess => false;
        }
    }
}