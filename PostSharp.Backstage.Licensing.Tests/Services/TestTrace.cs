// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using PostSharp.Backstage.Extensibility;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal class TestTrace : ITrace
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

        public void WriteLine( string format, params object[] args )
        {
            this._messages.Add( string.Format( format, args ) );
            this._logger.WriteLine( format, args );
        }

        public void Clear() => this._messages.Clear();
    }
}
