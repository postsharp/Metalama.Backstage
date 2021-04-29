// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Sources;

namespace PostSharp.Backstage.Licensing.Consumption
{
    public sealed class LicenseConsumptionManager : ILicenseConsumptionManager
    {
        private readonly IApplicationInfoService _applicationInfoService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITrace _trace;

        private readonly List<ILicenseSource> _unusedLicenseSources = new();
        private readonly HashSet<ILicense> _unusedLicenses = new();
        private readonly HashSet<ILicense> _usedLicenses = new();

        private readonly Dictionary<string, NamespaceLimitedLicenseFeatures> _namespaceLimitedLicensedFeatures = new();

        private LicensedFeatures _licensedFeatures;

        public LicenseConsumptionManager( IServiceProvider services, ITrace trace, params ILicenseSource[] licenseSources )
            : this( services, trace, (IEnumerable<ILicenseSource>) licenseSources )
        {
        }

        public LicenseConsumptionManager( IServiceProvider services, ITrace trace, IEnumerable<ILicenseSource> licenseSources )
        {
            this._trace = trace;

            this._applicationInfoService = services.GetService<IApplicationInfoService>();
            this._dateTimeProvider = services.GetService<IDateTimeProvider>();

            this._unusedLicenseSources.AddRange( licenseSources );
        }

        private bool TryLoadNextLicenseSource()
        {
            if ( this._unusedLicenseSources.Count == 0 )
            {
                return false;
            }

            var licenseSource = this._unusedLicenseSources.First();
            this._unusedLicenseSources.Remove( licenseSource );

            foreach ( var license in licenseSource.GetLicenses() )
            {
                this._unusedLicenses.Add( license );
            }

            return true;
        }

        private bool TryLoadNextLicense()
        {
            if ( this._unusedLicenses.Count == 0 )
            {
                return false;
            }

            var license = this._unusedLicenses.First();
            this._unusedLicenses.Remove( license );
            this._usedLicenses.Add( license );

            // TODO: trace
            // TODO: license audit

            if ( !license.TryGetLicenseData( out var licenseData ) )
            {
                return false;
            }

            if ( licenseData.LicensedNamespace == null )
            {
                this._licensedFeatures |= licenseData.LicensedFeatures;
            }
            else
            {
                if ( !this._namespaceLimitedLicensedFeatures.TryGetValue( licenseData.LicensedNamespace, out var namespaceFeatures ) )
                {
                    this._namespaceLimitedLicensedFeatures[licenseData.LicensedNamespace] = new( licenseData.LicensedNamespace, licenseData.LicensedFeatures );
                }
                else
                {
                    namespaceFeatures.Features |= licenseData.LicensedFeatures;
                }
            }

            return true;
        }

        public bool CanConsumeFeature( ILicenseConsumer consumer, LicensedFeatures requiredFeatures )
        {
            do
            {
                do
                {
                    if ( this._licensedFeatures.HasFlag( requiredFeatures ) )
                    {
                        return true;
                    }

                    if ( this._namespaceLimitedLicensedFeatures.Count > 0
                        && this._namespaceLimitedLicensedFeatures.Values.Any(
                            nsf => nsf.Constraint.AllowsNamespace( consumer.TargetTypeNamespace )
                            && nsf.Features.HasFlag( requiredFeatures ) ) )
                    {
                        return true;
                    }
                }
                while ( this.TryLoadNextLicense() );
            }
            while ( this.TryLoadNextLicenseSource() );

            return false;
        }

        public void ConsumeFeature( ILicenseConsumer consumer, LicensedFeatures requiredFeatures )
        {
            if ( !this.CanConsumeFeature( consumer, requiredFeatures ) )
            {
                // TODO
                throw new NotImplementedException();
            }
        }
    }
}
