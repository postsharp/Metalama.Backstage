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
        private readonly string? path;
        public readonly DateTime LastWriteTime;
        private readonly DateTime lastVerificationTime;
        private readonly LicenseFeatureContainer combinedFeatures = new LicenseFeatureContainer();

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly LicenseCache _cache;
        private readonly IDiagnosticsSink? _diagnosticsSink;
        private readonly ITrace? _licensingTrace;

        public LicenseSetData( License[] licenses, string path, string hash, LicenseCache cache, IDateTimeProvider dateTimeProvider, IDiagnosticsSink? diagnosticsSink, ITrace? licensingTrace )
            : this( licenses, path, hash, DateTime.MinValue, DateTime.MinValue, cache, dateTimeProvider, diagnosticsSink, licensingTrace )
        {
        }

        private LicenseSetData( License[] licenses, string? path, string hash, DateTime lastWriteTime, DateTime lastVerificationTime, LicenseCache cache, IDateTimeProvider dateTimeProvider, IDiagnosticsSink? diagnosticsSink, ITrace? licensingTrace )
        {
            this.hash = hash;
            this.licenses = licenses;
            this.lastVerificationTime = lastVerificationTime;
            this.path = path;
            this.LastWriteTime = lastWriteTime;

            this._dateTimeProvider = dateTimeProvider;
            this._cache = cache;
            this._diagnosticsSink = diagnosticsSink;
            this._licensingTrace = licensingTrace;

            foreach ( License license in licenses )
            {
                this.combinedFeatures.Add( license );
            }
        }

        public static bool TryGetCachedLicenseSetDataFromHash( License[] licenses, string hash, LicenseCache cache, IDiagnosticsSink diagnosticsSink, ITrace licensingTrace, out LicenseSetData? licenseSetData )
        {
            if ( cache.IsValidHash( hash, out var lastVerificationTime, out var dateTimeProvider ) )
            {
                licenseSetData = new( licenses, null, hash, DateTime.MinValue, lastVerificationTime, cache, dateTimeProvider, diagnosticsSink, licensingTrace );
                return true;
            }
            else
            {
                licenseSetData = null;
                return false;
            }
        }

        public static LicenseSetData GetLicenseSetDataFromPath( License[] licenses, string path, LicenseCache cache, IDiagnosticsSink diagnosticsSink, ITrace licensingTrace )
        {
            if ( cache.IsValidPath( path, out var hash, out var lastWriteTime, out var lastVerificationTime, out var dateTimeProvider ) )
            {
                return new( licenses, path, hash, lastWriteTime, lastVerificationTime, cache, dateTimeProvider, diagnosticsSink, licensingTrace );
            }
            else
            {
                return new( licenses, path, hash, lastWriteTime, DateTime.MinValue, cache, dateTimeProvider, diagnosticsSink, licensingTrace );
            }
        }

        private void SetVerificationResult( bool valid )
        {
            if ( this.path == null )
            {
                this._cache.SetVerificationResult( this.hash, valid );
            }
            else
            {
                this._cache.SetVerificationResult( this.hash, valid, this.path, this.LastWriteTime );
            }
        }

        public bool Verify( byte[] publicKeyToken, List<string>? errors = null )
        {
            if ( this._dateTimeProvider.GetCurrentDateTime().Subtract( this.lastVerificationTime ).TotalHours <= 2 && new Random().NextDouble() >= 0.01 )
            {
                this._licensingTrace?.WriteLine( "Skipping signature verification since it has been verified recently." );
                return true;
            }
            else
            {
                this._licensingTrace?.WriteLine( "Verifying license signature." );
                bool valid = true;

                foreach ( License license in this.licenses )
                {
                    string errorDescription;
                    if ( !license.Validate( publicKeyToken, out errorDescription ) )
                    {
                        valid = false;

                        this._diagnosticsSink?.ReportError( $"License error. The license {license.LicenseUniqueId} in file '{this.path}' is invalid: {errorDescription}" ); // PS0146

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
