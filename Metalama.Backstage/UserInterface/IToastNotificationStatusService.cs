// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.UserInterface;

public interface IToastNotificationStatusService : IBackstageService
{
    /// <summary>
    /// Tries to acquire the right to display a notification, and updates the snooze period.
    /// </summary>
    bool TryAcquire( ToastNotificationKind kind );

    void Snooze( ToastNotificationKind kind );

    void Mute( ToastNotificationKind kind );

    /// <summary>
    /// Pause all notifications. This is used when the VSX setup wizard is scheduled or open, and
    /// should take care of activation.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> during which notifications should be paused.</param>
    void PauseAll( TimeSpan timeSpan );

    void ResumeAll();
}