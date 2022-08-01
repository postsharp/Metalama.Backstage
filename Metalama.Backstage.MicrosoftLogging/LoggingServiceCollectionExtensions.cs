// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

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