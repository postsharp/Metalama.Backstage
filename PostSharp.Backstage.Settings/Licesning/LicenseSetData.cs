// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Settings;
using PostSharp.Backstage.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PostSharp.Backstage.Licensing
{
    internal class LicenseSetData
    {
        private readonly License[] licenses;
        private readonly string hash;
        private readonly string path;
        public readonly DateTime LastWriteTime;
        private readonly DateTime lastVerificationTime;
        private readonly LicenseFeatureContainer combinedFeatures = new LicenseFeatureContainer();

        public LicenseSetData( License[] licenses, string path, string hash )
            : this( licenses, path, hash, DateTime.MinValue, DateTime.MinValue )
        {

        }

        private LicenseSetData( License[] licenses, string path, string hash, DateTime lastWriteTime, DateTime lastVerificationTime )
        {
            this.hash = hash;
            this.licenses = licenses;
            this.lastVerificationTime = lastVerificationTime;
            this.path = path;
            this.LastWriteTime = lastWriteTime;

            foreach ( License license in licenses )
            {
                this.combinedFeatures.Add( license );
            }
        }

        public static LicenseSetData GetCachedLicenseSetDataFromHash( License[] licenses, string hash )
        {
            try
            {
                IRegistryKey registryKey = OpenCacheRegistryKey( hash, true );

                if ( registryKey == null )
                {
                    return null;
                }

                using ( registryKey )
                {
                    LicensingTrace.Licensing?.WriteLine( "License with hash '{0}' found in signature cache.", hash );


                    // Get the time when the signature was last verified.
                    DateTime? lastVerificationTime = registryKey.GetValueDateTime( "LastVerificationTime" );
                    if ( !lastVerificationTime.HasValue || lastVerificationTime.Value > UserSettings.GetCurrentDateTime() )
                    {
                        LicensingTrace.Licensing?.WriteLine( "Signature cache is invalid: last verification time ({0}) is invalid.", lastVerificationTime );
                        return null;
                    }

                    return new LicenseSetData( licenses, null, hash, DateTime.MinValue, lastVerificationTime.Value );
                }
            }
            catch ( SystemException e )
            {
                LicensingTrace.Licensing?.WriteLine( "Failed to work with license signature cache in registry: {0}", e );
                return null;
            }
        }

        public static LicenseSetData GetLicenseSetDataFromPath( License[] licenses, string path )
        {
            DateTime lastWriteTime = new FileInfo( path ).LastWriteTime;
            string hash = GetHash( GetCanonicalPath( path ) );

            try
            {
                IRegistryKey registryKey = OpenCacheRegistryKey( hash, true );

                if ( registryKey == null )
                    goto noCache;

                using ( registryKey )
                {
                    LicensingTrace.Licensing?.WriteLine( "License of '{0}' found in signature cache.", path );

                    // Check that the cache entry is for the same file.
                    string cachedPath = registryKey.GetValue( "Path" ) as string;
                    if ( cachedPath == null || !string.Equals( GetCanonicalPath( path ), GetCanonicalPath( cachedPath ), StringComparison.Ordinal ) )
                    {
                        LicensingTrace.Licensing?.WriteLine( "Signature cache is invalid: path does not match." );
                        goto noCache;
                    }

                    // Check that the cache entry is still valid.
                    DateTime? cachedLastWriteTime = registryKey.GetValueDateTime( "LastWriteTime" );
                    if ( !cachedLastWriteTime.HasValue || cachedLastWriteTime.Value != lastWriteTime )
                    {
                        LicensingTrace.Licensing?.WriteLine( "Signature cache is invalid: last write time does not match (cached={0}; actual={1}).",
                                                            cachedLastWriteTime,
                                                            lastWriteTime );
                        goto noCache;
                    }

                    // Get the time when the signature was last verified.
                    DateTime? lastVerificationTime = registryKey.GetValueDateTime( "LastVerificationTime" );
                    if ( !lastVerificationTime.HasValue || lastVerificationTime.Value > UserSettings.GetCurrentDateTime() )
                    {
                        LicensingTrace.Licensing?.WriteLine( "Signature cache is invalid: last verification time ({0}) is invalid.", lastVerificationTime );
                        goto noCache;
                    }

                    return new LicenseSetData( licenses, path, hash, cachedLastWriteTime.Value,
                                               lastVerificationTime.Value );
                }
            }
            catch ( SystemException e )
            {
                LicensingTrace.Licensing?.WriteLine( "Failed to get license set data from \"{0}\": {1}", path, e );
            }

            noCache:
            return new LicenseSetData( licenses, path, hash, lastWriteTime, DateTime.MinValue );
        }

        private void SetVerificationResult( bool valid )
        {
            try
            {
                IRegistryKey registryKey = OpenCacheRegistryKey( this.hash, true );

                if ( valid )
                {
                    LicensingTrace.Licensing?.WriteLine( "Adding '{0}' to signature cache.", this.path ?? this.hash );

                    if ( registryKey == null )
                        return;

                    using ( registryKey )
                    {
                        if ( this.path != null )
                        {
                            registryKey.SetValue( "Path", this.path );
                            registryKey.SetValueDateTime( "LastWriteTime", this.LastWriteTime );
                        }
                        registryKey.SetValueDateTime( "LastVerificationTime", UserSettings.GetCurrentDateTime() );
                    }
                }
                else
                {
                    if ( registryKey != null )
                    {
                        registryKey.Close();
                        LicensingTrace.Licensing?.WriteLine( "Removing '{0}' from signature cache.", this.path );

                        DeleteCacheRegistryKey( this.hash );
                    }
                }
            }
            catch ( SystemException e )
            {
                LicensingTrace.Licensing?.WriteLine( "Failed to set license verification result: {0}", e );
            }
        }

        public bool Verify( byte[] publicKeyToken, IDiagnosticsSink diagnosticsSink, List<string> errors = null )
        {
            if ( UserSettings.GetCurrentDateTime().Subtract( this.lastVerificationTime ).TotalHours <= 2 && (new Random()).NextDouble() >= 0.01 )
            {
                LicensingTrace.Licensing?.WriteLine( "Skipping signature verification since it has been verified recently." );
                return true;
            }
            else
            {
                LicensingTrace.Licensing?.WriteLine( "Verifying license signature." );
                bool valid = true;

                foreach ( License license in this.licenses )
                {
                    string errorDescription;
                    if ( !license.Validate( publicKeyToken, out errorDescription ) )
                    {
                        valid = false;

                        if ( diagnosticsSink != null )
                        {
                            diagnosticsSink.ReportError( $"License error. The license {license.LicenseUniqueId} in file '{this.path}' is invalid: {errorDescription}" ); // PS0146
                        }

                        if ( errors != null )
                        {
                            errors.Add( errorDescription );
                        }
                    }
                }
                
                this.SetVerificationResult( valid );
                return valid;
            }
        }


        private static IRegistryKey OpenCacheRegistryKey( string hash, bool writable )
        {
            return UserSettings.OpenRegistryKey( false, writable, string.Format(CultureInfo.InvariantCulture, "LicenseCache\\{0}", hash ) );
        }

        private static void DeleteCacheRegistryKey( string hash )
        {
            using ( IRegistryKey registryKey = UserSettings.OpenRegistryKey( false, true, "LicenseCache" ) )
            {
                registryKey.DeleteSubKey( hash, false );
            }
        }

        public static string GetCanonicalPath( string path )
        {
            return Path.GetFullPath( path ).ToLowerInvariant();
        }

        public static string GetHash( string s )
        {
            byte[] hash;
            using ( MD5Managed md5 = new MD5Managed() )
            {
                hash = md5.ComputeHash( Encoding.UTF8.GetBytes( s.Normalize() ) );
            }

            StringBuilder stringBuilder = new StringBuilder( hash.Length*2 );
            foreach ( byte t in hash )
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", t );
            }

            return stringBuilder.ToString();
        }

        public IReadOnlyLicenseFeatureContainer GetFilteredLicenseFeatures( string assemblyName, LicenseType[] requiredLicenseTypes )
        {
            if ( assemblyName == null && requiredLicenseTypes == null )
            {
                return this.combinedFeatures;
            }
            else
            {
                LicenseFeatureContainer grantedLicenseFeature = new LicenseFeatureContainer();
                foreach ( License license in this.licenses )
                {
                    if ( assemblyName != null && !license.AllowsNamespace( assemblyName ) )
                    {
                        continue;
                    }

                    if ( requiredLicenseTypes != null && Array.IndexOf( requiredLicenseTypes, (LicenseType) license.LicenseType ) < 0 )
                    {
                        continue;
                    }

                    grantedLicenseFeature.Add( license );
                }

                return grantedLicenseFeature;
            }
        }
    }
}
