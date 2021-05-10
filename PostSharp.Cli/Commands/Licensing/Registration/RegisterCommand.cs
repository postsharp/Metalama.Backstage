// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Cli.Console;

namespace PostSharp.Cli.Commands.Licensing.Registration
{
    internal class RegisterCommand : CommandBase
    {
        // TODO Description?

        public RegisterCommand()
            : base( "register" )
        {
            this.AddArgument( new Argument<string>( "license-string", "license key or license server URL" ) );

            this.Handler = CommandHandler.Create<string, bool, IConsole> ( this.Execute );
        }

        private int Execute( string licenseString, bool verbose, IConsole console )
        {
            (var services, var trace) = this.CreateServices( console, verbose );

            var factory = new LicenseFactory( services, trace );

            if ( !factory.TryCreate( licenseString, out var license )
                || !license.TryGetLicenseRegistrationData( out var data ) )
            {
                return -1;
            }

            var storage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, services, trace );

            storage.AddLicense( licenseString, data );
            storage.Save();

            return 0;
        }
    }
}
