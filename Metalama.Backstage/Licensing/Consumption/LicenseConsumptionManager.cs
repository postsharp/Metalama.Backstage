// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption.Sources;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Backstage.Licensing.Consumption;

/// <inheritdoc />
internal class LicenseConsumptionManager : ILicenseConsumptionManager
{
    private readonly ILogger _logger;
    private readonly object _initializationLock = new object();
    private readonly IEnumerable<ILicenseSource> _licenseSources;
    private readonly Dictionary<string, LicenseNamespaceConstraint> _namespaceLimitedLicensedFeatures = new();
    private readonly List<LicensingMessage> _warnings = new();

    private bool _initialized;
    private ImmutableList<string>? _redistributionLicenseKeys;
    private LicensedFeatures _licensedFeatures;
    private int _maxAspectsCount;

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
        this._logger = services.GetLoggerFactory().Licensing();
        this._licenseSources = licenseSources;
    }

    private void EnsureIsInitialized()
    {
        if ( this._initialized )
        {
            return;
        }

        lock ( this._initializationLock )
        {
            if ( this._initialized )
            {
                return;
            }

            void ReportWarning( LicensingMessage message )
            {
                this._warnings.Add( message );
                this._logger.Warning?.Log( message.Text );
            }

            var redistributionLicenseKeys = new List<string>();

            foreach ( var licenseSource in this._licenseSources )
            {
                this._logger.Trace?.Log( $"Enumerating the license source {licenseSource}." );

                foreach ( var license in licenseSource.GetLicenses( ReportWarning ) )
                {
                    this._logger.Trace?.Log( $"{licenseSource} provided the license '{license}'." );

                    this._logger.Trace?.Log( $"Loading the license '{license}'." );

                    // TODO: license audit

                    if ( !license.TryGetLicenseConsumptionData( out var licenseData ) )
                    {
                        continue;
                    }

                    if ( licenseData.LicensedNamespace == null )
                    {
                        this._licensedFeatures |= licenseData.LicensedFeatures;
                        this._maxAspectsCount |= Math.Max( this._maxAspectsCount, licenseData.MaxAspectsCount );
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
                                    licenseData.LicensedFeatures,
                                    licenseData.MaxAspectsCount);
                        }
                        else
                        {
                            namespaceFeatures.LicensedFeatures |= licenseData.LicensedFeatures;
                            namespaceFeatures.MaxApsectsCount = Math.Max( namespaceFeatures.MaxApsectsCount, licenseData.MaxAspectsCount );
                        }
                    }

                    if ( licenseSource.IsRedistributionLicenseSource && licenseData.IsRedistributable && licenseData.LicenseString != null )
                    {
                        redistributionLicenseKeys.Add( licenseData.LicenseString );
                    }
                }
            }

            this._redistributionLicenseKeys = redistributionLicenseKeys.ToImmutableList();
            this._initialized = true;
        }
    }

    /// <inheritdoc />
    public bool CanConsumeFeatures( LicensedFeatures requiredFeatures, string? consumerNamespace )
    {
        this.EnsureIsInitialized();

        if ( this._licensedFeatures.HasFlag( requiredFeatures ) )
        {
            return true;
        }

        if ( !string.IsNullOrEmpty( consumerNamespace )
                && this._namespaceLimitedLicensedFeatures.Count > 0
                && this._namespaceLimitedLicensedFeatures.Values.Any(
                    nsf => nsf.AllowsNamespace( consumerNamespace )
                        && nsf.LicensedFeatures.HasFlag( requiredFeatures ) ) )
        {
            return true;
        }

        return false;
    }

    public IEnumerable<string> GetRedistributionLicenseKeys()
    {
        this.EnsureIsInitialized();
        return this._redistributionLicenseKeys!;
    }

    public int GetMaxAspectsCount( string? consumerNamespace = null )
    {
        this.EnsureIsInitialized();

        return string.IsNullOrEmpty( consumerNamespace )
            ? this._maxAspectsCount
            : this._namespaceLimitedLicensedFeatures.Values
            .Where( nsf => nsf.AllowsNamespace( consumerNamespace ) )
            .Max( nsf => nsf.MaxApsectsCount );
    }

    public IReadOnlyList<LicensingMessage> Messages => this._warnings;
}