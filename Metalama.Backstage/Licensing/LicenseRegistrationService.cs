// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Licensing.Registration.Evaluation;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Backstage.Licensing;

internal class LicenseRegistrationService : ILicenseRegistrationService
{
    private readonly IServiceProvider _serviceProvider;

    public LicenseRegistrationService( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }

    public bool TryRegisterFreeEdition( [NotNullWhen( false )] out string? errorMessage )
    {
        var registrar = new EvaluationLicenseRegistrar( this._serviceProvider );

        if ( !registrar.TryActivateLicense() )
        {
            errorMessage = "Cannot activate Metalama Free.";

            return false;
        }

        errorMessage = null;

        return true;
    }

    public bool TryRegisterTrialEdition( [NotNullWhen( false )] out string? errorMessage )
    {
        var registrar = new EvaluationLicenseRegistrar( this._serviceProvider );

        if ( !registrar.TryActivateLicense() )
        {
            errorMessage = "Cannot start the trial period.";

            return false;
        }

        errorMessage = null;

        return true;
    }

    public bool TryRegisterLicense( string licenseString, [NotNullWhen( false )] out string? errorMessage )
    {
        var factory = new LicenseFactory( this._serviceProvider );

        if ( !factory.TryCreate( licenseString, out var license, out errorMessage )
             || !license.TryGetLicenseRegistrationData( out var data, out errorMessage ) )
        {
            return false;
        }

        var storage = ParsedLicensingConfiguration.OpenOrCreate( this._serviceProvider );
        storage.SetLicense( licenseString, data );

        return true;
    }
}