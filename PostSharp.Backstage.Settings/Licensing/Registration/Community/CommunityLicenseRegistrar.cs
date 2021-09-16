﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using System;
using System.Linq;

namespace PostSharp.Backstage.Licensing.Registration.Community
{
    /// <summary>
    /// Registers a community license.
    /// </summary>
    public class CommunityLicenseRegistrar
    {
        private readonly IServiceProvider _services;
        private readonly IStandardLicenseFileLocations _licenseFiles;
        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunityLicenseRegistrar"/> class.
        /// </summary>
        /// <param name="services">Service provider.</param>
        public CommunityLicenseRegistrar( IServiceProvider services )
        {
            this._services = services;
            this._licenseFiles = services.GetRequiredService<IStandardLicenseFileLocations>();
            this._logger = services.GetOptionalTraceLogger<CommunityLicenseRegistrar>();
        }

        /// <summary>
        /// Attempts to register a community license.
        /// </summary>
        /// <returns>
        /// A value indicating whether the license has been registered.
        /// Success is indicated when a new community license is registered
        /// as well as when an existing community license is registered already.
        /// </returns>
        public bool TryRegisterLicense()
        {
            void TraceFailure( string message )
            {
                this._logger?.LogTrace( $"Failed to register community license: {message}" );
            }

            this._logger?.LogTrace( "Registering community license." );

            try
            {
                var userStorage = LicenseFileStorage.OpenOrCreate( this._licenseFiles.UserLicenseFile, this._services );

                if ( userStorage.Licenses.Values.Any( l => l != null && l.LicenseType == LicenseType.Community ) )
                {
                    TraceFailure( "A community license is registered already." );

                    return true;
                }

                var factory = new UnsignedLicenseFactory( this._services );
                var (licenseKey, data) = factory.CreateCommunityLicense();

                userStorage.AddLicense( licenseKey, data );
                userStorage.Save();
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