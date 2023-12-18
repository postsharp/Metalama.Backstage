// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.UserInterface;

public class ToastNotificationServiceTests : TestsBase
{
    public ToastNotificationServiceTests( ITestOutputHelper logger, IApplicationInfo? applicationInfo = null ) : base( logger, applicationInfo ) { }

    [Fact]
    public void AutoSnooze()
    {
        // The first time should succeed.
        Assert.True( this.ToastNotifications.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // The second time should not.
        Assert.False( this.ToastNotifications.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // Advance the clock.
        this.Time.AddTime( ToastNotificationKinds.LicenseExpiring.AutoSnoozePeriod );

        // Now this should work again.
        Assert.True( this.ToastNotifications.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }

    [Fact]
    public void Disable()
    {
        this.ToastNotifications.Disable( ToastNotificationKinds.LicenseExpiring );
        Assert.False( this.ToastNotifications.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }

    [Fact]
    public void Snooze()
    {
        // Snooze before anything.
        this.ToastNotifications.Snooze( ToastNotificationKinds.LicenseExpiring );

        // This should be snoozed.
        Assert.False( this.ToastNotifications.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // Advance the clock.
        this.Time.AddTime( ToastNotificationKinds.LicenseExpiring.AutoSnoozePeriod );

        // Now this should work again.
        Assert.True( this.ToastNotifications.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }
}