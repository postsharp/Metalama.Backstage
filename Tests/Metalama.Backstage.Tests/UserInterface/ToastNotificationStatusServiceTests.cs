// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.UserInterface;

public class ToastNotificationStatusServiceTests : TestsBase
{
    public ToastNotificationStatusServiceTests( ITestOutputHelper logger, IApplicationInfo? applicationInfo = null ) :
        base( logger, applicationInfo ) { }

    [Fact]
    public void AutoSnooze()
    {
        // The first time should succeed.
        Assert.True( this.ToastNotificationsStatus.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // The second time should not.
        Assert.False( this.ToastNotificationsStatus.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // Advance the clock.
        this.Time.AddTime( ToastNotificationKinds.LicenseExpiring.AutoSnoozePeriod );

        // Now this should work again.
        Assert.True( this.ToastNotificationsStatus.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }

    [Fact]
    public void Disable()
    {
        this.ToastNotificationsStatus.Mute( ToastNotificationKinds.LicenseExpiring );
        Assert.False( this.ToastNotificationsStatus.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }

    [Fact]
    public void Snooze()
    {
        // Snooze before anything.
        this.ToastNotificationsStatus.Snooze( ToastNotificationKinds.LicenseExpiring );

        // This should be snoozed.
        Assert.False( this.ToastNotificationsStatus.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // Advance the clock.
        this.Time.AddTime( ToastNotificationKinds.LicenseExpiring.ManualSnoozePeriod );

        // Now this should work again.
        Assert.True( this.ToastNotificationsStatus.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }
}