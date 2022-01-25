// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

namespace Metalama.Backstage.Licensing.Licenses
{
    /// <summary>
    /// Represents a license serialized in a license key.
    /// </summary>
    public class License : ILicense
    {
        private readonly string _licenseKey;

        private readonly IServiceProvider _services;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger _logger;

        private static string CleanLicenseKey( string licenseKey )
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

        /// <summary>
        /// Initializes a new instance of the <see cref="License"/> class.
        /// </summary>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="services">Services.</param>
        internal License( string licenseKey, IServiceProvider services )
        {
            this._licenseKey = CleanLicenseKey( licenseKey );
            this._services = services;
            this._dateTimeProvider = services.GetRequiredService<IDateTimeProvider>();
            this._logger = services.GetLoggerFactory().Licensing();
        }

        private static bool TryGetLicenseId( string s, out int id )
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

                    // ReSharper disable once EmptyGeneralCatchClause
                    catch { }
                }
            }

            id = -1;

            return false;
        }

        /// <inheritdoc />
        public bool TryGetLicenseConsumptionData( [MaybeNullWhen( false )] out LicenseConsumptionData licenseConsumptionData )
        {
            if ( !this.TryGetAndValidateLicenseKeyData( out var licenseKeyData ) )
            {
                licenseConsumptionData = null;

                return false;
            }

            licenseConsumptionData = licenseKeyData.ToConsumptionData();

            return true;
        }

        /// <inheritdoc />
        public bool TryGetLicenseRegistrationData( [MaybeNullWhen( false )] out LicenseRegistrationData licenseRegistrationData )
        {
            if ( !this.TryGetLicenseKeyData( out var licenseKeyData ) )
            {
                licenseRegistrationData = null;

                return false;
            }

            licenseRegistrationData = licenseKeyData.ToRegistrationData();

            return true;
        }

        private bool TryGetLicenseKeyData( [MaybeNullWhen( false )] out LicenseKeyData data )
        {
            this._logger.Trace?.Log( $"Deserializing license '{this._licenseKey}'." );
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

                using ( var memoryStream = new MemoryStream( licenseBytes ) )
                using ( var reader = new BinaryReader( memoryStream ) )
                {
                    data = LicenseKeyData.Deserialize( reader );

                    if ( data.LicenseId != licenseId )
                    {
                        throw new InvalidLicenseException(
                            $"The license id in the body ({licenseId}) does not match the header for license {{{this._licenseKey}}}." );
                    }

                    data.LicenseGuid = licenseGuid;
                    data.LicenseString = this._licenseKey;

                    this._logger.Trace?.Log( $"Deserialized license: {data}" );

                    return true;
                }
            }
            catch ( Exception e )
            {
                if ( TryGetLicenseId( this._licenseKey, out var id ) )
                {
                    this._logger.Error?.Log( $"Cannot parse license key ID {id}: {e.Message}" );
                }
                else
                {
                    this._logger.Error?.Log( $"Cannot parse license key {this._licenseKey}: {e.Message}" );
                }

                this._logger.Trace?.Log( $"Cannot parse the license {{{this._licenseKey}}}: {e}" );
                data = null;

                return false;
            }
        }

        private bool TryGetAndValidateLicenseKeyData( [MaybeNullWhen( false )] out LicenseKeyData data )
        {
            if ( !this.TryGetLicenseKeyData( out data ) )
            {
                return false;
            }

            var applicationInfoService = this._services.GetRequiredService<IApplicationInfo>();

            if ( !data.Validate(
                    null,
                    this._dateTimeProvider,
                    applicationInfoService,
                    out var errorDescription ) )
            {
                this._logger.Error?.Log( $"License key {data.LicenseUniqueId} is invalid: {errorDescription}" );
                data = null;

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool Equals( object? obj ) => obj is License license && this._licenseKey == license._licenseKey;

        /// <inheritdoc />
        public override int GetHashCode() => 668981160 + EqualityComparer<string>.Default.GetHashCode( this._licenseKey );

        /// <inheritdoc />
        public override string ToString() => this._licenseKey;
    }
}