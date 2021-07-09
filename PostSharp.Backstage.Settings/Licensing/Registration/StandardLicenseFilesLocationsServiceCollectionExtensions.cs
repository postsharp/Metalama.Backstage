// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Licensing.Registration.Evaluation;

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Extension methods for setting standard license files locations services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class StandardLicenseFilesLocationsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds license file location services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddStandardLicenseFilesLocations( this IServiceCollection serviceCollection )
            => serviceCollection
                .AddSingleton<IStandardLicenseFileLocations>( services => new StandardLicenseFilesLocations( services ) )
                .AddStandardEvaluationLicenseFilesLocations();
    }
}