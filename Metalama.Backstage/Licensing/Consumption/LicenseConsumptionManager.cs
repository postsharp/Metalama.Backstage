// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption;

/// <inheritdoc />
internal class LicenseConsumptionManager : ILicenseConsumptionManager
{
    private readonly ILogger _logger;
    private readonly List<LicensingMessage> _messages = new();
    private readonly LicenseFactory _licenseFactory;
    private readonly Dictionary<string, NamespaceLicenseInfo> _embeddedRedistributionLicensesCache = new();
    private readonly LicenseRequirement _licensedRequirement;
    private readonly NamespaceLicenseInfo? _licensedNamespace;

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

        this._logger = services.GetLoggerFactory().Licensing();
        this._licenseFactory = new( services );

        foreach ( var source in licenseSources )
        {
            var license = source.GetLicense( ReportMessage );

            if ( license == null )
            {
                this._logger.Info?.Log( $"'{source.GetType().Name}' license source provided no license." );
                continue;
            }

            if (!license.TryGetLicenseConsumptionData( out var data ) )
            {
                var licenseUniqueId = license.TryGetLicenseRegistrationData( out var registrationData ) ? registrationData.UniqueId : "<invalid>";
                this._logger.Info?.Log( $"License '{licenseUniqueId}' provided by '{source.GetType().Name}' license source is invalid." );
                continue;
            }
            
            this._licensedRequirement = data.LicensedRequirement;
            this._licensedNamespace = string.IsNullOrEmpty( data.LicensedNamespace ) ? null : new( data.LicensedNamespace! );
            this.MaxAspectsCount = data.MaxAspectsCount;
            this.RedistributionLicenseKey = data.IsRedistributable ? data.LicenseString : null;
            return;
        }
    }

    /// <inheritdoc />
    public bool CanConsume( LicenseRequirement requirement, string? consumerNamespace = null )
    {
        if ( string.IsNullOrEmpty( consumerNamespace ) // This is a global requirement.
            || (this._licensedNamespace != null && !this._licensedNamespace.AllowsNamespace( consumerNamespace )) )
        {
            this._logger.Error?.Log( $"Consumer namespace '{consumerNamespace}' is not licensed." );
            return false;
        }

        if ( !this._licensedRequirement.HasFlag( requirement ) )
        {
            this._logger.Error?.Log( $"License requirement '{requirement}' is not licensed." );
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool ValidateRedistributionLicenseKey( string redistributionLicenseKey, string aspectClassNamespace )
    {
        if ( !this._embeddedRedistributionLicensesCache.TryGetValue( redistributionLicenseKey, out var licensedNamespace ) )
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

            if ( string.IsNullOrEmpty( licenseConsumptionData.LicensedNamespace ) )
            {
                return false;
            }

            licensedNamespace = new( licenseConsumptionData.LicensedNamespace! );
        }

        return licensedNamespace.AllowsNamespace( aspectClassNamespace );
    }

    /// <inheritdoc />
    public int MaxAspectsCount { get; }

    /// <inheritdoc />
    public string? RedistributionLicenseKey { get; }

    /// <inheritdoc />
    public IReadOnlyList<LicensingMessage> Messages => this._messages;
}