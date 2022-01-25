// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace PostSharp.Cli.Commands.Licensing.Registration
{
    internal class RegisterCommand : CommandBase
    {
        // TODO Reporting of license registration.

        public RegisterCommand( ICommandServiceProvider commandServiceProvider )
            : base(
                commandServiceProvider,
                "register",
                "Registers a license key, starts the trial period, switch to the community edition" )
        {
            this.AddArgument(
                new Argument<string>(
                    "license-key-or-type",
                    "The license key to be registered, or 'trial' or 'community'" ) );

            this.AddCommand( new RegisterTrialCommand( commandServiceProvider ) );
            this.AddCommand( new RegisterCommunityCommand( commandServiceProvider ) );

            this.Handler = CommandHandler.Create<string, bool, IConsole>( this.Execute );
        }

        private int Execute( string licenseKey, bool verbose, IConsole console )
        {
            var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );

            var factory = new LicenseFactory( services );

            if ( !factory.TryCreate( licenseKey, out var license )
                 || !license.TryGetLicenseRegistrationData( out var data ) )
            {
                console.Error.WriteLine( "Invalid license string." );

                return 1;
            }

            var storage = ParsedLicensingConfiguration.OpenOrCreate( services );

            storage.AddLicense( licenseKey, data );

            return 0;
        }
    }
}