// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Cli.Commands.Licensing.Registration;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class LicenseCommand : CommandBase
    {
        public LicenseCommand( ICommandServiceProvider commandServiceProvider )
            : base( commandServiceProvider, "license", "Manages licenses" )
        {
            Add( new ListCommand( commandServiceProvider ) );
            Add( new RegisterCommand( commandServiceProvider ) );
            Add( new UnregisterCommand( commandServiceProvider ) );
        }
    }
}