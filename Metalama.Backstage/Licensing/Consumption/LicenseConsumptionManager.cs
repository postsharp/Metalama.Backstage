// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Backstage.Licensing.Consumption;

/// <inheritdoc />
internal class LicenseConsumptionManager : ILicenseConsumptionManager
{
    private readonly ILogger _logger;
    private readonly object _initializationLock = new object();
    private readonly IEnumerable<ILicenseSource> _licenseSources;
    private readonly Dictionary<string, LicenseNamespaceConstraint> _namespaceLimitedConstraints = new();
    private readonly List<LicensingMessage> _messages = new();
    private readonly LicenseFactory _licenseFactory;
    private readonly HashSet<string> _namespaceUnlimitedRedistributionLicenseKeys = new();
    private readonly Dictionary<string, LicenseNamespaceConstraint> _redistributionLicenseKeyNamespaceConstraints = new();

    private bool _initialized;
    private ImmutableList<string>? _redistributionLicenseKeys;
    private LicensedFeatures _allLicensedFeatures;
    private LicensedFeatures _namespaceUnlimitedLicensedFeatures;
    private int _namespaceUnlimitedMaxAspectsCount;

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
        this._licenseFactory = new( services );
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

            void ReportMessage( LicensingMessage message )
            {
                this._messages.Add( message );

                if ( message.IsError )
                {
                    this._logger.Error?.Log( message.Text );
                }
                else
                {
                    this._logger.Warning?.Log( message.Text );
                }
            }

            var redistributionLicenseKeys = new List<string>();

            foreach ( var licenseSource in this._licenseSources )
            {
                this._logger.Trace?.Log( $"Enumerating the license source {licenseSource}." );

                foreach ( var license in licenseSource.GetLicenses( ReportMessage ) )
                {
                    this._logger.Trace?.Log( $"{licenseSource} provided the license '{license}'." );

                    this._logger.Trace?.Log( $"Loading the license '{license}'." );

                    // TODO: license audit

                    if ( !license.TryGetLicenseConsumptionData( out var licenseData ) )
                    {
                        continue;
                    }

                    this._allLicensedFeatures |= licenseData.LicensedFeatures;

                    if ( licenseData.LicensedNamespace == null )
                    {
                        this._namespaceUnlimitedLicensedFeatures |= licenseData.LicensedFeatures;
                        this._namespaceUnlimitedMaxAspectsCount = Math.Max( this._namespaceUnlimitedMaxAspectsCount, licenseData.MaxAspectsCount );
                    }
                    else
                    {
                        if ( !this._namespaceLimitedConstraints.TryGetValue(
                                licenseData.LicensedNamespace,
                                out var namespaceFeatures ) )
                        {
                            this._namespaceLimitedConstraints[licenseData.LicensedNamespace] =
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
    public bool CanConsumeFeatures( LicensedFeatures requiredFeatures, string? consumerNamespace = null )
    {
        this.EnsureIsInitialized();

        if ( this._namespaceUnlimitedLicensedFeatures.HasFlag( requiredFeatures ) )
        {
            // The feature is available regardless of the feature namespace limitation.
            return true;
        }
        else if ( string.IsNullOrEmpty( consumerNamespace ) && this._allLicensedFeatures.HasFlag( requiredFeatures ) )
        {
            // This is a global feature, so we look at all licenses, regardless of the license namespace limitation.
            return true;
        }
        else if ( !string.IsNullOrEmpty( consumerNamespace )
                && this._namespaceLimitedConstraints.Count > 0
                && this._namespaceLimitedConstraints.Values.Any(
                    nsf => nsf.AllowsNamespace( consumerNamespace )
                        && nsf.LicensedFeatures.HasFlag( requiredFeatures ) ) )
        {
            // The feature is available for the given namespace.
            return true;
        }

        // The feature is not available.
        return false;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetRedistributionLicenseKeys()
    {
        this.EnsureIsInitialized();
        return this._redistributionLicenseKeys!;
    }

    /// <inheritdoc />
    public int GetNamespaceUnlimitedMaxAspectsCount()
    {
        this.EnsureIsInitialized();

        return this._namespaceUnlimitedMaxAspectsCount;
    }

    /// <inheritdoc />
    public bool TryGetNamespaceLimitedMaxAspectsCount( string consumerNamespace, out int maxAspectsCount, [NotNullWhen( true )] out string? licensedNamespace )
    {
        this.EnsureIsInitialized();

        if ( string.IsNullOrEmpty( consumerNamespace ) )
        {
            throw new ArgumentOutOfRangeException( nameof( consumerNamespace ), "The consumer namespace has not been specified." );
        }

        var allowingConstraint = this._namespaceLimitedConstraints.Values
            .FirstOrDefault( nsf => nsf.AllowsNamespace( consumerNamespace ) );

        if ( allowingConstraint == null )
        {
            maxAspectsCount = 0;
            licensedNamespace = null;
            return false;
        }
        else
        {
            maxAspectsCount = allowingConstraint.MaxApsectsCount;
            licensedNamespace = allowingConstraint.AllowedNamespace;
            return true;
        }
    }

    /// <inheritdoc />
    public bool ValidateRedistributionLicenseKey( string redistributionLicenseKey, string aspectClassNamespace )
    {
        if ( this._namespaceUnlimitedRedistributionLicenseKeys.Contains( redistributionLicenseKey ) )
        {
            return true;
        }

        if ( !this._redistributionLicenseKeyNamespaceConstraints.TryGetValue( redistributionLicenseKey, out var constraints ) )
        {
            if ( !this._licenseFactory.TryCreate( redistributionLicenseKey, out var license ) )
            {
                return false;
            }

            if ( !license.TryGetLicenseConsumptionData( out var licenseConsumptionData ) )
            {
                return false;
            }

            if ( !licenseConsumptionData.IsRedistributable )
            {
                return false;
            }

            if ( licenseConsumptionData.LicensedNamespace == null )
            {
                this._namespaceUnlimitedRedistributionLicenseKeys.Add( redistributionLicenseKey );
                return true;
            }

            constraints = new( licenseConsumptionData.LicensedNamespace, licenseConsumptionData.LicensedFeatures, licenseConsumptionData.MaxAspectsCount );
        }

        return constraints.AllowsNamespace( aspectClassNamespace );
    }

    /// <inheritdoc />
    public IEnumerable<(string LicensedNamespace, int MaxAspectsCount)> GetNamespaceLimitedMaxAspectCounts()
    {
        return this._namespaceLimitedConstraints.Values.Select( c => (c.AllowedNamespace, c.MaxApsectsCount) );
    }

    /// <inheritdoc />
    public IReadOnlyList<LicensingMessage> Messages => this._messages;
}