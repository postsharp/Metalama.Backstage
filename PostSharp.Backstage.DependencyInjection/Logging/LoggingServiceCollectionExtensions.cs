// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using IMicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IPostSharpLoggerFactory = PostSharp.Backstage.Extensibility.ILoggerFactory;

namespace PostSharp.Backstage.DependencyInjection.Logging
{
    public static class LoggingServiceCollectionExtensions
    {
        public static IServiceCollection AddMicrosoftLoggerFactory( this IServiceCollection serviceCollection, IMicrosoftLoggerFactory microsoftLoggerFactory )
            => serviceCollection.AddSingleton<IPostSharpLoggerFactory>( new LoggerFactoryProxy( microsoftLoggerFactory ) );
    }
}