// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Metalama.Backstage.Utilities
{
    [PublicAPI]
    public static partial class RetryHelper
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
            var context = new DeadlockDetectionContext( serviceProvider, logger, files );

            ExecuteWithLockDetection(
                () =>
                {
                    foreach ( var file in files )
                    {
                        Retry( () => action( file ), retryPredicate, logger, context.OnRecoverableException );
                    }
                },
                context );
        }

        public static void RetryWithLockDetection(
            IReadOnlyList<string> files,
            Action action,
            IServiceProvider serviceProvider,
            Predicate<Exception>? retryPredicate = null,
            ILogger? logger = null )
        {
            var context = new DeadlockDetectionContext( serviceProvider, logger, files );

            ExecuteWithLockDetection( action, context );
        }

        private static void ExecuteWithLockDetection(
            Action action,
            DeadlockDetectionContext context )
        {
            try
            {
                action();
            }
            catch ( Exception e )
            {
                context.OnFatalException( e );

                throw;
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