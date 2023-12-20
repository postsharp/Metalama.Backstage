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
    private ILicenseConsumptionService _impl;

    public LicenseConsumptionService( IServiceProvider services, IReadOnlyList<ILicenseSource> licenseSources )
    {
        this._services = services;
        this._sources = licenseSources;

        foreach ( var source in this._sources )
        {
            source.Changed += this.OnSourceChanged;
        }

        // The component should be manually initialized, as
        this._impl = new UninitializedImpl();
    }

    public void Initialize()
    {
        if ( this._impl is not UninitializedImpl )
        {
            throw new InvalidOperationException( "The service cannot be initialized twice." );
        }

        this.OnSourceChanged();
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
        newService.Initialize();

        return newService;
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

    private class UninitializedImpl : ILicenseConsumptionService
    {
        private const string _message = "The LicenseConsumptionService has not been initialized.";

        public IReadOnlyList<LicensingMessage> Messages => throw new InvalidOperationException( _message );

        public bool CanConsume( LicenseRequirement requirement, string? consumerNamespace = null ) => throw new InvalidOperationException( _message );

        public bool ValidateRedistributionLicenseKey( string redistributionLicenseKey, string aspectClassNamespace )
            => throw new InvalidOperationException( _message );

        public bool IsTrialLicense => throw new InvalidOperationException( _message );

        public bool IsRedistributionLicense => throw new InvalidOperationException( _message );

        public string? LicenseString => throw new InvalidOperationException( _message );

        public event Action? Changed
        {
            add => throw new InvalidOperationException( _message );
            remove => throw new InvalidOperationException( _message );
        }

        public void Initialize() => throw new InvalidOperationException( _message );

        public ILicenseConsumptionService WithAdditionalLicense( string? licenseKey ) => throw new NotSupportedException();
    }
}