// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Cli.Commands.Licensing.Registration;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class LicenseCommand : CommandBase
    {
        // TODO: Description?
        public LicenseCommand( IServicesFactory servicesFactory )
            : base( servicesFactory, "license" )
        {
            this.Add( new ListCommand( servicesFactory ) );
            this.Add( new ShowCommand( servicesFactory ) );
            this.Add( new RegisterCommand( servicesFactory ) );
            this.Add( new UnregisterCommand( servicesFactory ) );
        }
    }
}
