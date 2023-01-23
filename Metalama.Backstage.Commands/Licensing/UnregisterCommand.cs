// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Registration;

namespace Metalama.Backstage.Commands.Licensing;

internal class UnregisterCommand : BaseCommand<BaseCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, BaseCommandSettings settings )
    {
        var licenseStorage = ParsedLicensingConfiguration.OpenOrCreate( context.ServiceProvider );

        if ( string.IsNullOrEmpty( licenseStorage.LicenseString ) )
        {
            throw new CommandException( "A license is not registered." );
        }

        var licenseString = licenseStorage.LicenseString;
        licenseStorage.RemoveLicense();
        context.Console.WriteSuccess( $"The license key '{licenseString}' has been unregistered." );
    }
}