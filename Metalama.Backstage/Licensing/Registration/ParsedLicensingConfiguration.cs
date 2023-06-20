// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using System;

namespace Metalama.Backstage.Licensing.Registration
{
    /// <summary>
    /// Manages license file for license registration purposes.
    /// </summary>
    public class ParsedLicensingConfiguration
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly LicensingConfiguration _configuration;

        private ParsedLicensingConfiguration( LicensingConfiguration configuration, IServiceProvider services )
        {
            this._configurationManager = services.GetRequiredBackstageService<IConfigurationManager>();
            this._configuration = configuration;
        }

        /// <summary>
        /// Gets the license contained in the storage or to be stored to the storage.
        /// </summary>
        public string? LicenseString { get; private set; }

        /// <summary>
        /// Gets the registration data of the license contained in the storage or to be stored to the storage.
        /// </summary>
        public LicenseRegistrationData? LicenseData { get; private set; }

        public DateTime? LastEvaluationStartDate
        {
            get => this._configuration.LastEvaluationStartDate;
            set => this._configurationManager.Update<LicensingConfiguration>( c => c with { LastEvaluationStartDate = value } );
        }

        /// <summary>
        /// Opens a license file or creates empty storage if the license file doesn't exist.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <returns></returns>
        public static ParsedLicensingConfiguration OpenOrCreate( IServiceProvider services )
        {
            var configurationManager = services.GetRequiredBackstageService<IConfigurationManager>();
            var licensingConfiguration = configurationManager.Get<LicensingConfiguration>();
            var licenseString = licensingConfiguration.License;

            LicenseRegistrationData? licenseRegistrationData = null;
            
            if ( !string.IsNullOrWhiteSpace( licenseString ) )
            {
                var licenseFactory = new LicenseFactory( services );

                if ( licenseFactory.TryCreate( licenseString, out var license, out _ ) )
                {
                    _ = license.TryGetLicenseRegistrationData( out licenseRegistrationData, out _ );
                }
            }

            var parsedLicensingConfiguration = new ParsedLicensingConfiguration( licensingConfiguration, services )
            {
                LicenseString = licenseString, LicenseData = licenseRegistrationData
            };

            return parsedLicensingConfiguration;
        }

        /// <summary>
        /// Stores a license.
        /// </summary>
        /// <param name="licenseString">String representing the license to be stored in the license file.</param>
        /// <param name="data">Data represented by the <paramref name="licenseString"/>.</param>
        public void SetLicense( string licenseString, LicenseRegistrationData data )
        {
            this.LicenseString = licenseString;
            this.LicenseData = data;
            this.Save();
        }

        /// <summary>
        /// Removes the stored license.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a license has been removed, <c>false</c> if there was no license to be removed.
        /// </returns>
        public bool RemoveLicense()
        {
            if ( this.LicenseString != null )
            {
                this.LicenseString = null;
                this.LicenseData = null;
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
            this._configurationManager
                .Update<LicensingConfiguration>( c => c with { License = this.LicenseString } );
        }
    }
}