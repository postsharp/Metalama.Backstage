// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Audit;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Backstage.Licensing.Consumption;

internal partial class LicenseConsumptionService
{
    private class ImmutableImpl : ILicenseConsumptionService
    {
        private readonly ILogger _logger;
        private readonly List<LicensingMessage> _messages = new();
        private readonly LicenseFactory _licenseFactory;
        private readonly Dictionary<string, NamespaceLicenseInfo> _embeddedRedistributionLicensesCache = new();
        private readonly LicenseConsumptionData? _license;
        private readonly NamespaceLicenseInfo? _licensedNamespace;

        public ImmutableImpl( IServiceProvider services, IReadOnlyList<ILicenseSource> licenseSources )
        {
            this._logger = services.GetLoggerFactory().Licensing();

            this._licenseFactory = new LicenseFactory( services );

            foreach ( var source in licenseSources.OrderBy( s => s.Priority ) )
            {
                var license = source.GetLicense( this.ReportMessage );

                if ( license == null )
                {
                    this._logger.Trace?.Log( $"'{source.GetType().Name}' license source provided no license." );

                    continue;
                }

                if ( !license.TryGetLicenseConsumptionData( out var data, out var errorMessage ) )
                {
                    _ = license.TryGetProperties( out var registrationData, out _ );
                    var message = registrationData == null ? "A license" : $"The {registrationData.Description}";
                    message += $" {errorMessage}.";

                    if ( registrationData is { IsSelfCreated: false } )
                    {
                        message += $" License key ID: '{registrationData.LicenseId}'.";
                    }

                    if ( source.GetType() != typeof(UserProfileLicenseSource) )
                    {
                        message += $" The license key originates from {source.Description}.";
                    }

                    this.ReportMessage( new LicensingMessage( message ) );

                    continue;
                }

                this._license = data;
                this._licensedNamespace = string.IsNullOrEmpty( data.LicensedNamespace ) ? null : new NamespaceLicenseInfo( data.LicensedNamespace! );

                var licenseAuditManager = services.GetBackstageService<ILicenseAuditManager>();
                licenseAuditManager?.ReportLicense( data );

                return;
            }
        }

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

        /// <inheritdoc />
        public bool CanConsume( LicenseRequirement requirement, string? consumerProjectName = null )
        {
            if ( this._license == null )
            {
                this._logger.Error?.Log( "No license provided." );

                return false;
            }

            // Redistributable licenses allow building any namespace. Their namespace field limits the redistribution only.
            // Ie., the namespace of redistributed aspects built with the redistribution license key.
            if ( !this.IsRedistributionLicense
                 && !string.IsNullOrEmpty( consumerProjectName )
                 && this._licensedNamespace != null
                 && !this._licensedNamespace.AllowsNamespace( consumerProjectName ) )
            {
                this.ReportMessage(
                    new LicensingMessage(
                        $"Project '{consumerProjectName}' is not licensed. Your license is limited to project names beginning with '{this._licensedNamespace.AllowedNamespace}'." )
                    {
                        IsError = true
                    } );

                return false;
            }

            if ( !requirement.IsFulfilledBy( this._license ) )
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
                if ( !this._licenseFactory.TryCreate( redistributionLicenseKey, out var license, out var errorMessage ) )
                {
                    this.ReportMessage( new LicensingMessage( errorMessage ) { IsError = true } );

                    return false;
                }

                if ( !license.TryGetLicenseConsumptionData( out var licenseConsumptionData, out errorMessage ) )
                {
                    this.ReportMessage( new LicensingMessage( errorMessage ) { IsError = true } );

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

                licensedNamespace = new NamespaceLicenseInfo( licenseConsumptionData.LicensedNamespace! );
                this._embeddedRedistributionLicensesCache.Add( redistributionLicenseKey, licensedNamespace );
            }

            return licensedNamespace.AllowsNamespace( aspectClassNamespace );
        }

        public bool IsTrialLicense => this._license?.LicenseType == LicenseType.Evaluation;

        public bool IsRedistributionLicense => this._license?.IsRedistributable == true;

        public string? LicenseString => this._license?.LicenseString;

        event Action? ILicenseConsumptionService.Changed { add { } remove { } }

        public ILicenseConsumptionService WithAdditionalLicense( string licenseKey ) => throw new NotSupportedException();

        /// <inheritdoc />
        public IReadOnlyList<LicensingMessage> Messages => this._messages;
    }
}