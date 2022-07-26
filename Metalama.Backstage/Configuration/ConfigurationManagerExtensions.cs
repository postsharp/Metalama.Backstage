// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Backstage.Configuration
{
    public static class ConfigurationManagerExtensions
    {
        private const int _maxUpdateAttempts = 10;

        public static T Get<T>( this IConfigurationManager configurationManager, bool ignoreCache = false )
            where T : ConfigurationFile
            => (T) configurationManager.Get( typeof(T), ignoreCache );

        public static string GetFileName<T>( this IConfigurationManager configurationManager )
            where T : ConfigurationFile
            => configurationManager.GetFileName( typeof(T) );

        public static bool UpdateIf<T>( this IConfigurationManager configurationManager, Predicate<T> condition, Action<T> action )
            where T : ConfigurationFile
        {
            T editableCopy;
            T originalSettings;

            var attempts = 0;

            do
            {
                attempts++;

                configurationManager.Logger.Trace?.Log( $"{attempts}-th attempt to update {typeof(T).Name}" );

                if ( attempts > _maxUpdateAttempts )
                {
                    throw new InvalidOperationException( "Too many attempts to update the configuration. There must be an unaddressed race condition." );
                }

                originalSettings = configurationManager.Get<T>( true );
                editableCopy = (T) originalSettings.Clone();

                if ( !condition( editableCopy ) )
                {
                    configurationManager.Logger.Trace?.Log(
                        $"Update of {typeof(T).Name} skipped because the configuration setting was already in the desired state." );

                    return false;
                }

                action( editableCopy );
            }
            while ( !configurationManager.TryUpdate( editableCopy, originalSettings.LastModified ) );

            return true;
        }

        public static bool Update<T>( this IConfigurationManager configurationManager, Action<T> action )
            where T : ConfigurationFile, new()
        {
            T editableCopy;
            T originalSettings;

            var attempts = 0;

            do
            {
                attempts++;

                configurationManager.Logger.Trace?.Log( $"{attempts}-th attempt to update {typeof(T).Name}" );

                if ( attempts > _maxUpdateAttempts )
                {
                    throw new InvalidOperationException( "Too many attempts to update the configuration. There must be an unaddressed race condition." );
                }

                originalSettings = configurationManager.Get<T>( true );
                editableCopy = (T) originalSettings.Clone();
                action( editableCopy );
            }
            while ( !configurationManager.TryUpdate( editableCopy, originalSettings.LastModified ) );

            return true;
        }
    }
}