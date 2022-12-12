// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Commands.Commands.Configuration;

internal class ConfigurationCommand : CommandBase
{
    public ConfigurationCommand( ICommandServiceProviderProvider commandServiceProvider ) : base( commandServiceProvider, "config", "Allows managing Metalama configuration settings. " )
    {
        this.Add( new PrintCommand( commandServiceProvider ) );
        this.Add( new EditCommand( commandServiceProvider ) );
        this.Add( new ResetCommand( commandServiceProvider ) );
        this.Add( new ListCommand( commandServiceProvider ) );
    }
}