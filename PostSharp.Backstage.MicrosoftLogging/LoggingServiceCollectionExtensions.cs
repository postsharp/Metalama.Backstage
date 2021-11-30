// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using IMicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IPostSharpLoggerFactory = PostSharp.Backstage.Extensibility.ILoggerFactory;

namespace PostSharp.Backstage.MicrosoftLogging
{
    public static class LoggingServiceCollectionExtensions
    {
        public static BackstageServiceCollection AddMicrosoftLoggerFactory(
            this BackstageServiceCollection serviceCollection,
            IMicrosoftLoggerFactory microsoftLoggerFactory )
        {
            return serviceCollection.AddSingleton<IPostSharpLoggerFactory>( new LoggerFactoryAdapter( microsoftLoggerFactory ) );
        }
    }
}