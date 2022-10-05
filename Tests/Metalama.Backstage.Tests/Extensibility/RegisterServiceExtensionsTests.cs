// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Metalama.Backstage.Licensing.Tests.Extensibility;

public class RegisterServiceExtensionsTests
{
    private static ServiceProviderBuilder CreateServiceProviderBuilder()
    {
        var serviceCollection = new ServiceCollection();

        return
            new ServiceProviderBuilder(
                ( type, instance ) => serviceCollection.AddSingleton( type, instance ),
                () => serviceCollection.BuildServiceProvider() );
    }

    [Fact]
    public void AddBackstageServices()
    {
        var serviceProviderBuilder = CreateServiceProviderBuilder().AddBackstageServices( new TestApplicationInfo( "Test", true, "1.0", DateTime.Today ) );
        serviceProviderBuilder.ServiceProvider.GetRequiredBackstageService<IPlatformInfo>();
    }
}