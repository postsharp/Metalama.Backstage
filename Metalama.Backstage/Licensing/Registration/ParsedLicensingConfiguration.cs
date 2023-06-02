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
        private readonly IServiceProvider _services;
        private readonly IConfigurationManager _configurationManager;
        private LicensingConfiguration _configuration = null!;

        private ParsedLicensingConfiguration( IServiceProvider services )
        {
            this._services = services;
            this._configurationManager = services.GetRequiredBackstageService<IConfigurationManager>();
            this._configurationManager.ConfigurationFileChanged += this.OnConfigurationFileChanged;

            this.ReadConfiguration();
        }

        private void ReadConfiguration()
        {
            var configurationManager = this._services.GetRequiredBackstageService<IConfigurationManager>();
            this._configuration = configurationManager.Get<LicensingConfiguration>();
            this.LicenseString = this._configuration.License;

            if ( string.IsNullOrWhiteSpace( this.LicenseString ) )
            {
                this.LicenseData = null;
                this.LicenseString = null;
            }
            else
            {
                var licenseFactory = new LicenseFactory( this._services );

                if ( licenseFactory.TryCreate( this.LicenseString, out var license, out _ ) )
                {
                    _ = license.TryGetLicenseRegistrationData( out var licenseRegistrationData, out _ );
                    this.LicenseData = licenseRegistrationData;
                }
                else
                {
                    this.LicenseData = null;
                }
            }
        }

        private void OnConfigurationFileChanged( ConfigurationFile file )
        {
            if ( file is LicensingConfiguration )
            {
                this.ReadConfiguration();
                this.Changed?.Invoke();
            }
        }

        public event Action? Changed;

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
            return new ParsedLicensingConfiguration( services );
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
            // The ConfigurationManager may trigger the OnConfigurationFileChanged event when saving.
            var licenseString = this.LicenseString;

            this._configurationManager
                .Update<LicensingConfiguration>( c => c with { License = licenseString } );
        }
    }
}