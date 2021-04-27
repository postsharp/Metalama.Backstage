// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
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
        
        private readonly IApplicationInfoService _applicationInfoService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDiagnosticsSink _diagnostics;
        private readonly ITrace _trace;

        internal License( string licenseKey, IServiceProvider services, ITrace trace )
        {
            this._licenseKey = licenseKey;
            this._applicationInfoService = services.GetService<IApplicationInfoService>();
            this._dateTimeProvider = services.GetService<IDateTimeProvider>();
            this._diagnostics = services.GetService<IDiagnosticsSink>();
            this._trace = trace;
        }

        public static bool TryGetLicenseId( string s, [MaybeNullWhen( returnValue: false )] out int id )
        {
            int firstDash = s.IndexOf( '-' );

            string prefix = s.Substring( 0, firstDash );
            if ( firstDash > 0 && int.TryParse( prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out id ) )
            {
                return true;
            }
            else
            {
                try
                {
                    byte[] guidBytes = Base32.FromBase32String( prefix );
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

            id = -1;
            return false;
        }

        public bool TryGetLicenseData( [MaybeNullWhen( returnValue: false )] out LicenseData licenseData )
        {
            if ( !this.TryGetLicenseKeyData( out var licenseKeyData ) )
            {
                licenseData = null;
                return false;
            }

            if ( !licenseKeyData.Validate( null, this._dateTimeProvider, this._applicationInfoService.BuildDate, this._applicationInfoService.Version, out var errorDescription ) )
            {
                this._diagnostics.ReportWarning( $"License key {licenseKeyData.LicenseUniqueId} is invalid: {errorDescription}" );
                licenseData = null;
                return false;
            }

            licenseData = new(
                licensedProduct: licenseKeyData.Product,
                licenseType: licenseKeyData.LicenseType,
                licensedFeatures: licenseKeyData.LicensedFeatures,
                licensedNamespace: licenseKeyData.Namespace,
                displayName: $"{licenseKeyData.ProductName} license id {licenseKeyData.LicenseUniqueId}" );

            return true;
        }

        internal bool TryGetLicenseKeyData( [MaybeNullWhen( returnValue: false )] out LicenseKeyData data )
        {
            this._trace.WriteLine( "Deserializing license {{{0}}}.", this._licenseKey );
            Guid? licenseGuid = null;

            try
            {
                // Parse the license key prefix.
                int firstDash = this._licenseKey.IndexOf( '-' );
                if ( firstDash < 0 )
                {
                    this._trace.WriteLine( "License header not found for license {{{0}}}.", this._licenseKey );
                    data = null;
                    return false;
                }

                string prefix = this._licenseKey.Substring( 0, firstDash );

                if ( !int.TryParse( prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out int licenseId ) )
                {
                    // If this is not an integer, this may be a GUID.
                    licenseGuid = new Guid( Base32.FromBase32String( prefix ) );
                }

                byte[] licenseBytes = Base32.FromBase32String( this._licenseKey.Substring( firstDash + 1 ) );
                MemoryStream memoryStream = new MemoryStream( licenseBytes );
                using ( BinaryReader reader = new BinaryReader( memoryStream ) )
                {
                    data = LicenseKeyData.Deserialize( reader );

                    if ( data.LicenseId != licenseId )
                    {
                        throw new InvalidLicenseException(
                            string.Format(CultureInfo.InvariantCulture, "The license id in the body ({0}) does not match the header for license {{{1}}}.",
                                           licenseId,
                                           this._licenseKey ) );
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
    }
}