// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Cli.Session;
using System;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class LicenseCommandSessionState : CliSessionState
    {
        public LicenseCommandSessionState( IServiceProvider services )
            : base( "license", services ) { }
    }
}