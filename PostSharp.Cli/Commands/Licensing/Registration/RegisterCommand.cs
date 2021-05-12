// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Cli.Commands.Licensing.Registration
{
    internal class RegisterCommand : CommandBase
    {
        // TODO Reporting of license registration.

        public RegisterCommand( IServicesFactory servicesFactory )
            : base( servicesFactory, "register", "Registers a license key, a trial license or a community license" )
        {
            this.AddArgument( new Argument<string>( "license-key", "The license key to be registered" ) );

            this.AddCommand( new RegisterTrialCommand( servicesFactory ) );
            this.AddCommand( new RegisterCommunityCommand( servicesFactory ) );

            this.Handler = CommandHandler.Create<string, bool, IConsole>( this.Execute );
        }

        private int Execute( string licenseKey, bool verbose, IConsole console )
        {
            (var services, var trace) = this.ServicesFactory.Create( console, verbose );

            var factory = new LicenseFactory( services, trace );

            if ( !factory.TryCreate( licenseKey, out var license )
                || !license.TryGetLicenseRegistrationData( out var data ) )
            {
                console.Error.WriteLine( "Invalid license string." );
                return 1;
            }

            var storage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, services, trace );

            storage.AddLicense( licenseKey, data );
            storage.Save();

            return 0;
        }
    }
}
