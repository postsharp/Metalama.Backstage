// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class ShowCommand : CommandBase
    {
        public ShowCommand( IServicesFactory servicesFactory )
            : base( servicesFactory, "show", "Shows a license key" )
        {
            this.AddArgument( new Argument<int>( "ordinal", "The ordinal obtained by the 'postsharp license list' command" ) );

            this.Handler = CommandHandler.Create<int, bool, IConsole>( this.Execute );
        }

        private int Execute( int ordinal, bool verbose, IConsole console )
        {
            (var services, var trace) = this.ServicesFactory.Create( console, verbose );

            var ordinals = LicenseStringsOrdinalDictionary.Load( services );

            if ( !ordinals.TryGetValue( ordinal, out var licenseString ) )
            {
                console.Error.WriteLine( "Unknown ordinal." );
                return 1;
            }

            var factory = new LicenseFactory( services, trace );

            if ( !factory.TryCreate( licenseString, out var license )
                || !license.TryGetLicenseRegistrationData( out var data ) )
            {
                console.Error.WriteLine( "Invalid license string." );
                return 2;
            }

            if (data.LicenseId == null)
            {
                console.Error.WriteLine( "This license doesn't come with a license key." );
                return 3;
            }

            console.Out.WriteLine( licenseString );
            return 0;
        }
    }
}
