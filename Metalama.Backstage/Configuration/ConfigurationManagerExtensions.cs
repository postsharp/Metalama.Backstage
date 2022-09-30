// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Configuration
{
    public static class ConfigurationManagerExtensions
    {
        private const int _maxUpdateAttempts = 10;

        public static T Get<T>( this IConfigurationManager configurationManager, bool ignoreCache = false )
            where T : ConfigurationFile
            => (T) configurationManager.Get( typeof(T), ignoreCache );

        public static string GetFilePath<T>( this IConfigurationManager configurationManager )
            where T : ConfigurationFile
            => configurationManager.GetFilePath( typeof(T) );

        public static bool CreateIfMissing<T>( this IConfigurationManager configurationManager )
            where T : ConfigurationFile
            => configurationManager.UpdateIf<T>( c => !c.LastModified.HasValue, c => c );

        public static bool UpdateIf<T>( this IConfigurationManager configurationManager, Predicate<T> condition, Func<T, T> updateFunc )
            where T : ConfigurationFile
        {
            T newSettings;
            T originalSettings;

            var attempts = 0;

            do
            {
                attempts++;

                configurationManager.Logger.Trace?.Log( $"{attempts}-th attempt to update {typeof(T).Name}" );

                if ( attempts > _maxUpdateAttempts )
                {
                    throw new InvalidOperationException(
                        $"Too many attempts to update the configuration {typeof(T).Name}. There must be an unaddressed race condition." );
                }

                originalSettings = configurationManager.Get<T>( true );

                if ( !condition( originalSettings ) )
                {
                    configurationManager.Logger.Trace?.Log(
                        $"Update of {typeof(T).Name} skipped because the configuration setting was already in the desired state." );

                    return false;
                }

                newSettings = updateFunc( originalSettings );

                if ( originalSettings.LastModified.HasValue && newSettings.Equals( originalSettings ) )
                {
                    configurationManager.Logger.Trace?.Log( $"Update of {typeof(T).Name} skipped because no change was required." );

                    return false;
                }
            }
            while ( !configurationManager.TryUpdate( newSettings, originalSettings.LastModified ) );

            return true;
        }

        public static bool Update<T>( this IConfigurationManager configurationManager, Func<T, T> updateFunc )
            where T : ConfigurationFile, new()
        {
            T newSettings;
            T originalSettings;

            var attempts = 0;

            do
            {
                attempts++;

                configurationManager.Logger.Trace?.Log( $"{attempts}-th attempt to update {typeof(T).Name}" );

                if ( attempts > _maxUpdateAttempts )
                {
                    throw new InvalidOperationException(
                        $"Too many attempts to update the configuration {typeof(T).Name}. There must be an unaddressed race condition." );
                }

                originalSettings = configurationManager.Get<T>( true );

                newSettings = updateFunc( originalSettings );

                if ( originalSettings.LastModified.HasValue && newSettings.Equals( originalSettings ) )
                {
                    configurationManager.Logger.Trace?.Log( $"Update of {typeof(T).Name} skipped because no change was required." );

                    return false;
                }
            }
            while ( !configurationManager.TryUpdate( newSettings, originalSettings.LastModified ) );

            return true;
        }
    }
}