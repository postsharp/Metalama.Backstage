// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Extensibility.Extensions;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <inheritdoc />
    internal class LicenseConsumptionManager : ILicenseConsumptionManager
    {
        private readonly IServiceProvider _services;
        private readonly ILogger? _logger;

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
            : this( services, (IEnumerable<ILicenseSource>)licenseSources ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseConsumptionManager"/> class.
        /// </summary>
        /// <param name="services">Services.</param>
        /// <param name="licenseSources">License sources.</param>
        public LicenseConsumptionManager( IServiceProvider services, IEnumerable<ILicenseSource> licenseSources )
        {
            _services = services;
            _logger = services.GetOptionalTraceLogger<LicenseConsumptionManager>();

            _unusedLicenseSources.AddRange( licenseSources );
        }

        private bool TryLoadNextLicenseSource()
        {
            // TODO: tracing
            _logger?.LogInformation( "TODO: tracing" );

            if (_unusedLicenseSources.Count == 0)
            {
                return false;
            }

            var licenseSource = _unusedLicenseSources.First();
            _unusedLicenseSources.Remove( licenseSource );

            foreach (var license in licenseSource.GetLicenses())
            {
                _unusedLicenses.Add( license );
            }

            return true;
        }

        private bool TryLoadNextLicense()
        {
            if (_unusedLicenses.Count == 0)
            {
                return false;
            }

            var license = _unusedLicenses.First();
            _unusedLicenses.Remove( license );
            _usedLicenses.Add( license );

            // TODO: trace
            // TODO: license audit

            if (!license.TryGetLicenseConsumptionData( out var licenseData ))
            {
                return false;
            }

            if (licenseData.LicensedNamespace == null)
            {
                _licensedFeatures |= licenseData.LicensedFeatures;
            }
            else
            {
                if (!_namespaceLimitedLicensedFeatures.TryGetValue( licenseData.LicensedNamespace, out var namespaceFeatures ))
                {
                    _namespaceLimitedLicensedFeatures[licenseData.LicensedNamespace] = new LicenseNamespaceConstraint(
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

            if (_usedLicenses.Count != 0)
            {
                return false;
            }

            var registrar = _services.GetService<IFirstRunLicenseActivator>();

            if (registrar == null || !registrar.TryRegisterLicense())
            {
                return false;
            }

            // At this point, we would not get the newly registered license since all license sources have been enumerated.
            // Thus, we allow all features, which is what a valid evaluation license would do.
            // In the next compilation, the registered evaluation license would be retrieved from a license source.
            _licensedFeatures |= LicensedFeatures.All;

            return true;
        }

        /// <inheritdoc />
        public bool CanConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures )
        {
            do
            {
                do
                {
                    if (_licensedFeatures.HasFlag( requiredFeatures ))
                    {
                        return true;
                    }

                    if (_namespaceLimitedLicensedFeatures.Count > 0
                        && _namespaceLimitedLicensedFeatures.Values.Any(
                            nsf => nsf.AllowsNamespace( consumer.TargetTypeNamespace )
                                   && nsf.LicensedFeatures.HasFlag( requiredFeatures ) ))
                    {
                        return true;
                    }
                }
                while (TryLoadNextLicense());
            }
            while (TryLoadNextLicenseSource());

            if (TryAutoRegisterLicense())
            {
                return true;
            }

            return false;
        }

        // TODO: Improve messages
        /// <inheritdoc />
        public void ConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures )
        {
            if (!CanConsumeFeatures( consumer, requiredFeatures ))
            {
                if (consumer.TargetTypeName == null)
                {
                    consumer.Diagnostics.ReportError( $"No license available for feature(s) {requiredFeatures}" );
                }
                else
                {
                    consumer.Diagnostics.ReportError(
                        $"No license available for feature(s) {requiredFeatures} required by '{consumer.TargetTypeName}' type.",
                        consumer.DiagnosticsLocation );
                }
            }
        }
    }
}