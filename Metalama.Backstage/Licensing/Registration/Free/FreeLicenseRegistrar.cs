// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Licenses;
using System;

namespace Metalama.Backstage.Licensing.Registration.Free
{
    /// <summary>
    /// Registers an unsigned Metalama Free.
    /// </summary>
    public class FreeLicenseRegistrar
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeLicenseRegistrar"/> class.
        /// </summary>
        /// <param name="services">Service provider.</param>
        public FreeLicenseRegistrar( IServiceProvider services )
        {
            this._services = services;
            this._logger = services.GetLoggerFactory().Licensing();
        }

        /// <summary>
        /// Attempts to register an unsigned Metalama Free license.
        /// </summary>
        /// <returns>
        /// A value indicating whether the license has been registered.
        /// Success is indicated when a new Metalama Free license is registered
        /// as well as when an existing Metalama Free license is registered already.
        /// </returns>
        public bool TryRegisterLicense()
        {
            void TraceFailure( string message )
            {
                this._logger.Trace?.Log( $"Failed to register Metalama Free license: {message}" );
            }

            this._logger.Trace?.Log( "Registering Metalama Free license." );

            try
            {
                var userStorage = ParsedLicensingConfiguration.OpenOrCreate( this._services );

                if ( userStorage.LicenseData is { Product: LicensedProduct.MetalamaFree } )
                {
                    TraceFailure( "A Metalama Free license is registered already." );

                    return true;
                }

                var factory = new UnsignedLicenseFactory( this._services );
                var (licenseKey, data) = factory.CreateFreeLicense();

                userStorage.StoreLicense( licenseKey, data );
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