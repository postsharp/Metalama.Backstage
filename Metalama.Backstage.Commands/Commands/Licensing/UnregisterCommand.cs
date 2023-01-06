// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Registration;

namespace Metalama.Backstage.Commands.Commands.Licensing;

internal class UnregisterCommand : CommandBase<RegisterLicenseCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, RegisterLicenseCommandSettings settings )
    {
        var licenseStorage = ParsedLicensingConfiguration.OpenOrCreate( context.ServiceProvider );

        if ( string.IsNullOrEmpty( licenseStorage.LicenseString ) )
        {
            throw new CommandException( "A license is not registered." );
        }

        licenseStorage.RemoveLicense();
        context.Console.WriteSuccess( $"The license key '{context}' has been unregistered." );
    }
}