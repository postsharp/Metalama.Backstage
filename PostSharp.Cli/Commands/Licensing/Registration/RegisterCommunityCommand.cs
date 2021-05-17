// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using PostSharp.Backstage.Licensing.Registration.Community;

namespace PostSharp.Cli.Commands.Licensing.Registration
{
    internal class RegisterCommunityCommand : CommandBase
    {
        public RegisterCommunityCommand( ICommandServiceProvider commandServiceProvider )
            : base( commandServiceProvider, "community", "Switchs to the community edition" )
        {
            this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        }

        private int Execute( bool verbose, IConsole console )
        {
            var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );

            var registrar = new CommunityLicenseRegistrar( services );

            if ( registrar.TryRegisterLicense() )
            {
                return 0;
            }
            else
            {
                console.Error.WriteLine( "Cannot switch to the community edition. Use --verbose (-v) flag for details." );
                return 1;
            }
        }
    }
}
