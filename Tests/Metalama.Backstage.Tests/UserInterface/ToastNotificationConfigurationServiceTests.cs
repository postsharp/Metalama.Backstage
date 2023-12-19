// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.UserInterface;

public class ToastNotificationConfigurationServiceTests : TestsBase
{
    public ToastNotificationConfigurationServiceTests( ITestOutputHelper logger, IApplicationInfo? applicationInfo = null ) :
        base( logger, applicationInfo ) { }

    [Fact]
    public void AutoSnooze()
    {
        // The first time should succeed.
        Assert.True( this.ToastNotificationsConfiguration.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // The second time should not.
        Assert.False( this.ToastNotificationsConfiguration.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // Advance the clock.
        this.Time.AddTime( ToastNotificationKinds.LicenseExpiring.AutoSnoozePeriod );

        // Now this should work again.
        Assert.True( this.ToastNotificationsConfiguration.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }

    [Fact]
    public void Disable()
    {
        this.ToastNotificationsConfiguration.Mute( ToastNotificationKinds.LicenseExpiring );
        Assert.False( this.ToastNotificationsConfiguration.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }

    [Fact]
    public void Snooze()
    {
        // Snooze before anything.
        this.ToastNotificationsConfiguration.Snooze( ToastNotificationKinds.LicenseExpiring );

        // This should be snoozed.
        Assert.False( this.ToastNotificationsConfiguration.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // Advance the clock.
        this.Time.AddTime( ToastNotificationKinds.LicenseExpiring.AutoSnoozePeriod );

        // Now this should work again.
        Assert.True( this.ToastNotificationsConfiguration.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }
}