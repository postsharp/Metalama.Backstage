// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.LicenseKeyReader
{
    internal static class ServiceProvider
    {
        public static IServiceProvider Instance { get; }

        static ServiceProvider()
        {
            var logger = new EventBackEndLogger();

            var serviceCollection = new ServiceCollection();

            var serviceProviderBuilder = new ServiceProviderBuilder(
                ( type, instance ) => serviceCollection.AddSingleton( type, instance ),
                () => serviceCollection.BuildServiceProvider() );

            serviceProviderBuilder
                .AddCurrentDateTimeProvider()
                .AddSingleton<IBackstageDiagnosticSink>( logger )
                .AddSingleton<EventBackEndLogger>( logger )
                .AddSingleton<LicenseFactory>( new LicenseFactory( serviceProviderBuilder.ServiceProvider ) );

            Instance = serviceProviderBuilder.ServiceProvider;
        }
    }
}