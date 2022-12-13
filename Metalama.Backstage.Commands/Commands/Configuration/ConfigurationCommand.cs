// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.CommandLine;
using System.CommandLine.IO;

namespace Metalama.Backstage.Commands.Commands.Configuration;

internal class ConfigurationCommand : CommandBase
{
    public ConfigurationCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "config",
        "Allows managing Metalama configuration settings" )
    {
        this.Add( new PrintCommand( commandServiceProvider ) );
        this.Add( new EditCommand( commandServiceProvider ) );
        this.Add( new ResetCommand( commandServiceProvider ) );
        this.Add( new ListCommand( commandServiceProvider ) );
    }

    internal static bool VerifyArgumentExistsInDictionary( string argument, IConsole console )
    {
        var containsKey = BackstageCommandFactory.ConfigurationFilesByCategory.ContainsKey( argument );
        
        if ( !containsKey )
        {
            console.Out.WriteLine( $"Argument '{argument}' is not a recognized configuration name. Use 'metalama config list' command to see list of all accepted names." );
        }
    
        return containsKey;
    }
}