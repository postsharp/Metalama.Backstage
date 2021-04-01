// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PostSharp.Backstage.Licensing.Helpers
{
    internal static class LicensingRegistryHelper
    {
        private const string leasedlicensesKeyName = "LeasedLicenses";
        private const string processedTypesCounterSubKey = "ProcessedTypesCounters";

        public static bool HasUserRegistryLicense()
        {
            return GetUserLicenseKeysFromRegistry().Any();
        }

        internal static IEnumerable<LicenseConfiguration> GetUserLicenseKeysFromRegistry()
        {
            return GetUserLicenseKeysFromRegistry( false ).Union( GetUserLicenseKeysFromRegistry( true ) );
        }

        internal static List<LicenseConfiguration> GetUserLicenseKeysFromRegistry( bool allUsers )
        {
            LicenseSource source = allUsers ? LicenseSource.AllUsersRegistry : LicenseSource.CurrentUserRegistry;
            string sourceDescription = allUsers ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";

            LicensingTrace.Licensing?.WriteLine( "Loading licenses from {0}.", sourceDescription );

            List<LicenseConfiguration> licenses = new List<LicenseConfiguration>();
            
            IRegistryKey registryKey = UserSettings.OpenRegistryKey( allUsers );

            if ( registryKey == null )
            {
                LicensingTrace.Licensing?.WriteLine( "Failed to open {0}.", sourceDescription );
                return licenses;
            }

            LicensingTrace.Licensing?.WriteLine( "Key \"{0}\" opened for {1}.", registryKey, sourceDescription );

            using ( registryKey )
            {
                try
                {
                    // Open the location compatible with PostSharp 3.0.
                    string licenseString = (string) registryKey.GetValue( "LicenseKey", null );

                    LicenseConfiguration licenseConfiguration = LicenseRegistrar.ParseLicenseString( licenseString, source, sourceDescription );

                    if ( licenseConfiguration == null )
                    {
                        LicensingTrace.Licensing?.WriteLine("Failed to parse license string \"{0}\" from \"{1}\\LicenseKey\".", licenseString, registryKey );
                    }
                    else
                    {
                        LicensingTrace.Licensing?.WriteLine("Adding license string \"{0}\" from \"{1}\\LicenseKey\".", licenseString, registryKey );
                        licenses.Add( licenseConfiguration );
                    }

                    // Open other locations.
                    IRegistryKey childIRegistryKey = registryKey.OpenSubKey( "LicenseKeys" );

                    if ( childIRegistryKey == null )
                        return licenses;

                    using ( childIRegistryKey )
                    {
                        // Load keys with no MinPostSharpVersion
                        foreach ( LicenseConfiguration license in GetUserLicensesFromIRegistryKey( childIRegistryKey, source, sourceDescription ) )
                        {
                            licenses.Add( license );
                        }

                        string[] versionDependentKeyNames = childIRegistryKey.GetSubKeyNames();

                        foreach ( string versionDependentKeyName in versionDependentKeyNames )
                        {
                            if ( !Version.TryParse( versionDependentKeyName, out _ ) )
                            {
                                continue;
                            }

                            // We don't check the minimal PostSharp version as in PostSharp 6.5.17+, 6.8.10+, 6.9.3+ and newer
                            // and Caravela, all license keys are backward compatible.
                            // - Licenses with unknown types and products fail validation and thus are not used.
                            // - Unknown license fields are skipped.

                            IRegistryKey versionDependentKey = childIRegistryKey.OpenSubKey( versionDependentKeyName );

                            if ( versionDependentKey == null )
                            {
                                continue;
                            }

                            using ( versionDependentKey )
                            {
                                foreach ( LicenseConfiguration license in GetUserLicensesFromIRegistryKey( versionDependentKey, source, sourceDescription ) )
                                {
                                    licenses.Add( license );
                                }
                            }
                        }
                    }

                    return licenses;
                }
                catch ( SystemException e )
                {
                    LicensingTrace.Licensing?.WriteLine( "Failed to work with {0}: {1}", sourceDescription, e );
                    return licenses;
                }
            }
        }

        private static IEnumerable<LicenseConfiguration> GetUserLicensesFromIRegistryKey(
            IRegistryKey IRegistryKey, LicenseSource source, string sourceDescription )
        {
            foreach ( string valueName in IRegistryKey.GetValueNames() )
            {
                string licenseString = IRegistryKey.GetValue( valueName, null ) as string;
                LicenseConfiguration licenseConfiguration = LicenseRegistrar.ParseLicenseString( licenseString, source, sourceDescription );

                if ( licenseConfiguration == null )
                {
                    LicensingTrace.Licensing?.WriteLine( "Failed to parse license string \"{0}\" from \"{1}\\{2}\".",
                                                        licenseString, IRegistryKey, valueName );
                }
                else
                {
                    LicensingTrace.Licensing?.WriteLine( "Adding license string \"{0}\" from \"{1}\\{2}\".",
                                                        licenseString, IRegistryKey, valueName );
                    yield return licenseConfiguration;
                }
            }
        }

        internal static void SetRegistryTimestamp( DateTime dateTime )
        {
            using ( IRegistryKey registryKey = UserSettings.OpenRegistryKey( false, true ) )
            {
                registryKey.SetValueDateTime( "LicenseTimestamp", dateTime );
            }
        }

        internal static DateTime GetRegistryTimestamp()
        {
            using ( IRegistryKey registryKey = UserSettings.OpenRegistryKey( false, true ) )
            {
                return registryKey.GetValueDateTime( "LicenseTimestamp", DateTime.MinValue );
            }
        }

        internal static void ClearLeaseCacheFromRegistry()
        {
            try
            {
                LicensingTrace.Licensing?.WriteLine( "Clearing license cache." );

                using ( IRegistryKey registryKey = UserSettings.OpenRegistryKey( writable: true ) )
                {
                    registryKey.DeleteSubKeyTree( leasedlicensesKeyName, false );
                }
            }
            catch ( SystemException e )
            {
                LicensingTrace.Licensing?.WriteLine( "Failed to clear license lease cache from regitry: {0}", e );
            }
        }

        // used for testing
        internal static IRegistryKey GetLicenseRegistryKey( bool allUsers, bool writable = true, Version minPostSharpVersion = null )
        {
            string subKey = "LicenseKeys";

            if ( minPostSharpVersion != null )
            {
                subKey = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", subKey, minPostSharpVersion.ToString( 3 ) );
            }

            return UserSettings.OpenRegistryKey( allUsers, writable, subKey );
        }

        internal static IRegistryKey GetLeasedLicenseRegistryKey( LicenseConfiguration licenseConfiguration )
        {
            return UserSettings.OpenRegistryKey( false, true, leasedlicensesKeyName + "\\" + licenseConfiguration.LicenseString );
        }

        internal static IRegistryKey GetProcessedTypesCountersKey()
        {
            return UserSettings.OpenRegistryKey( false, true, processedTypesCounterSubKey );
        }

        internal static void DeleteProcessedTypesCountersKey()
        {
            using ( IRegistryKey postSharpKey = UserSettings.OpenRegistryKey( false, true ) )
                postSharpKey.DeleteSubKey( processedTypesCounterSubKey, throwOnMissingSubKey: false );
        }
    }
}
