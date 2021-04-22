// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing
{
    public sealed class LicenseManager : ILicenseManager
    {
        private readonly IApplicationInfoService _applicationInfoService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITrace _trace;

        private readonly Dictionary<string, ILicenseSource> _unusedLicenseSources = new Dictionary<string, ILicenseSource>();

        private readonly HashSet<string> _unusedLicenseKeys = new();
        private readonly HashSet<string> _usedLicenseKeys = new();

        private LicensedPackages _licensedPackages;

        public LicenseManager( IServiceLocator services, ITrace trace, params ILicenseSource[] licenseSources )
            : this( services, trace, (IEnumerable<ILicenseSource>) licenseSources )
        {
        }

        public LicenseManager( IServiceLocator services, ITrace trace, IEnumerable<ILicenseSource> licenseSources )
        {
            this._trace = trace;
            this._applicationInfoService = services.GetService<IApplicationInfoService>();
            this._dateTimeProvider = services.GetService<IDateTimeProvider>();

            foreach ( var licenseSource in licenseSources )
            {
                this._unusedLicenseSources.Add( licenseSource.Id, licenseSource );
            }
        }

        public bool IsRequirementSatisfied( LicensedPackages packages )
        {
            do
            {
                do
                {
                    if ( this._licensedPackages.HasFlag( packages ) )
                    {
                        return true;
                    }
                } while ( this.TryLoadNextLicenseKey() );
            } while ( this.TryLoadNextLicenseSource() );

            return false;
        }

        public void Require( LicensedPackages packages )
        {
            if ( !this.IsRequirementSatisfied( packages ) )
            {
                // TODO
                throw new NotImplementedException();
            }
        }

        private bool TryLoadNextLicenseSource()
        {
            if ( this._unusedLicenseSources.Count == 0 )
            {
                return false;
            }

            var licenseSource = this._unusedLicenseSources.Values.First();
            this._unusedLicenseSources.Remove( licenseSource.Id );

            foreach ( var licenseKey in licenseSource.LicenseKeys )
            {
                this._unusedLicenseKeys.Add( licenseKey );
            }

            return true;
        }

        private bool TryLoadNextLicenseKey()
        {
            while ( this._unusedLicenseKeys.Count > 0 )
            {
                var licenseKey = this._unusedLicenseKeys.First();
                this._unusedLicenseKeys.Remove( licenseKey );
                this._usedLicenseKeys.Add( licenseKey );

                if ( !License.TryDeserialize( licenseKey, this._applicationInfoService, out var license, this._trace ) )
                {
                    return false;
                }

                if ( !license.Validate( null, this._dateTimeProvider, out var _errorDescription ) )
                {
                    // TODO: trace invalid licenses
                    return false;
                }

                // TODO: trace
                // TODO: license audit
                this._licensedPackages |= license.GetLicensedPackages();

                return true;
            }

            return false;
        }
    }
}
