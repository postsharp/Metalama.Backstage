// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Spectre.Console.Cli;

internal static class Program
{
    public static int Main( string[] args )
    {
        var app = new CommandApp();
        BackstageCommandFactory.ConfigureCommandApp( app, new BackstageCommandOptions(new ApplicationInfo()) );

        return app.Run( args );        
    }
}