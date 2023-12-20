// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.UserInterface;

public class ToastNotificationStatusServiceTests : TestsBase
{
    private readonly IToastNotificationStatusService _toastService;

    public ToastNotificationStatusServiceTests( ITestOutputHelper logger, IApplicationInfo? applicationInfo = null ) :
        base( logger, applicationInfo )
    {
        this._toastService = this.ServiceProvider.GetRequiredBackstageService<IToastNotificationStatusService>();
    }

    
    [Fact]
    public void AutoSnooze()
    {
        // The first time should succeed.
        Assert.True( this._toastService.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // The second time should not.
        Assert.False( this._toastService.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // Advance the clock.
        this.Time.AddTime( ToastNotificationKinds.LicenseExpiring.AutoSnoozePeriod );

        // Now this should work again.
        Assert.True( this._toastService.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }

    [Fact]
    public void Disable()
    {
        this._toastService.Mute( ToastNotificationKinds.LicenseExpiring );
        Assert.False( this._toastService.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }

    [Fact]
    public void Snooze()
    {
        // Snooze before anything.
        this._toastService.Snooze( ToastNotificationKinds.LicenseExpiring );

        // This should be snoozed.
        Assert.False( this._toastService.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
        
        // Advance the clock to the auto snooze. It should still be snoozed.
        this.Time.AddTime( ToastNotificationKinds.LicenseExpiring.AutoSnoozePeriod );
        Assert.False( this._toastService.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );

        // Advance the clock to the manual snooze. Now this should work again.
        this.Time.AddTime( ToastNotificationKinds.LicenseExpiring.ManualSnoozePeriod );
        Assert.True( this._toastService.TryAcquire( ToastNotificationKinds.LicenseExpiring ) );
    }
}