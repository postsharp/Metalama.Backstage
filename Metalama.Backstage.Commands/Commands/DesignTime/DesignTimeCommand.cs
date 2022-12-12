// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Commands.Commands.DesignTime;

internal class DesignTimeCommand : CommandBase
{
    public DesignTimeCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "design-time",
        "Manages design-time options" )
    {
        this.AddCommand( new ResetDesignTimeConfigurationCommand( commandServiceProvider ) );
        this.AddCommand( new PrintDesignTimeConfigurationCommand( commandServiceProvider ) );
        this.AddCommand( new EditDesignTimeConfigurationCommand( commandServiceProvider ) );
    }
}