// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Metalama.Backstage.Commands.Commands.Configuration;

internal class ResetCommand : CommandBase
{
    public ResetCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "reset",
        "Resets configuration for all applications and all categories" )
    {
        this.AddArgument(
            new Argument<string>(
                "name",
                "Name of the configuration category to be reset" ) );
        
        this.Handler = CommandHandler.Create<string, bool, IConsole>( this.Execute );
    }

    private void Execute( string name, bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();
        var configurationType = BackstageCommandFactory.ConfigurationCategories[name].GetType();

        configurationManager.Update<DiagnosticsConfiguration>( _ => new DiagnosticsConfiguration() );
    }
}