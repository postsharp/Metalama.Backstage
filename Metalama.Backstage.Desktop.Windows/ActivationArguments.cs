// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Desktop.Windows.Commands;

namespace Metalama.Backstage.Desktop.Windows;

internal static class ActivationArguments
{
    public const string VsxInstall = "activate " + InstallVsxCommand.Name;
    public const string VsxForget = "activate " + SnoozeVsxNotificationCommand.Name;
    public const string VsxSnooze = "activate " + SnoozeVsxNotificationCommand.Name;
    public const string LicenseActivate = "license/activate";
}