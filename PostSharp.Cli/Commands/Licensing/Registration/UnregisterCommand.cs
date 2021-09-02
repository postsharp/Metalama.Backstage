// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace PostSharp.Cli.Commands.Licensing.Registration
{
    internal class UnregisterCommand : CommandBase
    {
        private class UnregisterActions
        {
            private readonly IServiceProvider _services;
            private readonly ILogger? _logger;
            private readonly IConsole _console;

            public UnregisterActions( IServiceProvider services, IConsole console )
            {
                this._services = services;
                this._logger = services.GetOptionalTraceLogger<UnregisterCommand>();
                this._console = console;
            }

            public int UnregisterOrdinal( int ordinal )
            {
                // TODO: tracing
                this._logger.LogInformation( "TODO: tracing" );
                
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
                var licenseFiles = this._services.GetRequiredService<IStandardLicenseFileLocations>();
                var storage = LicenseFileStorage.OpenOrCreate( licenseFiles.UserLicenseFile, this._services );

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
            this.AddArgument(
                new Argument<string>(
                    "license-key-or-ordinal",
                    "The ordinal obtained by the 'postsharp license list' command or the license key to be unregistered" ) );

            this.Handler = CommandHandler.Create<string, bool, IConsole>( this.Execute );
        }

        private int Execute( string licenseKeyOrOrdinal, bool verbose, IConsole console )
        {
            var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );

            var actions = new UnregisterActions( services, console );

            if ( int.TryParse( licenseKeyOrOrdinal, out var ordinal ) )
            {
                return actions.UnregisterOrdinal( ordinal );
            }
            else
            {
                return actions.UnregisterLicense( licenseKeyOrOrdinal );
            }
        }
    }
}