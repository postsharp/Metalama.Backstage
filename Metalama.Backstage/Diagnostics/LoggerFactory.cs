// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Utilities;
using System;
using System.Collections.Concurrent;

namespace Metalama.Backstage.Diagnostics
{
    internal class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new( StringComparer.OrdinalIgnoreCase );

        private readonly ConcurrentDictionary<string, LogFileWriter> _logFileWriters = new();

        public LoggerFactory(
            IServiceProvider serviceProvider,
            DiagnosticsConfiguration configuration,
            ProcessKind processKind )
        {
            this.DateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();

            this.Configuration = configuration;
            this.ProcessKind = processKind;

            var tempFileManager = serviceProvider.GetRequiredBackstageService<ITempFileManager>();

            if ( configuration.Logging.Processes.TryGetValue( processKind.ToString(), out var enabled ) && enabled )
            {
                this.LogDirectory = tempFileManager.GetTempDirectory( "Logs", CleanUpStrategy.Always );
            }
        }

        public string? LogDirectory { get; }

        internal DiagnosticsConfiguration Configuration { get; }

        internal IDateTimeProvider DateTimeProvider { get; }

        public ProcessKind ProcessKind { get; }

        public bool IsEnabled => this.LogDirectory != null;

        public IDisposable EnterScope( string scope )
        {
            var previousScope = LoggingContext.Current.Value;
            LoggingContext.Current.Value = new LoggingContext( scope );

            return new DisposableAction(
                () =>
                {
                    this.CloseScope( scope );
                    LoggingContext.Current.Value = previousScope;
                } );
        }

        public ILogger GetLogger( string category )
        {
            if ( this.IsEnabled )
            {
                if ( this._loggers.TryGetValue( category, out var logger ) )
                {
                    return logger;
                }
                else
                {
                    logger = new Logger( this, category );

                    return this._loggers.GetOrAdd( category, logger );
                }
            }
            else
            {
                return NullLogger.Instance;
            }
        }

        public LogFileWriter GetLogFileWriter() => this.GetLogFileWriter( LoggingContext.Current.Value?.Scope ?? "" );

        // Used for testing.
        internal event Action<string>? FileCreated;

        private LogFileWriter GetLogFileWriter( string scope )
        {
            if ( !this._logFileWriters.TryGetValue( scope, out var writer ) )
            {
                writer = this._logFileWriters.GetOrAdd(
                    scope,
                    s =>
                    {
                        var newWriter = new LogFileWriter( this, s );

                        if ( newWriter.LogFile != null )
                        {
                            this.FileCreated?.Invoke( newWriter.LogFile );
                        }

                        return newWriter;
                    } );
            }

            return writer;
        }

        private void CloseScope( string name )
        {
            if ( this._logFileWriters.TryRemove( name, out var file ) )
            {
                file.Dispose();
            }
        }

        public void Flush()
        {
            foreach ( var file in this._logFileWriters )
            {
                file.Value.Flush();
            }
        }

        public void Close()
        {
            foreach ( var file in this._logFileWriters )
            {
                file.Value.Dispose();
            }

            this._logFileWriters.Clear();
        }
    }
}