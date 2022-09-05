// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Backstage.Licensing.Registration
{
    /// <summary>
    /// Manages license file for license registration purposes.
    /// </summary>
    public class ParsedLicensingConfiguration
    {
        private readonly IServiceProvider _services;
        private readonly LicensingConfiguration _configuration;

        private readonly List<(string LicenseString, LicenseRegistrationData? LicenseData)> _licenses = new();

        public DateTime? LastEvaluationStartDate
        {
            get => this._configuration.LastEvaluationStartDate;
            set => this._configuration.ConfigurationManager.Update<LicensingConfiguration>( c => c.LastEvaluationStartDate = value );
        }

        /// <summary>
        /// Gets licenses contained in the storage.
        /// </summary>
        /// <remarks>
        /// Includes both the licenses loaded from a license file and licenses pending to be stored.
        /// </remarks>
        public IReadOnlyList<(string LicenseString, LicenseRegistrationData? LicenseData)> Licenses => this._licenses;

        public bool TryGetRegistrationData( string licenseKey, out LicenseRegistrationData? registrationData )
        {
            var item = this._licenses.FirstOrDefault( l => l.LicenseString == licenseKey );

            if ( item.LicenseString != null! )
            {
                registrationData = item.LicenseData;

                return true;
            }
            else
            {
                registrationData = null;

                return false;
            }
        }

        /// <summary>
        /// Creates an empty storage.
        /// </summary>
        /// <returns>The empty storage.</returns>
        public static ParsedLicensingConfiguration CreateEmpty( IServiceProvider services )
        {
            var storage = new ParsedLicensingConfiguration( new LicensingConfiguration(), services );

            return storage;
        }

        /// <summary>
        /// Opens a license file or creates empty storage if the license file doesn't exist.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <returns></returns>
        public static ParsedLicensingConfiguration OpenOrCreate( IServiceProvider services )
        {
            var licensingConfiguration = services.GetRequiredBackstageService<IConfigurationManager>().Get<LicensingConfiguration>();

            var storage = new ParsedLicensingConfiguration( licensingConfiguration, services );
            var factory = new LicenseFactory( services );

            foreach ( var licenseString in licensingConfiguration.Licenses )
            {
                if ( string.IsNullOrWhiteSpace( licenseString ) )
                {
                    continue;
                }

                LicenseRegistrationData? data = null;

                if ( factory.TryCreate( licenseString, out var license ) )
                {
                    _ = license.TryGetLicenseRegistrationData( out data );
                }

                storage._licenses.Add( (licenseString, data) );
            }

            return storage;
        }

        private ParsedLicensingConfiguration( LicensingConfiguration configuration, IServiceProvider services )
        {
            this._services = services;
            this._configuration = configuration;
        }

        /// <summary>
        /// Adds a license.
        /// </summary>
        /// <param name="licenseString">String representing the license to be stored in the license file.</param>
        /// <param name="data">Data represented by the <paramref name="licenseString"/>.</param>
        public void AddLicense( string licenseString, LicenseRegistrationData data )
        {
            this._licenses.Add( (licenseString, data) );
            this.Save();
        }

        /// <summary>
        /// Removes a license.
        /// </summary>
        /// <param name="licenseString">String representing the license to be removed.</param>
        public bool RemoveLicense( string licenseString )
        {
            if ( this._licenses.RemoveAll( l => l.LicenseString == licenseString ) > 0 )
            {
                this.Save();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes the licenses to a file.
        /// </summary>
        /// <remarks>
        /// Overwrites existing file.
        /// </remarks>
        private void Save()
        {
            this._services.GetRequiredBackstageService<IConfigurationManager>()
                .Update<LicensingConfiguration>( c => c.Licenses = this._licenses.Select( l => l.LicenseString ).ToArray() );
        }
    }
}