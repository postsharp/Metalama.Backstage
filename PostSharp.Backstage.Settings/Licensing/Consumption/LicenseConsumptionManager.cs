// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Extensibility.Extensions;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <inheritdoc />
    internal class LicenseConsumptionManager : ILicenseConsumptionManager
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        private readonly List<ILicenseSource> _unusedLicenseSources = new();
        private readonly HashSet<ILicense> _unusedLicenses = new();
        private readonly HashSet<ILicense> _usedLicenses = new();

        private readonly Dictionary<string, LicenseNamespaceConstraint> _namespaceLimitedLicensedFeatures = new();

        private LicensedFeatures _licensedFeatures;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseConsumptionManager"/> class.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <param name="licenseSources">License sources.</param>
        public LicenseConsumptionManager( IServiceProvider services, params ILicenseSource[] licenseSources )
            : this( services, (IEnumerable<ILicenseSource>) licenseSources ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseConsumptionManager"/> class.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <param name="licenseSources">License sources.</param>
        public LicenseConsumptionManager( IServiceProvider services, IEnumerable<ILicenseSource> licenseSources )
        {
            this._services = services;
            this._logger = services.GetLogger<LicensingLogCategory>();

            this._unusedLicenseSources.AddRange( licenseSources );
        }

        private bool TryLoadNextLicenseSource()
        {
            // TODO: tracing
            this._logger.Info?.Log( "TODO: tracing" );

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

            if ( !license.TryGetLicenseConsumptionData( out var licenseData ) )
            {
                return false;
            }

            if ( licenseData.LicensedNamespace == null )
            {
                this._licensedFeatures |= licenseData.LicensedFeatures;
            }
            else
            {
                if ( !this._namespaceLimitedLicensedFeatures.TryGetValue(
                        licenseData.LicensedNamespace,
                        out var namespaceFeatures ) )
                {
                    this._namespaceLimitedLicensedFeatures[licenseData.LicensedNamespace] =
                        new LicenseNamespaceConstraint(
                            licenseData.LicensedNamespace,
                            licenseData.LicensedFeatures );
                }
                else
                {
                    namespaceFeatures.LicensedFeatures |= licenseData.LicensedFeatures;
                }
            }

            return true;
        }

        private bool TryAutoRegisterLicense()
        {
            // TODO: trace

            if ( this._usedLicenses.Count != 0 )
            {
                return false;
            }

            var registrar = this._services.GetService<IFirstRunLicenseActivator>();

            if ( registrar == null || !registrar.TryRegisterLicense() )
            {
                return false;
            }

            // At this point, we would not get the newly registered license since all license sources have been enumerated.
            // Thus, we allow all features, which is what a valid evaluation license would do.
            // In the next compilation, the registered evaluation license would be retrieved from a license source.
            this._licensedFeatures |= LicensedFeatures.All;

            return true;
        }

        /// <inheritdoc />
        public bool CanConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures )
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
                             nsf => nsf.AllowsNamespace( consumer.TargetTypeNamespace )
                                    && nsf.LicensedFeatures.HasFlag( requiredFeatures ) ) )
                    {
                        return true;
                    }
                }
                while ( this.TryLoadNextLicense() );
            }
            while ( this.TryLoadNextLicenseSource() );

            if ( this.TryAutoRegisterLicense() )
            {
                return true;
            }

            return false;
        }

        // TODO: Improve messages

        /// <inheritdoc />
        public void ConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures )
        {
            if ( !this.CanConsumeFeatures( consumer, requiredFeatures ) )
            {
                if ( consumer.TargetTypeName == null )
                {
                    consumer.DiagnosticsSink.ReportError( $"No license available for feature(s) {requiredFeatures}" );
                }
                else
                {
                    consumer.DiagnosticsSink.ReportError(
                        $"No license available for feature(s) {requiredFeatures} required by '{consumer.TargetTypeName}' type.",
                        consumer.DiagnosticLocation );
                }
            }
        }
    }
}