﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Metalama.Backstage.Diagnostics;

internal class LogFileWriter
{
    private const int _inactiveStatus = 0;
    private const int _activeStatus = 1;
    private const int _finishingStatus = 2;
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private readonly IFileSystem _fileSystem;
    private readonly object _textWriterSync = new();
    private readonly ConcurrentQueue<string> _messageQueue = new();
    private TextWriter? _textWriter;
    private volatile int _backgroundTaskStatus;
    private volatile bool _disposing;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Scope { get; }

    public string? LogFile { get; }

    public LogFileWriter( LoggerFactory loggerFactory, string scope )
    {
        this._fileSystem = loggerFactory.FileSystem;
        this.Scope = scope;

        try
        {
            var scopeWithDash = string.IsNullOrEmpty( scope ) ? "" : "-" + scope;

            // The filename must be unique because several instances of the current assembly (of different versions) may be loaded in the process.
            this.LogFile = Path.Combine(
                loggerFactory.LogDirectory!,
                $"Metalama-{loggerFactory.ProcessKind}{scopeWithDash}-{Guid.NewGuid()}.log" );
        }
        catch
        {
            // Don't fail if we cannot initialize the log.
        }
    }

    private void WriteBufferedMessagesToFile( object? state )
    {
        if ( this.LogFile == null )
        {
            return;
        }

        if ( this._disposing )
        {
            this._backgroundTaskStatus = _inactiveStatus;

            return;
        }

        lock ( this._textWriterSync )
        {
            try
            {
                this._textWriter ??= this._fileSystem.CreateTextFile( this.LogFile );

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

        // If there was an error initializing the log file, do not attempt to write anything.
        if ( this.LogFile == null )
        {
            return;
        }

        this._messageQueue.Enqueue( s );

        if ( Interlocked.CompareExchange( ref this._backgroundTaskStatus, _activeStatus, _inactiveStatus ) != _activeStatus )
        {
            ThreadPool.QueueUserWorkItem( this.WriteBufferedMessagesToFile );
        }
    }

    public void Flush()
    {
        if ( this.LogFile == null )
        {
            return;
        }

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

    public void Dispose()
    {
        // Make sure we write the lines that we already have.
        this.Flush();

        // Stop accepting new lines. This also prevents queued records to be processed. 
        this._disposing = true;

        this._textWriter?.Close();
    }
}