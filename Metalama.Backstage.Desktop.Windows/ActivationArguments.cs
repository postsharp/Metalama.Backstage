// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Desktop.Windows.Commands;

namespace Metalama.Backstage.Desktop.Windows;

internal class ActivationArguments
{
    private readonly string _options;

    public ActivationArguments( BaseSettings settings )
    {
        this._options = settings.IsDevelopmentEnvironment ? "--dev" : "";
    }

    public string VsxInstall => $"activate {InstallVsxCommand.Name} {this._options}";

    public string VsxForget => $"activate {SnoozeVsxNotificationCommand.Name} {this._options}";

    public string VsxSnooze => $"activate {SnoozeVsxNotificationCommand.Name} {this._options}";

    public string Setup => $"activate {SetupWizardCommand.Name} {this._options}";
}