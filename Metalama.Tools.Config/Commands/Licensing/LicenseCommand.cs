// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

namespace Metalama.DotNetTools.Commands.Licensing;

internal class LicenseCommand : CommandBase
{
    public LicenseCommand( ICommandServiceProviderProvider commandServiceProvider )
        : base( commandServiceProvider, "license", "Manages licenses" )
    {
        this.Add( new ListCommand( commandServiceProvider ) );
        this.Add( new RegisterCommand( commandServiceProvider ) );
        this.Add( new UnregisterCommand( commandServiceProvider ) );
    }
}