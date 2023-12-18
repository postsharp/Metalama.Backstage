// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Tools;
using System;

namespace Metalama.Backstage.UserInterface;

internal class WindowsToastNotificationService : ToastNotificationService
{
    private readonly IBackstageToolsExecutor _toolsExecutor;

    public override bool CanShow => true;

    public WindowsToastNotificationService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._toolsExecutor = serviceProvider.GetRequiredBackstageService<IBackstageToolsExecutor>();
    }

    protected override void ShowCore( ToastNotification notification )
    {
        // Build arguments.
        var arguments = $"notify {notification.Kind.Name}";

        if ( !string.IsNullOrEmpty( notification.Text ) )
        {
            arguments += $" --text \"{notification.Text}\"";
        }

        // Start the UI process.
        this._toolsExecutor.Start( BackstageTool.DesktopWindows, arguments );
    }
}