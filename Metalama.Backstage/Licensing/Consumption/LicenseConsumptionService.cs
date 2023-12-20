// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption.Sources;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption;

/// <inheritdoc />
internal partial class LicenseConsumptionService : ILicenseConsumptionService
{
    private readonly IServiceProvider _services;
    private readonly IReadOnlyList<ILicenseSource> _sources;
    private ILicenseConsumptionService? _impl;

    public LicenseConsumptionService( IServiceProvider services, IReadOnlyList<ILicenseSource> licenseSources )
    {
        this._services = services;
        this._sources = licenseSources;

        foreach ( var source in this._sources )
        {
            source.Changed += this.OnSourceChanged;
        }
    }

    private ILicenseConsumptionService GetInitializedImpl()
    {
        if ( this._impl == null )
        {
            this.OnSourceChanged();
        }

        return this._impl!;
    }

    public ILicenseConsumptionService WithAdditionalLicense( string? licenseKey )
    {
        if ( string.IsNullOrWhiteSpace( licenseKey ) )
        {
            return this;
        }
        
        var sources = new List<ILicenseSource>( this._sources.Count + 1 );
        sources.AddRange( this._sources );
        sources.Add( new ExplicitLicenseSource( licenseKey!, this._services ) );
        var newService = new LicenseConsumptionService( this._services, sources );

        return newService;
    }

    private void OnSourceChanged()
    {
        this._impl = new ImmutableImpl( this._services, this._sources );

        this.Changed?.Invoke();
    }

    public IReadOnlyList<LicensingMessage> Messages => this.GetInitializedImpl().Messages;

    public bool CanConsume( LicenseRequirement requirement, string? consumerNamespace = null ) => this.GetInitializedImpl().CanConsume( requirement, consumerNamespace );

    public bool ValidateRedistributionLicenseKey( string redistributionLicenseKey, string aspectClassNamespace )
        => this.GetInitializedImpl().ValidateRedistributionLicenseKey( redistributionLicenseKey, aspectClassNamespace );

    public bool IsTrialLicense => this.GetInitializedImpl().IsTrialLicense;

    public bool IsRedistributionLicense => this.GetInitializedImpl().IsRedistributionLicense;

    public string? LicenseString => this.GetInitializedImpl().LicenseString;

    public event Action? Changed;
}