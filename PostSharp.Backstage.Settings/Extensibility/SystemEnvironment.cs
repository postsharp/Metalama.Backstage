// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Backstage.Extensibility
{
    public class SystemEnvironment : IEnvironment
    {
        public bool TryGetEnvironmentVariable( string name, [MaybeNullWhen( returnValue: false )] out string? value )
        {
            value = Environment.GetEnvironmentVariable( name );
            return value != null;
        }

        public void SetEnvironmentVariable( string name, string? value )
        {
            Environment.SetEnvironmentVariable( name, value );
        }
    }
}
