// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Backstage.Utilities
{
    internal static class MutexHelper
    {
        public static IDisposable WithGlobalLock( string name )
        {
            var mutex = CreateGlobalMutex( name );
            mutex.WaitOne();

            return new MutexHandle( mutex );
        }

        public static bool WithGlobalLock( string name, TimeSpan timeout, [NotNullWhen( true )] out IDisposable? mutexHandle )
        {
            var mutex = CreateGlobalMutex( name );

            if ( mutex.WaitOne( timeout ) )
            {
                mutexHandle = new MutexHandle( mutex );

                return true;
            }
            else
            {
                mutexHandle = null;

                return false;
            }
        }

        private static Mutex CreateGlobalMutex( string fullName )
        {
            var mutexName = @"Global\Metalama_" + HashUtilities.HashString( fullName );

            return new Mutex( false, mutexName );
        }

        private class MutexHandle : IDisposable
        {
            private readonly Mutex _mutex;

            public MutexHandle( Mutex mutex )
            {
                this._mutex = mutex;
            }

            public void Dispose()
            {
                this._mutex.ReleaseMutex();
                this._mutex.Dispose();
            }
        }
    }
}