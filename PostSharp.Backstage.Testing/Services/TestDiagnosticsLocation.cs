// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;

// ReSharper disable RedundantToStringCall

namespace PostSharp.Backstage.Testing.Services
{
    public class TestDiagnosticsLocation : IDiagnosticsLocation
    {
        private readonly string _description;

        public TestDiagnosticsLocation( string description )
        {
            this._description = description;
        }

        public override string ToString()
        {
            return this._description.ToString();
        }
    }
}