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
            private readonly ITrace? _trace;
            private readonly IConsole _console;

            public Unregisterer( IServiceProvider services, IConsole console )
            {
                this._services = services;
                this._trace = services.GetOptionalService<ITrace>();
                this._console = console;
            }

            public int UnregisterOrdinal( int ordinal )
            {
                var ordinals = new LicenseCommandSessionState( this._services ).Load();

                if ( !ordinals.TryGetValue( ordinal, out var license ) )
                {
                    this._console.Error.WriteLine( "Invalid ordinal." );
                    return 1;
                }

                return this.UnregisterLicense( license! );
            }

            public int UnregisterLicense( string licenseString )
            {
                var storage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, this._services );

                if ( !storage.Licenses.TryGetValue( licenseString, out var data ) )
                {
                    this._console.Error.WriteLine( "This license is not registered." );
                    return 2;
                }

                storage.RemoveLicense( licenseString );
                storage.Save();

                string description;

                if ( data == null )
                {
                    description = licenseString;
                }
                else if ( data.IsSelfCreated )
                {
                    description = data.Description;
                }
                else
                {
                    description = licenseString;
                }

                this._console.Out.WriteLine( $"{description} unregistered." );

                return 0;
            }
        }

        public UnregisterCommand( ICommandServiceProvider commandServiceProvider )
            : base( commandServiceProvider, "unregister", "Unregisters a license" )
        {
            this.AddArgument( new Argument<string>( "license-key-or-ordinal", "The ordinal obtained by the 'postsharp license list' command or the license key to be unregistered" ) );

            this.Handler = CommandHandler.Create<string, bool, IConsole>( this.Execute );
        }

        private int Execute( string licenseKeyOrOrdinal, bool verbose, IConsole console )
        {
            var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );

            var unregisterer = new Unregisterer( services, console );

            if (int.TryParse(licenseKeyOrOrdinal, out var ordinal))
            {
                return unregisterer.UnregisterOrdinal( ordinal );
            }
            else
            {
                return unregisterer.UnregisterLicense( licenseKeyOrOrdinal );
            }
        }
    }
}
