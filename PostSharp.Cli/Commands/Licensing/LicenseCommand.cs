// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class LicenseCommand : CommandBase
    {
        // TODO: Description?
        public LicenseCommand( IServiceProvider services, ITrace trace )
            : base( services, trace, "license" )
        {
        }
    }
}
