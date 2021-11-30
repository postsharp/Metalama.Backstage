// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using BackstageLogLevel = PostSharp.Backstage.Extensibility.LogLevel;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PostSharp.Backstage.MicrosoftLogging
{
    internal static class LogLevelExtensions
    {
        public static MicrosoftLogLevel Convert( this BackstageLogLevel logLevel )
            => logLevel switch
            {
                BackstageLogLevel.Trace => MicrosoftLogLevel.Trace,
                BackstageLogLevel.Debug => MicrosoftLogLevel.Debug,
                BackstageLogLevel.Information => MicrosoftLogLevel.Information,
                BackstageLogLevel.Warning => MicrosoftLogLevel.Warning,
                BackstageLogLevel.Error => MicrosoftLogLevel.Error,
                BackstageLogLevel.Critical => MicrosoftLogLevel.Critical,
                BackstageLogLevel.None => MicrosoftLogLevel.None,
                _ => throw new ArgumentException( $"Unknown log level: {logLevel}", nameof(logLevel) )
            };
    }
}