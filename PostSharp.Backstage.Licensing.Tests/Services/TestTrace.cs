// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal class TestTrace : ITrace
    {
        private readonly ITestOutputHelper _logger;

        public TestTrace( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        public void WriteLine( string message )
        {
            this._logger.WriteLine( message );
        }

        public void WriteLine( string message, params object[] args )
        {
            this._logger.WriteLine( message, args );
        }
    }
}
