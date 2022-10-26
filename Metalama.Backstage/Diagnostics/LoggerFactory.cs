// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Metalama.Backstage.Diagnostics
{
    internal class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new( StringComparer.OrdinalIgnoreCase );
        private readonly object _textWriterSync = new();

        public string? LogFile { get; }

        private readonly ConcurrentQueue<string> _messageQueue = new();
        private TextWriter? _textWriter;
        private volatile int _backgroundTasks;

        internal DiagnosticsConfiguration Configuration { get; }

        internal IDateTimeProvider DateTimeProvider { get; }

        public LoggerFactory( IServiceProvider serviceProvider, DiagnosticsConfiguration configuration, ProcessKind processKind, string? projectName )
        {
            var tempFileManager = serviceProvider.GetRequiredBackstageService<ITempFileManager>();
            this.DateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();

            this.Configuration = configuration;

            if ( this.Configuration.Logging.Processes.TryGetValue( processKind, out var enabled ) && enabled )
            {
                var directory = tempFileManager.GetTempDirectory( "Logs", CleanUpStrategy.Always );

                try
                {
                    RetryHelper.Retry(
                        () =>
                        {
                            if ( !Directory.Exists( directory ) )
                            {
                                Directory.CreateDirectory( directory );
                            }
                        } );

                    var projectNameWithDot = string.IsNullOrEmpty( projectName ) ? "" : "-" + projectName;

                    // The filename must be unique because several instances of the current assembly (of different versions) may be loaded in the process.
                    this.LogFile = Path.Combine(
                        directory,
                        $"Metalama-{processKind}{projectNameWithDot}-{Guid.NewGuid()}.log" );
                }
                catch
                {
                    // Don't fail if we cannot initialize the log.
                }
            }
        }

        private void WriteToFile( object? state )
        {
            lock ( this._textWriterSync )
            {
                if ( this.LogFile == null )
                {
                    throw new InvalidOperationException( $"Cannot write diagnostics. The '{this.GetType().Name}' is not properly initialized." );
                }

                this._textWriter ??= File.CreateText( this.LogFile );

                // Process enqueued messages.
                while ( this._messageQueue.TryDequeue( out var s ) )
                {
                    this._textWriter.WriteLine( s );
                }

                this._backgroundTasks = 0;

                // Process messages that may have been added in the meantime (most of the times, none).
                while ( this._messageQueue.TryDequeue( out var s ) )
                {
                    this._textWriter.WriteLine( s );
                }
            }
        }

        public void WriteLine( string s )
        {
            this._messageQueue.Enqueue( s );

            if ( Interlocked.CompareExchange( ref this._backgroundTasks, 1, 0 ) == 0 )
            {
                ThreadPool.QueueUserWorkItem( this.WriteToFile );
            }
        }

        public ILogger GetLogger( string category )
        {
            if ( this.LogFile != null )
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

        public void Dispose()
        {
            if ( this._backgroundTasks != 0 )
            {
                var spinWait = default(SpinWait);

                while ( this._backgroundTasks > 0 )
                {
                    spinWait.SpinOnce();
                }
            }

            this._textWriter?.Close();
        }
    }
}