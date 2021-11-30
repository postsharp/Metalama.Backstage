using System;
using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Testing;
using PostSharp.Backstage.Testing.Services;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests
{
    public abstract class LicensingTestsBase : TestsBase
    {
        private protected LicenseFactory LicenseFactory { get; }

        private protected UnsignedLicenseFactory SelfSignedLicenseFactory { get; }

        private protected TestDiagnosticsSink Diagnostics { get; }

        private protected IStandardLicenseFileLocations LicenseFiles { get; }

        private protected LicensingTestsBase(ITestOutputHelper logger,
            Action<BackstageServiceCollection>? serviceBuilder = null)
            : base(
                logger,
                serviceCollection =>
                {
                    // ReSharper disable once ExplicitCallerInfoArgument
                    serviceCollection.AddSingleton<IDiagnosticsSink>(services =>
                        new TestDiagnosticsSink(services.ToServiceProvider(), "default"));
                    serviceCollection.AddSingleton<IApplicationInfo>(
                        new TestApplicationInfo(
                            "Licensing Test App",
                            false,
                            new Version(0, 1, 0),
                            new DateTime(2021, 1, 1)));
                    serviceCollection.AddStandardLicenseFilesLocations();

                    serviceBuilder?.Invoke(serviceCollection);
                })
        {
            LicenseFactory = new LicenseFactory(Services);
            SelfSignedLicenseFactory = new UnsignedLicenseFactory(Services);
            Diagnostics = (TestDiagnosticsSink)Services.GetRequiredService<IDiagnosticsSink>();
            LicenseFiles = Services.GetRequiredService<IStandardLicenseFileLocations>();
        }
    }
}