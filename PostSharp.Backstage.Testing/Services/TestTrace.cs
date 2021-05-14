// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using PostSharp.Backstage.Extensibility;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Testing.Services
{
    public class TestTrace : ITrace
    {
        private readonly List<string> _messages = new();

        public IReadOnlyList<string> Messages => this._messages;

        private readonly ITestOutputHelper _logger;

        public TestTrace( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        public void WriteLine( string message )
        {
            this._messages.Add( message );
            this._logger.WriteLine( message );
        }

        public void Clear() => this._messages.Clear();
    }
}
