// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Testing.Services
{
    public class TestEnvironment : IEnvironment
    {
        private readonly Dictionary<string, string> _variables = new();

        public bool TryGetEnvironmentVariable( string name, out string? value )
        {
            return this._variables.TryGetValue( name, out value );
        }

        public void SetEnvironmentVariable( string name, string? value )
        {
            if (value == null)
            {
                this._variables.Remove( name );
            }
            else
            {
                this._variables[name] = value;
            }
        }
    }
}
