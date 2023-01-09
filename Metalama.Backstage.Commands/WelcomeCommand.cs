// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Welcome;

namespace Metalama.Backstage.Commands;

[UsedImplicitly]
internal class WelcomeCommand : BaseCommand<BaseCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, BaseCommandSettings settings )
    {
        var welcomeService = new WelcomeService( context.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup();
        welcomeService.OpenWelcomePage();
    }
}