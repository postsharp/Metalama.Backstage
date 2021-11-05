// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.Logging;
using System;

namespace PostSharp.Backstage.DependencyInjection.Logging
{
    public static class LogLevelExtensions
    {
        public static LogLevel Convert( this Extensibility.LogLevel logLevel )
            => logLevel switch
            {
                Extensibility.LogLevel.Trace => LogLevel.Trace,
                Extensibility.LogLevel.Debug => LogLevel.Debug,
                Extensibility.LogLevel.Information => LogLevel.Information,
                Extensibility.LogLevel.Warning => LogLevel.Warning,
                Extensibility.LogLevel.Error => LogLevel.Error,
                Extensibility.LogLevel.Critical => LogLevel.Critical,
                Extensibility.LogLevel.None => LogLevel.None,
                _ => throw new ArgumentException( $"Unknown log level: {logLevel}", nameof(logLevel) )
            };
    }
}