// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Registration;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Extension methods for setting up license consumption services in an <see cref="BackstageServiceCollection" />.
    /// </summary>
    internal static class LicenseConsumptionServiceCollectionExtensions
    {
        /// <summary>
        /// Adds license file location and license consumption services to the specified <see cref="BackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="BackstageServiceCollection" /> to add services to.</param>
        /// <param name="licenseSources">License sources.</param>
        /// <returns>The <see cref="BackstageServiceCollection" /> so that additional calls can be chained.</returns>
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
        /// Adds license file location and license consumption services to the specified <see cref="BackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="BackstageServiceCollection" /> to add services to.</param>
        /// <param name="licenseSources">License sources.</param>
        /// <returns>The <see cref="BackstageServiceCollection" /> so that additional calls can be chained.</returns>
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