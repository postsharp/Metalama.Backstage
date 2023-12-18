// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.UserInterface;
using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace Metalama.Backstage.Desktop.Windows.Commands;

internal class SetupWizardCommand : AsyncCommand<BaseSettings>
{
    public const string Name = "setup";

    public override async Task<int> ExecuteAsync( CommandContext context, BaseSettings settings )
    {
        // Start the web server.
        var serviceProvider = App.GetBackstageServices( settings );
        var userInterfaceService = serviceProvider.GetRequiredBackstageService<IUserInterfaceService>();

        await userInterfaceService.OpenConfigurationWebPageAsync( "Setup" );

        return 0;
    }
}