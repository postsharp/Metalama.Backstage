// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Backstage.Licensing.Consumption;

/// <inheritdoc />
internal class LicenseConsumptionManager : ILicenseConsumptionManager
{
    private readonly ILogger _logger;
    private readonly List<ILicenseSource> _unusedLicenseSources = new();
    private readonly HashSet<ILicense> _unusedLicenses = new();

    // ReSharper disable once CollectionNeverQueried.Local (should be used in license audit).
    private readonly HashSet<ILicense> _usedLicenses = new();
    private readonly Dictionary<string, LicenseNamespaceConstraint> _namespaceLimitedLicensedFeatures = new();
    private readonly List<LicensingMessage> _warnings = new();

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
        this._logger = services.GetLoggerFactory().Licensing();

        this._unusedLicenseSources.AddRange( licenseSources );
    }

    private bool TryLoadNextLicenseSource( Action<LicensingMessage> reportMessage )
    {
        if ( this._unusedLicenseSources.Count == 0 )
        {
            return false;
        }

        var licenseSource = this._unusedLicenseSources.First();

        this._logger.Trace?.Log( $"Enumerating the license source {licenseSource}." );

        this._unusedLicenseSources.Remove( licenseSource );

        foreach ( var license in licenseSource.GetLicenses( reportMessage ) )
        {
            this._logger.Trace?.Log( $"{licenseSource} provided the license '{license}'." );
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

        this._logger.Trace?.Log( $"Loading the license '{license}'." );
        this._unusedLicenses.Remove( license );
        this._usedLicenses.Add( license );

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

    /// <inheritdoc />
    public bool CanConsumeFeatures( LicensedFeatures requiredFeatures, string? consumerNamespace )
    {
        void ReportWarning( LicensingMessage message )
        {
            this._warnings.Add( message );
            this._logger.Warning?.Log( message.Text );
        }

        do
        {
            do
            {
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
            }
            while ( this.TryLoadNextLicense() );
        }
        while ( this.TryLoadNextLicenseSource( ReportWarning ) );

        return false;
    }

    public IReadOnlyList<LicensingMessage> Messages => this._warnings;
}