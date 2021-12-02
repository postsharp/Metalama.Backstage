// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Registration;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Extension methods for setting up license consumption services in an <see cref="ServiceProviderBuilder" />.
    /// </summary>
    public static class LicenseConsumptionServiceCollectionExtensions
    {
        /// <summary>
        /// Adds license file location and license consumption services to the specified <see cref="ServiceProviderBuilder" />.
        /// </summary>
        /// <param name="services">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
        /// <param name="licenseSources">License sources.</param>
        /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
        public static ServiceProviderBuilder AddLicenseConsumption(
            this ServiceProviderBuilder services,
            IEnumerable<ILicenseSource> licenseSources )
        {
            return services
                .AddStandardLicenseFilesLocations()
                .AddSingleton<ILicenseConsumptionManager>( new LicenseConsumptionManager( services.ServiceProvider, licenseSources ) );
        }

        /// <summary>
        /// Adds license file location and license consumption services to the specified <see cref="ServiceProviderBuilder" />.
        /// </summary>
        /// <param name="services">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
        /// <param name="licenseSources">License sources.</param>
        /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
        public static ServiceProviderBuilder AddLicenseConsumption(
            this ServiceProviderBuilder services,
            params ILicenseSource[] licenseSources )
        {
            return services
                .AddStandardLicenseFilesLocations()
                .AddSingleton<ILicenseConsumptionManager>( new LicenseConsumptionManager( services.ServiceProvider, licenseSources ) );
        }
    }
}