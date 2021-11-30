// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Registration.Evaluation
{
    /// <summary>
    /// Extension methods for setting evaluation license files locations services in an <see cref="BackstageServiceCollection" />.
    /// </summary>
    public static class EvaluationLicenseRegistrationServiceCollectionExtension
    {
        /// <summary>
        /// Adds evaluation license files locations service to the specified <see cref="BackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="BackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="BackstageServiceCollection" /> so that additional calls can be chained.</returns>
        internal static BackstageServiceCollection AddStandardEvaluationLicenseFilesLocations( this BackstageServiceCollection serviceCollection )
        {
            return serviceCollection
                .AddSingleton<IEvaluationLicenseFilesLocations>(
                    services =>
                        new EvaluationLicenseFilesLocations( services.ToServiceProvider() ) );
        }

        public static BackstageServiceCollection AddFirstRunEvaluationLicenseActivator( this BackstageServiceCollection serviceCollection )
        {
            return serviceCollection
                .AddSingleton<IFirstRunLicenseActivator>(
                    services =>
                        new EvaluationLicenseRegistrar( services.ToServiceProvider() ) );
        }
    }
}