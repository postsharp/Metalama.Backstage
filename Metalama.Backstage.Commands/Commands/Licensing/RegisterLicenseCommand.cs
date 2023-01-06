// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;

namespace Metalama.Backstage.Commands.Commands.Licensing;

internal class RegisterLicenseCommand : CommandBase<RegisterLicenseCommandSettings>
{
    // TODO Reporting of license registration (#29957)

    protected override void Execute( ExtendedCommandContext context, RegisterLicenseCommandSettings settings )
    {
        var factory = new LicenseFactory( context.ServiceProvider );

        if ( !factory.TryCreate( settings.License, out var license, out var errorMessage )
             || !license.TryGetLicenseRegistrationData( out var data, out errorMessage ) )
        {
            throw new CommandException( $"Invalid license string: {errorMessage}" );
        }

        var storage = ParsedLicensingConfiguration.OpenOrCreate( context.ServiceProvider );

        storage.SetLicense( settings.License, data );

        context.Console.WriteSuccess( $"The license key '{context}' has been registered." );
    }
}