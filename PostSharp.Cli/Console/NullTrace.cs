// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;

namespace PostSharp.Cli.Console
{
    internal class NullTrace : ITrace
    {
        public void WriteLine( string message )
        {
        }

        public void WriteLine( string format, params object[] args )
        {
        }
    }
}
