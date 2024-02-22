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
    private ImmutableImpl _impl;

    public LicenseConsumptionService( IServiceProvider services, IReadOnlyList<ILicenseSource> licenseSources )
    {
        this._services = services;
        this._sources = licenseSources;
        this._impl = new ImmutableImpl( services, this._sources );

        foreach ( var source in this._sources )
        {
            source.Changed += this.OnSourceChanged;
        }
    }

    private void OnSourceChanged()
    {
        this._impl = new ImmutableImpl( this._services, this._sources );

        this.Changed?.Invoke();
    }

    public IReadOnlyList<LicensingMessage> Messages => this._impl.Messages;

    public bool CanConsume( LicenseRequirement requirement, string? consumerNamespace = null ) => this._impl.CanConsume( requirement, consumerNamespace );

    public bool ValidateRedistributionLicenseKey( string redistributionLicenseKey, string aspectClassNamespace )
        => this._impl.ValidateRedistributionLicenseKey( redistributionLicenseKey, aspectClassNamespace );

    public bool IsTrialLicense => this._impl.IsTrialLicense;

    public bool IsRedistributionLicense => this._impl.IsRedistributionLicense;

    public string? LicenseString => this._impl.LicenseString;

    public event Action? Changed;
}