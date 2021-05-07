﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class LicenseCommand : CommandBase
    {
        // TODO: Description?
        public LicenseCommand( IServiceProvider services )
            : base( services, "license" )
        {
            this.Add( new ListCommand( services ) );
        }
    }
}
