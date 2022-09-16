// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;

namespace Metalama.Backstage.Extensibility;

public interface IEnvironmentVariableProvider : IBackstageService
{
    bool IsEnvironmentVariableSet( string variable );
    
    string? GetEnvironmentVariable( string variable );

    void SetEnvironmentVariable( string variable, string? value );

    DiagnosticsConfiguration? GetDiagnosticsConfigurationFromEnvironmentVariable( string variable );
}