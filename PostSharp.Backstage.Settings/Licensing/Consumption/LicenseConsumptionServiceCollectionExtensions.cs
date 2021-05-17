// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Extension methods for setting up license consumption services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class LicenseConsumptionServiceCollectionExtensions
    {
        /// <summary>
        /// Adds license file location and license consumption services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="licenseSources">License sources.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddLicenseConsumption( this IServiceCollection serviceCollection, IEnumerable<ILicenseSource> licenseSources )
            => serviceCollection
                .AddStandardLicenseFilesLocations()
                .AddSingleton<ILicenseConsumptionManager>( services => new LicenseConsumptionManager( services, licenseSources ) );

        /// <summary>
        /// Adds license file location and license consumption services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="licenseSources">License sources.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddLicenseConsumption( this IServiceCollection serviceCollection, params ILicenseSource[] licenseSources )
            => serviceCollection
                .AddStandardLicenseFilesLocations()
                .AddSingleton<ILicenseConsumptionManager>( services => new LicenseConsumptionManager( services, licenseSources ) );
    }
}
