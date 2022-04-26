// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Registration;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Globalization;

namespace Metalama.DotNetTools.Commands.Licensing
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
            var services = this.CommandServiceProvider.Initialize( console, verbose );
            var storage = ParsedLicensingConfiguration.OpenOrCreate( services );

            var index = 1;

            foreach ( var license in storage.Licenses )
            {
                this.WriteLicense( console.Out, index, license.LicenseString, license.LicenseData );
                console.Out.WriteLine();
                index++;
            }

            return 0;
        }

#pragma warning disable CA1822 // Member can be marked static
        private void WriteLicense(
            IStandardStreamWriter @out,
            int ordinal,
            string licenseKey,
            LicenseRegistrationData? data )
#pragma warning restore CA1822 // Member can be marked static
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

            Write( "Ordinal", ordinal.ToString( CultureInfo.InvariantCulture ) );
            Write( "License ID", data?.LicenseId?.ToString( CultureInfo.InvariantCulture ) );

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

                Write( "License Expiration", expiration );
                Write( "Maintenance Expiration", Format( data.SubscriptionEndDate ) );
            }
        }
    }
}