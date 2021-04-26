﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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

        private readonly List<ILicenseSource> _unusedLicenseSources = new();
        private readonly HashSet<string> _unusedLicenseStrings = new();
        private readonly HashSet<string> _usedLicenseStrings = new();

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

            this._unusedLicenseSources.AddRange( licenseSources );
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
                } while ( this.TryLoadNextLicenseString() );
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

            var licenseSource = this._unusedLicenseSources.First();
            this._unusedLicenseSources.Remove( licenseSource );

            foreach ( var licenseString in licenseSource.LicenseStrings )
            {
                this._unusedLicenseStrings.Add( licenseString );
            }

            return true;
        }

        private bool TryLoadNextLicenseString()
        {
            while ( this._unusedLicenseStrings.Count > 0 )
            {
                var licenseString = this._unusedLicenseStrings.First();
                this._unusedLicenseStrings.Remove( licenseString );
                this._usedLicenseStrings.Add( licenseString );

                if ( LicenseServerClient.IsLicenseServerUrl( licenseString ) )
                {
                    // TODO
                    throw new NotImplementedException();
                }
                else
                {
                    return this.TryLoadLicenseKey( licenseString );
                }
            }

            return false;
        }

        private bool TryLoadLicenseKey( string licenseKey )
        {
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
    }
}
