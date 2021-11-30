using System;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands.Licensing
{
    public abstract class LicensingCommandsTestsBase : CommandsTestsBase
    {
        protected LicensingCommandsTestsBase(ITestOutputHelper logger,
            Action<BackstageServiceCollection>? serviceBuilder = null)
            : base(
                logger,
                serviceCollection =>
                {
                    serviceCollection.AddStandardLicenseFilesLocations();

                    serviceBuilder?.Invoke(serviceCollection);
                })
        {
        }
    }
}