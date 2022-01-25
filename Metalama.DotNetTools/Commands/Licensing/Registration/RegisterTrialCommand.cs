// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Registration.Evaluation;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Licensing.Registration
{
    internal class RegisterTrialCommand : CommandBase
    {
        public RegisterTrialCommand( ICommandServiceProvider commandServiceProvider )
            : base( commandServiceProvider, "trial", "Starts the trial period" )
        {
            this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        }

        private int Execute( bool verbose, IConsole console )
        {
            var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );

            var registrar = new EvaluationLicenseRegistrar( services );

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
}