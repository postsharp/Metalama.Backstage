using System.Collections.Generic;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Extension methods for setting up license consumption services in an <see cref="IBackstageServiceCollection" />.
    /// </summary>
    internal static class LicenseConsumptionServiceCollectionExtensions
    {
        /// <summary>
        /// Adds license file location and license consumption services to the specified <see cref="IBackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IBackstageServiceCollection" /> to add services to.</param>
        /// <param name="licenseSources">License sources.</param>
        /// <returns>The <see cref="IBackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static BackstageServiceCollection AddLicenseConsumption(
            this BackstageServiceCollection serviceCollection,
            IEnumerable<ILicenseSource> licenseSources )
        {
            return serviceCollection
                .AddStandardLicenseFilesLocations()
                .AddSingleton<ILicenseConsumptionManager>(
                    services =>
                        new LicenseConsumptionManager( services.ToServiceProvider(), licenseSources ) );
        }

        /// <summary>
        /// Adds license file location and license consumption services to the specified <see cref="IBackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IBackstageServiceCollection" /> to add services to.</param>
        /// <param name="licenseSources">License sources.</param>
        /// <returns>The <see cref="IBackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static BackstageServiceCollection AddLicenseConsumption(
            this BackstageServiceCollection serviceCollection,
            params ILicenseSource[] licenseSources )
        {
            return serviceCollection
                .AddStandardLicenseFilesLocations()
                .AddSingleton<ILicenseConsumptionManager>(
                    services =>
                        new LicenseConsumptionManager( services.ToServiceProvider(), licenseSources ) );
        }
    }
}