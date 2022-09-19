// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;

namespace Metalama.Backstage.Extensibility;

public interface IEnvironmentVariableProvider : IBackstageService
{
    string DefaultDiagnosticsEnvironmentVariableName { get; }
    
    string? GetEnvironmentVariable( string variable );

    void SetEnvironmentVariable( string variable, string? value );

    DiagnosticsConfiguration? GetDiagnosticsConfigurationFromEnvironmentVariable( string variable );
}