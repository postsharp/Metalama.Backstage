// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Globalization;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class ListCommand : CommandBase
    {
        public ListCommand( ICommandServiceProvider commandServiceProvider )
            : base( commandServiceProvider, "list", "Lists registered licenses" )
        {
            this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        }

        private int Execute( bool verbose, IConsole console )
        {
            var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );

            var storage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, services );

            var ordinal = 1;
            LicenseCommandSessionState ordinals = new( services );

            foreach ( var license in storage.Licenses )
            {
                ordinals.Add( ordinal, license.Key );
                this.WriteLicense( console.Out, ordinal, license.Key, license.Value );
                console.Out.WriteLine();
               
                ordinal++;
            }

            ordinals.Save();

            return 0;
        }

        private void WriteLicense( IStandardStreamWriter @out, int ordinal, string licenseKey, LicenseRegistrationData? data )
        {
            string? Format( DateTime? dateTime )
            {
                if ( dateTime == null )
                {
                    return null;
                }

                return dateTime.Value.ToString( "D", CultureInfo.InvariantCulture );
            }

            void Write( string description, string? value )
            {
                if ( value != null )
                {
                    @out.WriteLine( $"{description,24}: {value}" );
                }
            }

            Write( "Ordinal", ordinal.ToString() );
            Write( "License ID", data?.LicenseId?.ToString() );

            if ( data == null || data.LicenseId != null )
            {
                Write( "License Key", licenseKey );   
            }
            
            if ( data != null )
            {
                Write( "Description", data.Description );
                Write( "Licensee", data.Licensee );

                string? expiration = null;

                if ( data.Perpetual != null )
                {
                    if ( data.Perpetual.Value )
                    {
                        expiration = "Never (perpetual license)";
                    }
                    else
                    {
                        expiration = Format( data.ValidTo );
                    }
                }

                Write( "Subscription End Date", expiration );
                Write( "Maintenance Expiration", Format( data.SubscriptionEndDate ) );
            }
        }
    }
}
