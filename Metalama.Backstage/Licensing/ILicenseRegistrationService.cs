// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Backstage.Licensing;

public interface ILicenseRegistrationService : IBackstageService
{
    bool TryRegisterFreeEdition( [NotNullWhen( false )] out string? errorMessage );

    bool TryRegisterTrialEdition( [NotNullWhen( false )] out string? errorMessage );

    bool TryRegisterLicense( string licenseString, [NotNullWhen( false )] out string? errorMessage );
}