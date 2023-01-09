// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

namespace Metalama.Backstage.Utilities
{
    [PublicAPI]
    public static class RetryHelper
    {
        public static void Retry( Action action, Predicate<Exception>? retryPredicate = null, ILogger? logger = null, Action<Exception>? onException = null )
            => Retry(
                () =>
                {
                    action();

                    return true;
                },
                retryPredicate,
                logger,
                onException );

        public static void RetryWithLockDetection(
            string file,
            Action<string> action,
            IServiceProvider serviceProvider,
            Predicate<Exception>? retryPredicate = null,
            ILogger? logger = null )
            => RetryWithLockDetection( new[] { file }, action, serviceProvider, retryPredicate, logger );

        public static void RetryWithLockDetection(
            IReadOnlyList<string> files,
            Action<string> action,
            IServiceProvider serviceProvider,
            Predicate<Exception>? retryPredicate = null,
            ILogger? logger = null )
        {
            try
            {
                foreach ( var file in files )
                {
                    Retry( () => action( file ), retryPredicate, logger, OnException );
                }
            }
            catch ( Exception e )
            {
                var lockingDetection = serviceProvider.GetBackstageService<ILockingProcessDetector>();

                if ( lockingDetection != null )
                {
                    var lockingProcesses = lockingDetection.GetProcessesUsingFiles( files );

                    if ( lockingProcesses.Count > 0 )
                    {
                        var additionalMessage =
                            $" The following process(es) are locking the file(s): {string.Join( ", ", lockingProcesses.Select( p => $"{p.ProcessName} ({p.Id})" ) )}.";

                        throw new LockedFileException( e.Message + additionalMessage, e );
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            void OnException( Exception obj )
            {
                var lockingDetection = serviceProvider.GetBackstageService<ILockingProcessDetector>();

                if ( lockingDetection != null && logger != null )
                {
                    var lockingProcesses = lockingDetection.GetProcessesUsingFiles( files );

                    if ( lockingProcesses.Count == 0 )
                    {
                        logger.Trace?.Log( "No process locking these files was found." );
                    }
                    else
                    {
                        logger.Warning?.Log(
                            "The following process(es) are locking these files: " + string.Join(
                                ", ",
                                lockingProcesses.Select( p => $"{p.ProcessName} ({p.Id})" ) ) );
                    }
                }
            }
        }

        [ExcludeFromCodeCoverage]
        public static T Retry<T>( Func<T> action, Predicate<Exception>? retryPredicate = null, ILogger? logger = null, Action<Exception>? onException = null )
        {
            var delay = 10.0;
            const int maxAttempts = 12;
            retryPredicate ??= e => e is UnauthorizedAccessException or IOException || (uint) e.HResult == 0x80070020;

            for ( var i = 0; /* nothing */; i++ )
            {
                try
                {
                    return action();
                }
                catch ( Exception e ) when ( i < maxAttempts && retryPredicate( e ) )
                {
                    logger?.Warning?.Log( $"{nameof(RetryHelper)} caught {e.GetType().Name} '{e.Message}'. Retrying in {delay}." );

                    if ( i == 0 )
                    {
                        onException?.Invoke( e );
                    }

                    Thread.Sleep( TimeSpan.FromMilliseconds( delay ) );
                    delay *= 1.2;
                }
            }
        }
    }
}