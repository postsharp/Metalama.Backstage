// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Extensibility.Extensions;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

namespace PostSharp.Backstage.Licensing.Licenses
{
    /// <summary>
    /// Represents a license serialized in a license key.
    /// </summary>
    public class License : ILicense
    {
        private readonly string _licenseKey;

        private readonly IServiceProvider _services;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDiagnosticsSink _diagnostics;
        private readonly ILogger? _logger;

        private static string CleanLicenseKey( string licenseKey )
        {
            var stringBuilder = new StringBuilder( licenseKey.Length );

            // Remove all spaces from the license.
            foreach (var c in licenseKey)
            {
                if (char.IsLetterOrDigit( c ) || c == '-')
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
            _licenseKey = CleanLicenseKey( licenseKey );
            _services = services;
            _dateTimeProvider = services.GetRequiredService<IDateTimeProvider>();
            _diagnostics = services.GetRequiredService<IDiagnosticsSink>();
            _logger = services.GetOptionalTraceLogger<License>();
        }

        private static bool TryGetLicenseId( string s, out int id )
        {
            var firstDash = s.IndexOf( '-' );

            if (firstDash > 0)
            {
                var prefix = s.Substring( 0, firstDash );

                if (int.TryParse( prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out id ))
                {
                    return true;
                }
                else
                {
                    try
                    {
                        var guidBytes = Base32.FromBase32String( prefix );

                        if (guidBytes != null && guidBytes.Length == 16)
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
            if (!TryGetLicenseKeyData( out var licenseKeyData ))
            {
                licenseConsumptionData = null;

                return false;
            }

            var applicationInfoService = _services.GetRequiredService<IApplicationInfo>();

            if (!licenseKeyData.Validate(
                null,
                _dateTimeProvider,
                applicationInfoService,
                out var errorDescription ))
            {
                _diagnostics.ReportWarning( $"License key {licenseKeyData.LicenseUniqueId} is invalid: {errorDescription}" );
                licenseConsumptionData = null;

                return false;
            }

            licenseConsumptionData = licenseKeyData.ToConsumptionData();

            return true;
        }

        /// <inheritdoc />
        public bool TryGetLicenseRegistrationData( [MaybeNullWhen( false )] out LicenseRegistrationData licenseRegistrationData )
        {
            if (!TryGetLicenseKeyData( out var licenseKeyData ))
            {
                licenseRegistrationData = null;

                return false;
            }

            licenseRegistrationData = licenseKeyData.ToRegistrationData();

            return true;
        }

        private bool TryGetLicenseKeyData( [MaybeNullWhen( false )] out LicenseKeyData data )
        {
            _logger?.LogTrace( $"Deserializing license {{{_licenseKey}}}." );
            Guid? licenseGuid = null;

            try
            {
                // Parse the license key prefix.
                var firstDash = _licenseKey.IndexOf( '-' );

                if (firstDash < 0)
                {
                    throw new InvalidLicenseException( $"License header not found for license {{{_licenseKey}}}." );
                }

                var prefix = _licenseKey.Substring( 0, firstDash );

                if (!int.TryParse( prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out var licenseId ))
                {
                    // If this is not an integer, this may be a GUID.
                    licenseGuid = new Guid( Base32.FromBase32String( prefix ) );
                }

                var licenseBytes = Base32.FromBase32String( _licenseKey.Substring( firstDash + 1 ) );
                var memoryStream = new MemoryStream( licenseBytes );

                using (var reader = new BinaryReader( memoryStream ))
                {
                    data = LicenseKeyData.Deserialize( reader );

                    if (data.LicenseId != licenseId)
                    {
                        throw new InvalidLicenseException(
                            $"The license id in the body ({licenseId}) does not match the header for license {{{_licenseKey}}}." );
                    }

                    data.LicenseGuid = licenseGuid;
                    data.LicenseString = _licenseKey;

                    _logger?.LogTrace( $"Deserialized license: {data}" );

                    return true;
                }
            }
            catch (Exception e)
            {
                if (TryGetLicenseId( _licenseKey, out var id ))
                {
                    _diagnostics.ReportWarning( $"Cannot parse license key ID {id}: {e.Message}" );
                }
                else
                {
                    _diagnostics.ReportWarning( $"Cannot parse license key {_licenseKey}: {e.Message}" );
                }

                _logger?.LogTrace( $"Cannot parse the license {{{_licenseKey}}}: {e}" );
                data = null;

                return false;
            }
        }

        /// <inheritdoc />
        public override bool Equals( object? obj )
        {
            return obj is License license &&
                   _licenseKey == license._licenseKey;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return 668981160 + EqualityComparer<string>.Default.GetHashCode( _licenseKey );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"License '{_licenseKey}'";
        }
    }
}