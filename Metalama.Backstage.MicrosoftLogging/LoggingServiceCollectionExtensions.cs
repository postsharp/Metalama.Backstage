// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using IMicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IPostSharpLoggerFactory = Metalama.Backstage.Diagnostics.ILoggerFactory;

namespace Metalama.Backstage.MicrosoftLogging
{
    public static class LoggingServiceCollectionExtensions
    {
        public static ServiceProviderBuilder AddMicrosoftLoggerFactory(
            this ServiceProviderBuilder serviceProviderBuilder,
            IMicrosoftLoggerFactory microsoftLoggerFactory )
        {
            serviceProviderBuilder.AddService(
                typeof(IPostSharpLoggerFactory),
                new LoggerFactoryAdapter( microsoftLoggerFactory ) );

            return serviceProviderBuilder;
        }
    }
}