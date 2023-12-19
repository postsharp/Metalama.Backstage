// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.UserInterface;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Testing;

public class TestToastNotificationStatusService : ToastNotificationStatusService
{
    public TestToastNotificationStatusService( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    public override bool CanShow => true;

    public List<ToastNotification> Notifications { get; } = new();

    protected override void ShowCore( ToastNotification notification ) => this.Notifications.Add( notification );
}