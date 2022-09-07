// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using System;
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
            set => this._configuration.ConfigurationManager.Update<LicensingConfiguration>( c => c.LastEvaluationStartDate = value );
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
            var configurationManager = services.GetRequiredBackstageService<IConfigurationManager>();
            var licensingConfiguration = configurationManager.Get<LicensingConfiguration>();

            var storage = new ParsedLicensingConfiguration( licensingConfiguration, services );
            var factory = new LicenseFactory( services );

            // We no longer support multiple licenses. Strip aditional licenses if the have been added before. (There is probably no such case.)
            if ( licensingConfiguration.Licenses.Length > 1 )
            {
                configurationManager.Update<LicensingConfiguration>( c => c.Licenses = new[] { licensingConfiguration.Licenses.First() } );
                licensingConfiguration = configurationManager.Get<LicensingConfiguration>();
            }

            var licenseString = licensingConfiguration.Licenses.FirstOrDefault();

            if ( string.IsNullOrWhiteSpace( licenseString ) )
            {
                return storage;
            }

            LicenseRegistrationData? data = null;

            if ( factory.TryCreate( licenseString, out var license ) )
            {
                _ = license.TryGetLicenseRegistrationData( out data );
            }

            storage.LicenseString = licenseString;
            storage.LicenseData = data;

            return storage;
        }

        private ParsedLicensingConfiguration( LicensingConfiguration configuration, IServiceProvider services )
        {
            this._services = services;
            this._configuration = configuration;
        }

        /// <summary>
        /// Stores a license.
        /// </summary>
        /// <param name="licenseString">String representing the license to be stored in the license file.</param>
        /// <param name="data">Data represented by the <paramref name="licenseString"/>.</param>
        public void StoreLicense( string licenseString, LicenseRegistrationData data )
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
            this._services.GetRequiredBackstageService<IConfigurationManager>()
                .Update<LicensingConfiguration>( c => c.Licenses = this.LicenseString == null ? Array.Empty<string>() : new[] { this.LicenseString } );
        }
    }
}