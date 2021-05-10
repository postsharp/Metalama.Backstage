// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Cli.Commands.Licensing.Registration;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class LicenseCommand : CommandBase
    {
        // TODO: Description?
        public LicenseCommand()
            : base( "license" )
        {
            this.Add( new ListCommand() );
            this.Add( new RegisterCommand() );
        }
    }
}
