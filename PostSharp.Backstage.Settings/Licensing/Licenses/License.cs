// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Utilities;

namespace PostSharp.Backstage.Licensing.Licenses
{

    /// <exclude />
    /// <summary>
    /// Encapsulates a PostSharp license.
    /// </summary>
    [Serializable]
    public class License : ILicense
    {
        private readonly string _licenseKey;

        private readonly IServiceProvider _services;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDiagnosticsSink _diagnostics;
        private readonly ITrace _trace;

        /// <summary>
        /// Removes invalid characters from a license string.
        /// </summary>
        /// <param name="licenseKey">A license string.</param>
        /// <returns>The license string in canonical format.</returns>
        public static string CleanLicenseKey( string licenseKey )
        {
            var stringBuilder = new StringBuilder( licenseKey.Length );

            // Remove all spaces from the license.
            foreach ( var c in licenseKey )
            {
                if ( char.IsLetterOrDigit( c ) || c == '-' )
                {
                    stringBuilder.Append( c );
                }
            }

            return stringBuilder.ToString().ToUpperInvariant();
        }

        internal License( string licenseKey, IServiceProvider services, ITrace trace )
        {
            this._licenseKey = CleanLicenseKey( licenseKey );
            this._services = services;
            this._dateTimeProvider = services.GetService<IDateTimeProvider>();
            this._diagnostics = services.GetService<IDiagnosticsSink>();
            this._trace = trace;
        }

        public static bool TryGetLicenseId( string s, [MaybeNullWhen( returnValue: false )] out int id )
        {
            var firstDash = s.IndexOf( '-' );

            if ( firstDash > 0 )
            {
                var prefix = s.Substring( 0, firstDash );

                if ( int.TryParse( prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out id ) )
                {
                    return true;
                }
                else
                {
                    try
                    {
                        var guidBytes = Base32.FromBase32String( prefix );
                        if ( guidBytes != null && guidBytes.Length == 16 )
                        {
                            id = 0;
                            return false;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            id = -1;
            return false;
        }

        public bool TryGetLicenseConsumptionData( [MaybeNullWhen( returnValue: false )] out LicenseConsumptionData licenseConsumptionData )
        {
            if ( !this.TryGetLicenseKeyData( out var licenseKeyData ) )
            {
                licenseConsumptionData = null;
                return false;
            }

            var applicationInfoService = this._services.GetService<IApplicationInfoService>();

            if ( !licenseKeyData.Validate( null, this._dateTimeProvider, applicationInfoService.BuildDate, applicationInfoService.Version, out var errorDescription ) )
            {
                this._diagnostics.ReportWarning( $"License key {licenseKeyData.LicenseUniqueId} is invalid: {errorDescription}" );
                licenseConsumptionData = null;
                return false;
            }

            licenseConsumptionData = licenseKeyData.ToConsumptionData();
            return true;
        }

        public bool TryGetLicenseRegistrationData( [MaybeNullWhen( returnValue: false )] out LicenseRegistrationData licenseRegistrationData )
        {
            if ( !this.TryGetLicenseKeyData( out var licenseKeyData ) )
            {
                licenseRegistrationData = null;
                return false;
            }

            licenseRegistrationData = licenseKeyData.ToRegistrationData();
            return true;
        }

        internal bool TryGetLicenseKeyData( [MaybeNullWhen( returnValue: false )] out LicenseKeyData data )
        {
            this._trace.WriteLine( "Deserializing license {{{0}}}.", this._licenseKey );
            Guid? licenseGuid = null;

            try
            {
                // Parse the license key prefix.
                var firstDash = this._licenseKey.IndexOf( '-' );

                if ( firstDash < 0 )
                {
                    throw new InvalidLicenseException( $"License header not found for license {{{this._licenseKey}}}." );
                }

                var prefix = this._licenseKey.Substring( 0, firstDash );

                if ( !int.TryParse( prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out var licenseId ) )
                {
                    // If this is not an integer, this may be a GUID.
                    licenseGuid = new Guid( Base32.FromBase32String( prefix ) );
                }

                var licenseBytes = Base32.FromBase32String( this._licenseKey.Substring( firstDash + 1 ) );
                var memoryStream = new MemoryStream( licenseBytes );
                using ( var reader = new BinaryReader( memoryStream ) )
                {
                    data = LicenseKeyData.Deserialize( reader );

                    if ( data.LicenseId != licenseId )
                    {
                        throw new InvalidLicenseException( $"The license id in the body ({licenseId}) does not match the header for license {{{this._licenseKey}}}." );
                    }

                    data.LicenseGuid = licenseGuid;
                    data.LicenseString = this._licenseKey;

                    this._trace.WriteLine( "Deserialized license: {0}", data.ToString() );
                    return true;
                }
            }
            catch ( Exception e )
            {
                if ( TryGetLicenseId( this._licenseKey, out var id ) )
                {
                    this._diagnostics.ReportWarning( $"Cannot parse license key ID {id}: {e.Message}" );
                }
                else
                {
                    this._diagnostics.ReportWarning( $"Cannot parse license key {this._licenseKey}: {e.Message}" );
                }

                this._trace.WriteLine( "Cannot parse the license {{{0}}}: {1}", this._licenseKey, e );
                data = null;
                return false;
            }
        }

        public override bool Equals( object? obj )
        {
            return obj is License license &&
                   this._licenseKey == license._licenseKey;
        }

        public override int GetHashCode()
        {
            return 668981160 + EqualityComparer<string>.Default.GetHashCode( this._licenseKey );
        }

        public override string ToString()
        {
            return $"License '{this._licenseKey}'";
        }
    }
}