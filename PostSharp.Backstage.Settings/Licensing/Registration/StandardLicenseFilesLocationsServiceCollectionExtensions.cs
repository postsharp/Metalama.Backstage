// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration.Evaluation;

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Extension methods for setting standard license files locations services in an <see cref="BackstageServiceCollection" />.
    /// </summary>
    public static class StandardLicenseFilesLocationsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds license file location services to the specified <see cref="BackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="BackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="BackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static BackstageServiceCollection AddStandardLicenseFilesLocations( this BackstageServiceCollection serviceCollection )
        {
            return serviceCollection
                .AddSingleton<IStandardLicenseFileLocations>(
                    services =>
                        new StandardLicenseFilesLocations( services.ToServiceProvider() ) )
                .AddStandardEvaluationLicenseFilesLocations();
        }
    }
}