// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using IPostSharpLogger = Metalama.Backstage.Diagnostics.ILogger;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Metalama.Backstage.MicrosoftLogging
{
    internal class LoggerAdapter : IPostSharpLogger
    {
        private readonly IMicrosoftLogger _logger;

        public LoggerAdapter( IMicrosoftLogger logger )
        {
            this._logger = logger;
            this.Error = this.CreateLogWriter( LogLevel.Error );
            this.Info = this.CreateLogWriter( LogLevel.Information );
            this.Trace = this.CreateLogWriter( LogLevel.Trace );
            this.Warning = this.CreateLogWriter( LogLevel.Warning );
        }

        private LogWriter? CreateLogWriter( LogLevel logLevel )
        {
            if ( this._logger.IsEnabled( logLevel ) )
            {
                return new LogWriter( this._logger, logLevel );
            }
            else
            {
                return null;
            }
        }

        public ILogWriter? Trace { get; }

        public ILogWriter? Info { get; }

        public ILogWriter? Warning { get; }

        public ILogWriter? Error { get; }

        public IPostSharpLogger WithPrefix( string prefix ) => throw new NotImplementedException();

        private class LogWriter : ILogWriter
        {
            private readonly IMicrosoftLogger _logger;
            private readonly LogLevel _logLevel;

            public LogWriter( IMicrosoftLogger logger, LogLevel logLevel )
            {
                this._logger = logger;
                this._logLevel = logLevel;
            }

            public void Log( string message )
            {
                this._logger.Log( this._logLevel, $"Thread {Thread.CurrentThread.ManagedThreadId}: {message}" );
            }
        }
    }
}