// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Testing.Services;

public class TestEnvironmentVariableProvider : IEnvironmentVariableProvider
{
    public class MockEnvironment
    {
        public Dictionary<string, string?> Environment { get; }

        public MockEnvironment()
        {
            this.Environment = new Dictionary<string, string?>();
        }
    }

    public MockEnvironment Mock { get; } = new();

    public string EnvironmentVariableName => "METALAMA_DIAGNOSTICS";

    public bool IsEnvironmentVariableSet( string variable )
    {
        return this.Mock.Environment.ContainsKey( variable );
    }

    public string? GetEnvironmentVariable( string variable ) => this.Mock.Environment[variable];

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

    public DiagnosticsConfiguration GetDiagnosticsConfigurationFromEnvironmentVariable( string variable )
    {
        return JsonConvert.DeserializeObject<DiagnosticsConfiguration>( this.GetEnvironmentVariable( variable ) );
    }
}