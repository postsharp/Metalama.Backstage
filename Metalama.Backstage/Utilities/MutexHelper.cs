// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

#if DEBUG
using System.Diagnostics;
#endif

namespace Metalama.Backstage.Utilities;

public static class MutexHelper
{
    public static IDisposable WithGlobalLock( string name, ILogger? logger = null )
    {
        logger?.Trace?.Log( $"Acquiring lock '{name}'." );

        var mutex = OpenOrCreateGlobalMutex( name, logger );

        try
        {
            if ( !mutex.WaitOne( 0 ) )
            {
                logger?.Trace?.Log( $"  Another process owns '{name}'. Waiting." );
                mutex.WaitOne();
            }
        }
        catch ( AbandonedMutexException )
        {
            logger?.Warning?.Log( "  Ignoring an AbandonedMutexException." );
        }

        logger?.Trace?.Log( $"Lock '{name}' acquired." );

        return new MutexHandle( mutex, name, logger );
    }

    public static bool WithGlobalLock( string name, TimeSpan timeout, [NotNullWhen( true )] out IDisposable? mutexHandle, ILogger? logger = null )
    {
        var mutex = OpenOrCreateGlobalMutex( name, logger );

        bool acquired;

        try
        {
            acquired = mutex.WaitOne( timeout );
        }
        catch ( AbandonedMutexException )
        {
            acquired = true;

            logger?.Warning?.Log( "  Ignoring an AbandonedMutexException." );
        }

        if ( acquired )
        {
            logger?.Trace?.Log( $"Lock '{name}' acquired." );

            mutexHandle = new MutexHandle( mutex, name, logger );

            return true;
        }
        else
        {
            logger?.Trace?.Log( $"Lock '{name}' not acquired." );

            mutexHandle = null;

            return false;
        }
    }

    private static Mutex OpenOrCreateGlobalMutex( string fullName, ILogger? logger )
    {
        var mutexName = "Global\\Metalama_" + HashUtilities.HashString( fullName );

        return OpenOrCreateMutex( mutexName, logger );
    }

    // This code is duplicated in DesignTimeEntryPointManager in Metalama.Framework and should be kept in sync.
    internal static Mutex OpenOrCreateMutex( string mutexName, ILogger? logger )
    {
        logger?.Trace?.Log( $"  Mutex name: '{mutexName}'." );

        // The number of iterations is intentionally very low.
        // We will restart if the following occurs:
        //   1) TryOpenExisting fails, i.e. there is no existing mutex.
        //   2) Creating a new mutex fails, i.e. the mutex was created in the meantime by a process with higher set of rights.
        // The probability of mutex being destroyed when we call TryOpenExisting again is fairly low.

#pragma warning disable IDE0059 // Unnecessary assignment of a value

        // ReSharper disable once BadSemicolonSpaces
        for ( var i = 0; ; i++ )
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        {
            // First try opening the mutex.
            if ( Mutex.TryOpenExisting( mutexName, out var existingMutex ) )
            {
                logger?.Trace?.Log( $"  Opened existing mutex." );

                return existingMutex;
            }
            else
            {
                // Otherwise we will try to create the mutex.
                try
                {
                    logger?.Trace?.Log( $"  Creating new mutex." );

                    return new Mutex( false, mutexName );
                }
                catch ( UnauthorizedAccessException )
                {
                    if ( i < 3 )
                    {
                        // Mutex was probably created in the meantime and is not accessible - we will restart.
                        logger?.Trace?.Log( $"  Mutex was probably created and current process has restricted access to it, restarting." );

                        // ReSharper disable once RedundantJumpStatement
                        continue;
                    }
                    else
                    {
                        // There were too many restarts - just rethrow.
                        logger?.Trace?.Log( $"  Tried to open mutex too many times - throwing the exception received." );

                        throw;
                    }
                }
            }
        }
    }

    private sealed class MutexHandle : IDisposable
    {
        private readonly Mutex _mutex;
        private readonly string _name;
        private readonly ILogger? _logger;

#if DEBUG
        private readonly StackTrace _stackTrace = new();
#endif

        public MutexHandle( Mutex mutex, string name, ILogger? logger )
        {
            this._mutex = mutex;
            this._name = name;
            this._logger = logger;
        }

        public void Dispose()
        {
            this._logger?.Trace?.Log( $"Releasing lock '{this._name}'." );

            this._mutex.ReleaseMutex();
            this._mutex.Dispose();

#if DEBUG
            GC.SuppressFinalize( this );
#endif
        }

#pragma warning disable CA1821
#if DEBUG
        ~MutexHandle()
        {
            throw new InvalidOperationException( "The mutex was not disposed. It was acquired here: " + Environment.NewLine + this._stackTrace );
        }
#endif
#pragma warning restore CA1821
    }
}