// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.Logging;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using IPostSharpLogger = PostSharp.Backstage.Extensibility.ILogger;
using PostSharpLogLevel = PostSharp.Backstage.Extensibility.LogLevel;

namespace PostSharp.Backstage.MicrosoftLogging
{
    internal class LoggerAdapter : IPostSharpLogger
    {
        public LoggerAdapter( IMicrosoftLogger logger )
        {
            _logger = logger;
        }

        private readonly IMicrosoftLogger _logger;

        public bool IsEnabled( PostSharpLogLevel logLevel )
        {
            return _logger.IsEnabled( logLevel.Convert() );
        }

        public void LogTrace( string message )
        {
            _logger.LogTrace( message );
        }

        public void LogInformation( string message )
        {
            _logger.LogInformation( message );
        }
    }
}