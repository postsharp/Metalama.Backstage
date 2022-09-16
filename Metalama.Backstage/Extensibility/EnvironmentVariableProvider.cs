// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Newtonsoft.Json;
using System;

namespace Metalama.Backstage.Extensibility;

public class EnvironmentVariableProvider : IEnvironmentVariableProvider
{
    public bool IsEnvironmentVariableSet( string variable ) => !string.IsNullOrEmpty( this.GetEnvironmentVariable( variable ) );

    public string? GetEnvironmentVariable( string variable ) => Environment.GetEnvironmentVariable( variable, EnvironmentVariableTarget.Process );

    public void SetEnvironmentVariable( string variable, string? value ) => Environment.SetEnvironmentVariable( variable, value, EnvironmentVariableTarget.Process );

    public DiagnosticsConfiguration? GetDiagnosticsConfigurationFromEnvironmentVariable( string variable )
    {
        var environmentVariableValue = this.GetEnvironmentVariable( variable );
        
        if ( string.IsNullOrEmpty( environmentVariableValue ) )
        {
            throw new InvalidOperationException( $"Environment variable '{variable}' is not set or it's value is empty." );
        }
        
        return JsonConvert.DeserializeObject<DiagnosticsConfiguration>( environmentVariableValue! );
    }
}