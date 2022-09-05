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
    private readonly LicenseConsumptionData? _license;
    private readonly NamespaceLicenseInfo? _licensedNamespace;

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseConsumptionManager"/> class.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <param name="licenseSources">License sources.</param>
    public LicenseConsumptionManager( IServiceProvider services, params ILicenseSource[] licenseSources )
        : this( services, (IEnumerable<ILicenseSource>) licenseSources ) { }

    private void ReportMessage( LicensingMessage message )
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

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseConsumptionManager"/> class.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <param name="licenseSources">License sources.</param>
    public LicenseConsumptionManager( IServiceProvider services, IEnumerable<ILicenseSource> licenseSources )
    {
        this._logger = services.GetLoggerFactory().Licensing();
        this._licenseFactory = new( services );

        foreach ( var source in licenseSources )
        {
            var license = source.GetLicense( this.ReportMessage );

            if ( license == null )
            {
                this._logger.Info?.Log( $"'{source.GetType().Name}' license source provided no license." );
                continue;
            }

            if ( !license.TryGetLicenseConsumptionData( out var data ) )
            {
                var licenseUniqueId = license.TryGetLicenseRegistrationData( out var registrationData ) ? registrationData.UniqueId : "<invalid>";
                this._logger.Info?.Log( $"License '{licenseUniqueId}' provided by '{source.GetType().Name}' license source is invalid." );
                continue;
            }

            this._license = data;
            this._licensedNamespace = string.IsNullOrEmpty( data.LicensedNamespace ) ? null : new( data.LicensedNamespace! );
            this.RedistributionLicenseKey = data.IsRedistributable ? data.LicenseString : null;
            return;
        }
    }

    /// <inheritdoc />
    public bool CanConsume( LicenseRequirement requirement, string? consumerNamespace = null )
    {
        if ( this._license == null )
        {
            this._logger.Error?.Log( "No license provided." );

            return false;
        }

        if ( !string.IsNullOrEmpty( consumerNamespace )
             && this._licensedNamespace != null
             && !this._licensedNamespace.AllowsNamespace( consumerNamespace ) )
        {
            this.ReportMessage( new LicensingMessage(
                $"Namespace '{consumerNamespace}' is not licensed. Your license is limited to '{this._licensedNamespace.AllowedNamespace}' namespace.",
                true ) );

            return false;
        }

        if ( !requirement.IsFullfilledBy( this._license ) )
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
    public string? RedistributionLicenseKey { get; }

    /// <inheritdoc />
    public IReadOnlyList<LicensingMessage> Messages => this._messages;
}