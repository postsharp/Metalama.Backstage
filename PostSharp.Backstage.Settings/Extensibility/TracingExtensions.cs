// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace PostSharp.Backstage.Extensibility
{
    public static class TracingExtensions
    {
        public static ILogger? GetOptionalTraceLogger<T>( this IServiceProvider services )
        {
            var logger = services.GetService<ILoggerFactory>()?.CreateLogger<T>();

            return logger == null || !logger.IsEnabled( LogLevel.Trace ) ? null : logger;
        }
    }
}