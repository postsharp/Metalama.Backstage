﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Tests.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing.Consumption;

public class LicenseSourcePriorityTests : LicensingTestsBase
{
    private const string _invalidProjectLicense = "invalid-project";

    private const string _invalidUserLicense = "invalid-user";

    private static readonly LicenseRequirement _testLicenseRequirement = LicenseRequirement.Ultimate;

    public LicenseSourcePriorityTests( ITestOutputHelper logger ) : base( logger ) { }

    protected override void ConfigureServices( ServiceProviderBuilder services ) { }

    private ILicenseConsumptionService CreateConsumptionManager( bool isUnattendedProcess, string? projectLicense, string? userLicense, bool isPreview )
    {
        var serviceCollection = this.CloneServiceCollection();

        var serviceProviderBuilder =
            new ServiceCollectionBuilder( serviceCollection );

        serviceProviderBuilder.AddSingleton<IApplicationInfoProvider>(
                new ApplicationInfoProvider(
                    new TestApplicationInfo( "License Source Priority Test App", isPreview, "1.0.0", new DateTime( 2022, 1, 1 ) )
                    {
                        IsUnattendedProcess = isUnattendedProcess
                    } ) )
            .AddConfigurationManager();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        if ( userLicense != null )
        {
            TestLicensingConfigurationHelpers.SetStoredLicenseString( serviceProvider, userLicense );
        }

        var options = new LicensingInitializationOptions() { ProjectLicense = projectLicense, DisableLicenseAudit = true };

        return LicenseConsumptionServiceFactory.Create( serviceProvider, options );
    }

    [Fact]
    public void NoMessageGivenWithNoLicense()
    {
        var licenseConsumptionManager = this.CreateConsumptionManager( false, null, null, false );
        Assert.False( licenseConsumptionManager.CanConsume( _testLicenseRequirement ) );
        Assert.Empty( licenseConsumptionManager.Messages );
    }

    [Fact]
    public void UnattendedLicenseHasHighestPriority()
    {
        // We don't pass an invalid project license, because project license disables unattended license.
        var licenseConsumptionManager = this.CreateConsumptionManager( true, null, _invalidUserLicense, false );
        Assert.True( licenseConsumptionManager.CanConsume( _testLicenseRequirement ) );
        Assert.Empty( licenseConsumptionManager.Messages );
    }

    [Fact]
    public void ProjectLicenseHasPriorityOverUserLicense()
    {
        var licenseConsumptionManager = this.CreateConsumptionManager( false, _invalidProjectLicense, _invalidUserLicense, false );
        Assert.False( licenseConsumptionManager.CanConsume( _testLicenseRequirement ) );
        Assert.Contains( _invalidProjectLicense, licenseConsumptionManager.Messages[0].Text, StringComparison.OrdinalIgnoreCase );
    }

    [Fact]
    public void UserLicenseHasPriorityOverPreviewLicense()
    {
        var licenseConsumptionManager = this.CreateConsumptionManager( false, null, _invalidUserLicense, true );
        Assert.False( licenseConsumptionManager.CanConsume( _testLicenseRequirement ) );
        Assert.Contains( _invalidUserLicense, licenseConsumptionManager.Messages[0].Text, StringComparison.OrdinalIgnoreCase );
    }
}