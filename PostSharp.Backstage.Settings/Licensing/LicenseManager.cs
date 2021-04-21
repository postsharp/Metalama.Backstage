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

        private readonly HashSet<string> _unusedLicenseKeys = new();
        private readonly HashSet<string> _usedLicenseKeys = new();

        private readonly Dictionary<LicensedProduct, LicensedPackages> _availableRequirements = new();

        private bool _userLicensesLoaded;

        public LicenseManager(IEnumerable<string> licenseKeys, IServiceLocator services, ITrace trace )
        {
            this._trace = trace;
            this._applicationInfoService = services.GetService<IApplicationInfoService>();
            this._dateTimeProvider = services.GetService<IDateTimeProvider>();

            foreach ( var licenseKey in licenseKeys )
            {
                this._unusedLicenseKeys.Add( licenseKey );
            }
        }

        public bool IsRequirementSatisfied( LicensedProduct product, LicensedPackages package )
        {
            do
            {
                if ( this._availableRequirements.TryGetValue( product, out var elligiblePackages ) )
                {
                    if (elligiblePackages.HasFlag(package))
                    {
                        return true;
                    }
                }

                while (this._unusedLicenseKeys.Count > 0)
                {
                    if (this.TryLoadNextLicenseKey())
                    {
                        break;
                    }
                }
            } while ( this._unusedLicenseKeys.Count > 0 );

            if ( !this._userLicensesLoaded )
            {
                this.LoadUserLicenseKeys();
                return this.IsRequirementSatisfied( product, package );
            }

            return false;
        }

        public void Require( LicensedProduct product, LicensedPackages package )
        {
            throw new NotImplementedException();
        }

        private void LoadUserLicenseKeys()
        {
            this._userLicensesLoaded = true;
            throw new NotImplementedException();
        }

        private bool TryLoadNextLicenseKey()
        {
            var licenseKey = this._unusedLicenseKeys.First();
            this._unusedLicenseKeys.Remove( licenseKey );
            this._usedLicenseKeys.Add( licenseKey );

            if ( !License.TryDeserialize( licenseKey, this._applicationInfoService, out var license, this._trace ) )
            {
                return false;
            }

            if (!license.Validate(null, this._dateTimeProvider, out var _errorDescription))
            {
                // TODO: trace invalid licenses
                return false;
            }

            // TODO: trace
            LicensedPackages availablePackages = this._availableRequirements[license.Product];
            availablePackages |= license.GetLicensedPackages();
            this._availableRequirements[license.Product] = availablePackages;

            return true;
        }
    }
}
