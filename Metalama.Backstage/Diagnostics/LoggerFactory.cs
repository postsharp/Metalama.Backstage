// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Utilities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Metalama.Backstage.Diagnostics
{
    internal class LoggerFactory : ILoggerFactory
    {
        private const int _inactiveStatus = 0;
        private const int _activeStatus = 1;
        private const int _finishingStatus = 2;
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new( StringComparer.OrdinalIgnoreCase );
        private readonly object _textWriterSync = new();
        private readonly ConcurrentQueue<string> _messageQueue = new();
        private TextWriter? _textWriter;
        private volatile int _backgroundTaskStatus;
        private volatile bool _disposing;

        public string? LogFile { get; }

        internal DiagnosticsConfiguration Configuration { get; }

        internal IDateTimeProvider DateTimeProvider { get; }

        public LoggerFactory( IServiceProvider serviceProvider, DiagnosticsConfiguration configuration, ProcessKind processKind, string? projectName )
        {
            var tempFileManager = serviceProvider.GetRequiredBackstageService<ITempFileManager>();
            this.DateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();

            this.Configuration = configuration;

            if ( this.Configuration.Logging.Processes.TryGetValue( processKind.ToString(), out var enabled ) && enabled )
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

        private void WriteBufferedMessagesToFile( object? state )
        {
            if ( this.LogFile == null )
            {
                throw new InvalidOperationException( $"Cannot write diagnostics. The '{this.GetType().Name}' is not properly initialized." );
            }

            if ( this._disposing )
            {
                return;
            }

            lock ( this._textWriterSync )
            {
                try
                {
                    this._textWriter ??= File.CreateText( this.LogFile );

                    // Process enqueued messages.
                    var lastFlush = _stopwatch.ElapsedMilliseconds;

                    while ( !this._messageQueue.IsEmpty )
                    {
                        while ( this._messageQueue.TryDequeue( out var s ) )
                        {
                            this._textWriter.WriteLine( s );

                            // Flush at least every 250 ms.
                            if ( _stopwatch.ElapsedMilliseconds > lastFlush + 250 )
                            {
                                this._textWriter.Flush();
                                lastFlush = _stopwatch.ElapsedMilliseconds;
                            }
                        }

                        // Avoid too frequent start and stop of the task because this would also result in frequent flushing.
                        Thread.Sleep( 100 );
                    }

                    // Say that we are going to end the task.
                    this._backgroundTaskStatus = _finishingStatus;

                    // Process messages that may have been added in the meantime.
                    while ( this._messageQueue.TryDequeue( out var s ) )
                    {
                        this._textWriter.WriteLine( s );
                    }

                    this._textWriter.Flush();
                }
                catch ( ObjectDisposedException ) { }
                finally
                {
                    this._backgroundTaskStatus = _inactiveStatus;    
                }
            }
        }

        public void WriteLine( string s )
        {
            // Make sure that we are not starting a background writer thread after we start disposing.
            // Note that there can still be a race between Dispose in WriteLine, so we could still start a task, but we are
            // also checking the disposing flag in WriteBufferedMessagesToFile.
            if ( this._disposing )
            {
                return;
            }
            
            this._messageQueue.Enqueue( s );

            if ( Interlocked.CompareExchange( ref this._backgroundTaskStatus, _activeStatus, _inactiveStatus ) != _activeStatus )
            {
                ThreadPool.QueueUserWorkItem( this.WriteBufferedMessagesToFile );
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

        public void Flush()
        {
            if ( this._backgroundTaskStatus != _inactiveStatus )
            {
                var spinWait = default(SpinWait);

                while ( this._backgroundTaskStatus != _inactiveStatus )
                {
                    spinWait.SpinOnce();
                }
            }

            this._textWriter?.Flush();
        }

        public void Close()
        {
            this._disposing = true;
            this.Flush();
            this._textWriter?.Close();
        }

        public void Dispose()
        {
            this.Flush();
            
            // TODO: Have proper project-scoped logger factories. This class should be non-disposable.
            // this.Close();
        }
    }
}