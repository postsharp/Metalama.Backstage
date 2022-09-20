// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Licensing;

internal class RegisterCommand : CommandBase
{
    // TODO Reporting of license registration (#29957)

    public RegisterCommand( ICommandServiceProviderProvider commandServiceProvider )
        : base(
            commandServiceProvider,
            "register",
            "Registers a license key, starts the trial period, switch to the Metalama Free" )
    {
        this.AddArgument(
            new Argument<string>(
                "license-key-or-type",
                "The license key to be registered, or 'trial' or 'free'" ) );

        this.AddCommand( new RegisterTrialCommand( commandServiceProvider ) );
        this.AddCommand( new RegisterFreeCommand( commandServiceProvider ) );

        this.Handler = CommandHandler.Create<string, bool, IConsole>( this.Execute );
    }

    private int Execute( string licenseKey, bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        var factory = new LicenseFactory( this.CommandServices.ServiceProvider );

        if ( !factory.TryCreate( licenseKey, out var license )
             || !license.TryGetLicenseRegistrationData( out var data ) )
        {
            console.Error.WriteLine( "Invalid license string." );

            return 1;
        }

        var storage = ParsedLicensingConfiguration.OpenOrCreate( this.CommandServices.ServiceProvider );

        storage.SetLicense( licenseKey, data );

        return 0;
    }
}