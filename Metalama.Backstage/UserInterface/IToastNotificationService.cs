// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.UserInterface;

public interface IToastNotificationService : IBackstageService
{
    bool CanShow { get; }

    void Show( ToastNotification notification );

    /// <summary>
    /// Tries to acquire the right to display a notification, and updates the snooze period.
    /// </summary>
    bool TryAcquire( ToastNotificationKind kind );

    void Snooze( ToastNotificationKind kind );

    void Disable( ToastNotificationKind kind );
}