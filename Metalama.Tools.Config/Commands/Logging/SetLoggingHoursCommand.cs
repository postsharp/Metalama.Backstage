// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Metalama.DotNetTools.Commands.Logging;

internal class SetLoggingHoursCommand : CommandBase
{
    public SetLoggingHoursCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "log",
        "Sets the time before logging is automatically disabled" )
    {
        this.AddArgument( new Argument<double>( "hours", "Hours after last modification of the configuration file before logging is automatically disabled" ) );
        this.Handler = CommandHandler.Create<double, IConsole>( this.Execute );
    }

    private void Execute( double hours, IConsole console )
    {
        this.CommandServices.Initialize( console, false );
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();
       
        configurationManager.Update<DiagnosticsConfiguration>(
            c =>
            {
                c.Logging.StopLoggingAfterHours = hours;

                return c;
            } );
    }
}