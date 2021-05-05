// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Community
{
    public class CommunityLicenseRegistrar
    {
        private readonly IServiceProvider _services;
        private readonly ITrace _trace;

        public CommunityLicenseRegistrar( IServiceProvider services, ITrace trace )
        {
            this._services = services;
            this._trace = trace;
        }

        public bool TryRegisterLicense()
        {
            void TraceFailure( string message )
            {
                this._trace.WriteLine( "Failed to register community license: {0}", message );
            }

            this._trace.WriteLine( "Registering community license." );

            try
            {
                var userStorage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, this._services, this._trace );

                if ( userStorage.Licenses.Values.Any( l => l != null && l.LicenseType == LicenseType.Community ) )
                {
                    TraceFailure( "A community license is registered already." );
                    return true;
                }

                var factory = new SelfSignedLicenseFactory( this._services );
                (var licenseKey, var data) = factory.CreateCommunityLicense();

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
