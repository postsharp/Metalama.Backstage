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
        public ListCommand( IServicesFactory servicesFactory )
            : base( servicesFactory, "list", "Lists registered licenses" )
        {
            this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        }

        private int Execute( bool verbose, IConsole console )
        {
            (var services, var trace) = this.ServicesFactory.Create( console, verbose );

            var storage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, services, trace );

            var ordinal = 1;
            LicenseStringsOrdinalDictionary ordinals = new( services );

            foreach ( var license in storage.Licenses )
            {
                ordinals.Add( ordinal, license.Key );
                console.Out.Write( $"({ordinal})" );
                ordinal++;

                if ( license.Value == null )
                {
                    console.Out.WriteLine( $" {license.Key}" );
                    continue;
                }

                this.WriteLicense( console.Out, license.Value );
                console.Out.WriteLine();
            }

            ordinals.Save();

            return 0;
        }

        private void WriteLicense( IStandardStreamWriter @out, LicenseRegistrationData data )
        {
            string? TryFormat( DateTime? dateTime )
            {
                if ( dateTime == null )
                {
                    return null;
                }

                return dateTime.Value.ToString( "D", CultureInfo.InvariantCulture );
            }

            void TryWrite( string description, string? value )
            {
                if ( value != null )
                {
                    @out.WriteLine( $"{description}: {value}" );
                }
            }

            @out.WriteLine( $" {data.Description}" );
            TryWrite( "License ID", data.LicenseId?.ToString() );
            TryWrite( "Licensee", data.Licensee );

            string? expiration = null;

            if ( data.Perpetual != null )
            {
                if ( data.Perpetual.Value )
                {
                    expiration = "Never (perpetual license)";
                }
                else
                {
                    expiration = TryFormat( data.ValidTo );
                }
            }

            TryWrite( "Subscription End Date", expiration );
            TryWrite( "Maintenance Expiration", TryFormat( data.SubscriptionEndDate ) );
        }
    }
}
