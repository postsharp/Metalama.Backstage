// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using PostSharp.Backstage.Licensing.Evaluation;

namespace PostSharp.Cli.Commands.Licensing.Registration
{
    internal class RegisterTrialCommand : CommandBase
    {
        public RegisterTrialCommand( ICommandServiceProvider commandServiceProvider )
            : base( commandServiceProvider, "trial", "Registers a trial license" )
        {
            this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        }

        private int Execute( bool verbose, IConsole console )
        {
            var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );

            var registrar = new EvaluationLicenseRegistrar( services );

            if ( registrar.TryRegisterLicense() )
            {
                return 0;
            }
            else
            {
                console.Error.WriteLine( "Cannot register evaluation license. Use --verbose (-v) flag for details." );
                return 1;
            }
        }
    }
}
