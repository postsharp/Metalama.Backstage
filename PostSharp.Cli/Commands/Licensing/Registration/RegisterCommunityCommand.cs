﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Registration.Community;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace PostSharp.Cli.Commands.Licensing.Registration
{
    internal class RegisterCommunityCommand : CommandBase
    {
        public RegisterCommunityCommand( ICommandServiceProvider commandServiceProvider )
            : base( commandServiceProvider, "community", "Switches to the community edition" )
        {
            Handler = CommandHandler.Create<bool, IConsole>( Execute );
        }

        private int Execute( bool verbose, IConsole console )
        {
            var services = CommandServiceProvider.CreateServiceProvider( console, verbose );

            var registrar = new CommunityLicenseRegistrar( services );

            if (registrar.TryRegisterLicense())
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