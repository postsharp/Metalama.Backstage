// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Configuration
{
    public static class ConfigurationManagerExtensions
    {
        public static T Get<T>( this IConfigurationManager configurationManager, bool ignoreCache = false )
            where T : ConfigurationFile
            => (T) configurationManager.Get( typeof(T), ignoreCache );

        public static string GetFileName<T>( this IConfigurationManager configurationManager )
            where T : ConfigurationFile
            => configurationManager.GetFileName( typeof(T) );

        /// <summary>
        /// Updates a setting file by supplying new values. Any previous version will
        /// be overwritten.
        /// </summary>
        public static void Update( this IConfigurationManager configurationManager, ConfigurationFile value )
        {
            ConfigurationFile originalSettings;

            do
            {
                originalSettings = configurationManager.Get( value.GetType(), true );
            }
            while ( !configurationManager.TryUpdate( value, originalSettings.LastModified ) );
        }

        /// <summary>
        /// Updates the settings by supplying a delegate that changes the object.
        /// The delegate may be invoked several times if a race happens.
        /// </summary>
        public static bool Update( this IConfigurationManager configurationManager, Type type, Func<ConfigurationFile, bool> action )
        {
            ConfigurationFile editableCopy;
            ConfigurationFile originalSettings;

            do
            {
                originalSettings = configurationManager.Get( type, true );
                editableCopy = originalSettings.Clone();

                if ( !action( editableCopy ) )
                {
                    return false;
                }
            }
            while ( !configurationManager.TryUpdate( editableCopy, originalSettings.LastModified ) );

            return true;
        }

        public static bool Update<T>( this IConfigurationManager configurationManager, Func<T, bool> action )
            where T : ConfigurationFile
        {
            T editableCopy;
            T originalSettings;

            do
            {
                originalSettings = configurationManager.Get<T>( true );
                editableCopy = (T) originalSettings.Clone();

                if ( !action( editableCopy ) )
                {
                    return false;
                }
            }
            while ( !configurationManager.TryUpdate( editableCopy, originalSettings.LastModified ) );

            return true;
        }

        public static bool Update<T>( this IConfigurationManager configurationManager, Action<T> action )
            where T : ConfigurationFile, new()
        {
            T editableCopy;
            T originalSettings;

            do
            {
                originalSettings = configurationManager.Get<T>( true );
                editableCopy = (T) originalSettings.Clone();
                action( editableCopy );
            }
            while ( !configurationManager.TryUpdate( editableCopy, originalSettings.LastModified ) );

            return true;
        }
    }
}