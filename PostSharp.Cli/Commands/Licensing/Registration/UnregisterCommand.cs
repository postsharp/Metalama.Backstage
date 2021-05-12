// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Cli.Commands.Licensing.Registration
{
    internal class UnregisterCommand : CommandBase
    {
        private class Unregisterer
        {
            private readonly IServiceProvider _services;
            private readonly ITrace _trace;
            private readonly IConsole _console;

            public Unregisterer( IServiceProvider services, ITrace trace, IConsole console )
            {
                this._services = services;
                this._trace = trace;
                this._console = console;
            }

            public int UnregisterOrdinal( int ordinal )
            {
                var ordinals = LicenseStringsOrdinalDictionary.Load( this._services );

                if ( !ordinals.TryGetValue( ordinal, out var license ) )
                {
                    this._console.Error.WriteLine( "Unknown ordinal." );
                    return 1;
                }

                return this.UnregisterLicense( license );
            }

            public int UnregisterLicense( string license )
            {
                var storage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, this._services, this._trace );

                if ( !storage.Licenses.ContainsKey( license ) )
                {
                    this._console.Error.WriteLine( $"'{license}' is not registered." );
                    return 2;
                }

                storage.RemoveLicense( license );
                storage.Save();

                this._console.Out.WriteLine( $"'{license}' unregistered." );

                return 0;
            }
        }

        public UnregisterCommand( IServicesFactory servicesFactory )
            : base( servicesFactory, "unregister", "Unregisters a license" )
        {
            this.AddArgument( new Argument<string>( "license", "The ordinal obtained by the 'postsharp license list' command or the license key to be unregistered" ) );

            this.Handler = CommandHandler.Create<string, bool, IConsole>( this.Execute );
        }

        private int Execute( string license, bool verbose, IConsole console )
        {
            (var services, var trace) = this.ServicesFactory.Create( console, verbose );

            var unregisterer = new Unregisterer( services, trace, console );

            if (int.TryParse(license, out var ordinal))
            {
                return unregisterer.UnregisterOrdinal( ordinal );
            }
            else
            {
                return unregisterer.UnregisterLicense( license );
            }
        }
    }
}
