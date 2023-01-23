// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Registration.Evaluation;

namespace Metalama.Backstage.Commands.Licensing;

internal class RegisterTrialCommand : BaseCommand<BaseCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, BaseCommandSettings settings )
    {
        var registrar = new EvaluationLicenseRegistrar( context.ServiceProvider );

        if ( !registrar.TryActivateLicense() )
        {
            throw new CommandException( "Cannot start the trial period." );
        }

        context.Console.WriteSuccess( "You are now using Metalama Trial." );
    }
}