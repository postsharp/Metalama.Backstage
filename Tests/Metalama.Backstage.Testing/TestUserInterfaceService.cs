// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.UserInterface;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Testing;

public class TestUserInterfaceService : UserInterfaceService
{
    public List<ToastNotificationKind> Notifications { get; } = new();

    public TestUserInterfaceService( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    protected override void Notify( ToastNotificationKind kind, ref bool notificationReported )
    {
        this.Notifications.Add( kind );
        notificationReported = true;
    }
}