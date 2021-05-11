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
        // TODO: Description?
        public RegisterTrialCommand( IServicesFactory servicesFactory )
            : base( servicesFactory, "trial" )
        {
            this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        }

        private int Execute( bool verbose, IConsole console )
        {
            (var services, var trace) = this.ServicesFactory.Create( console, verbose );

            var registrar = new EvaluationLicenseRegistrar( services, trace );

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
