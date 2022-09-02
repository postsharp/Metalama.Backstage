// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

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
        public ListCommand( ICommandServiceProviderProvider commandServiceProvider )
            : base( commandServiceProvider, "list", "Lists registered license" )
        {
            this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        }

        private int Execute( bool verbose, IConsole console )
        {
            this.CommandServices.Initialize( console, verbose );
            var storage = ParsedLicensingConfiguration.OpenOrCreate( this.CommandServices.ServiceProvider );

            if ( storage.LicenseString != null )
            {
                this.WriteLicense( console.Out, storage.LicenseString, storage.LicenseData );
                console.Out.WriteLine();
            }

            return 0;
        }

#pragma warning disable CA1822 // Member can be marked static
        private void WriteLicense(
            IStandardStreamWriter @out,
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