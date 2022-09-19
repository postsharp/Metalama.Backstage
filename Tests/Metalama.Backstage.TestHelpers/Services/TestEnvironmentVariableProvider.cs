// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Testing.Services;

public class TestEnvironmentVariableProvider : IEnvironmentVariableProvider
{
    private class MockEnvironment
    {
        public Dictionary<string, string?> Environment { get; }

        public MockEnvironment()
        {
            this.Environment = new Dictionary<string, string?>();
        }
    }

    private MockEnvironment Mock { get; } = new();

    public string DefaultDiagnosticsEnvironmentVariableName { get; } = "METALAMA_DIAGNOSTICS";

    public string? GetEnvironmentVariable( string variable ) => this.Mock.Environment.ContainsKey( variable ) ? this.Mock.Environment[variable] : null;

    public void SetEnvironmentVariable( string variable, string? value )
    {
        if ( this.Mock.Environment.ContainsKey( variable ) )
        {
            this.Mock.Environment[variable] = value;
        }
        else
        {
            this.Mock.Environment.Add( variable, value );
        }
    }

    public DiagnosticsConfiguration? GetDiagnosticsConfigurationFromEnvironmentVariable( string variable )
    {
        var environmentVariableValue = this.GetEnvironmentVariable( variable );

        if ( string.IsNullOrEmpty( environmentVariableValue ) )
        {
            throw new InvalidOperationException( $"Environment variable '{variable}' is not set." );
        }

        return JsonConvert.DeserializeObject<DiagnosticsConfiguration>( environmentVariableValue! );
    }
}