// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using IMicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IPostSharpLoggerFactory = PostSharp.Backstage.Logging.ILoggerFactory;

namespace PostSharp.Backstage.MicrosoftLogging
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