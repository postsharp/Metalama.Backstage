// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Registration;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Licensing;

internal class UnregisterCommand : CommandBase
{
    public UnregisterCommand( ICommandServiceProviderProvider commandServiceProvider )
        : base( commandServiceProvider, "unregister", "Unregisters the registered license" )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
    }

    private int Execute( bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        var licenseStorage = ParsedLicensingConfiguration.OpenOrCreate( this.CommandServices.ServiceProvider );

        if ( string.IsNullOrEmpty( licenseStorage.LicenseString ) )
        {
            console.Error.WriteLine( "A license is not registered." );

            return 2;
        }

        licenseStorage.RemoveLicense();
        console.Out.WriteLine( $"The license has been unregistered." );

        return 0;
    }
}