// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Registration.Evaluation;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Licensing;

internal class RegisterTrialCommand : CommandBase
{
    public RegisterTrialCommand( ICommandServiceProviderProvider commandServiceProvider )
        : base( commandServiceProvider, "trial", "Starts the trial period" )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
    }

    private int Execute( bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        var registrar = new EvaluationLicenseRegistrar( this.CommandServices.ServiceProvider );

        if ( registrar.TryActivateLicense() )
        {
            return 0;
        }
        else
        {
            console.Error.WriteLine( "Cannot start the trial period. Use --verbose (-v) flag for details." );

            return 1;
        }
    }
}