// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Backstage.Extensibility
{
    public interface IEnvironment
    {
        bool TryGetEnvironmentVariable( string name, [MaybeNullWhen( returnValue: false )] out string? value );

        void SetEnvironmentVariable( string name, string? value );
    }
}
