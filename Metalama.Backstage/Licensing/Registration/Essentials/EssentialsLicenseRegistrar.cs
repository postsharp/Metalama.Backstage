// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Licenses;
using System;
using System.Linq;

namespace Metalama.Backstage.Licensing.Registration.Essentials
{
    /// <summary>
    /// Registers a Essentials license.
    /// </summary>
    public class EssentialsLicenseRegistrar
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EssentialsLicenseRegistrar"/> class.
        /// </summary>
        /// <param name="services">Service provider.</param>
        public EssentialsLicenseRegistrar( IServiceProvider services )
        {
            this._services = services;
            this._logger = services.GetLoggerFactory().Licensing();
        }

        /// <summary>
        /// Attempts to register a Essentials license.
        /// </summary>
        /// <returns>
        /// A value indicating whether the license has been registered.
        /// Success is indicated when a new Essentials license is registered
        /// as well as when an existing Essentials license is registered already.
        /// </returns>
        public bool TryRegisterLicense()
        {
            void TraceFailure( string message )
            {
                this._logger.Trace?.Log( $"Failed to register Essentials license: {message}" );
            }

            this._logger.Trace?.Log( "Registering Essentials license." );

            try
            {
                var userStorage = ParsedLicensingConfiguration.OpenOrCreate( this._services );

                if ( userStorage.Licenses.Any( l => l.LicenseData is { LicenseType: LicenseType.Essentials } ) )
                {
                    TraceFailure( "A Essentials license is registered already." );

                    return true;
                }

                var factory = new UnsignedLicenseFactory( this._services );
                var (licenseKey, data) = factory.CreateEssentialsLicense();

                userStorage.AddLicense( licenseKey, data );
            }
            catch ( Exception e )
            {
                TraceFailure( e.ToString() );

                return false;
            }

            return true;
        }
    }
}