// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Registration;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Licensing;

internal class UnregisterCommand : CommandBase
{
    public UnregisterCommand( ICommandServiceProviderProvider commandServiceProvider )
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
        this.CommandServices.Initialize( console, verbose );

        var licenseStorage = ParsedLicensingConfiguration.OpenOrCreate( this.CommandServices.ServiceProvider );

        if ( int.TryParse( licenseKeyOrOrdinal, out var ordinal ) )
        {
            return UnregisterOrdinal( ordinal, licenseStorage, console );
        }
        else
        {
            return UnregisterLicense( licenseKeyOrOrdinal, licenseStorage, console );
        }
    }

    private static int UnregisterOrdinal(
        int ordinal,
        ParsedLicensingConfiguration licensingConfiguration,
        IConsole console )
    {
        if ( ordinal <= 0 || ordinal > licensingConfiguration.Licenses.Count )
        {
            console.Error.WriteLine( "Invalid ordinal." );

            return 1;
        }

        var license = licensingConfiguration.Licenses[ordinal - 1];

        return UnregisterLicense( license.LicenseString, licensingConfiguration, console );
    }

    private static int UnregisterLicense(
        string licenseString,
        ParsedLicensingConfiguration licensingConfiguration,
        IConsole console )
    {
        if ( !licensingConfiguration.RemoveLicense( licenseString ) )
        {
            console.Error.WriteLine( "This license is not registered." );

            return 2;
        }

        console.Out.WriteLine( $"{licenseString} unregistered." );

        return 0;
    }
}