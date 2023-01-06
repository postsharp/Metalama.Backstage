// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Registration.Free;

namespace Metalama.Backstage.Commands.Commands.Licensing;

internal class RegisterFreeCommand : CommandBase<CommonCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, CommonCommandSettings settings )
    {
        var registrar = new FreeLicenseRegistrar( context.ServiceProvider );

        if ( !registrar.TryRegisterLicense() )
        {
            throw new CommandException( "Cannot switch to the Metalama Free." );
        }
        else
        {
            context.Console.WriteSuccess( "You are now using Metalama Free edition." );
        }
    }
}