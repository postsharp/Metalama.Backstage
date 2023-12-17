// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Notifications;

public interface IToastNotificationService : IBackstageService
{
    void Show( ToastNotification notification );

    void Snooze( ToastNotificationKind kind );

    void Disable( ToastNotificationKind kind );
}

internal class ToastNotificationService : IToastNotificationService
{
    public void Show( ToastNotification notification ) => throw new NotImplementedException();

    public void Snooze( ToastNotificationKind kind ) => throw new NotImplementedException();

    public void Disable( ToastNotificationKind kind ) => throw new NotImplementedException();
}