// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Welcome;

namespace Metalama.Backstage.Commands.Commands;

internal class WelcomeCommand : CommandBase<CommonCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, CommonCommandSettings settings )
    {
        var welcomeService = new WelcomeService( context.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup();
        welcomeService.OpenWelcomePage();
    }
}