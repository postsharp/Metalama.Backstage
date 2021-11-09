// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Registration.Evaluation
{
    /// <summary>
    /// Extension methods for setting evaluation license files locations services in an <see cref="IBackstageServiceCollection" />.
    /// </summary>
    internal static class EvaluationLicenseRegistrationServiceCollectionExtension
    {
        /// <summary>
        /// Adds evaluation license files locations service to the specified <see cref="IBackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IBackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IBackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static IBackstageServiceCollection AddStandardEvaluationLicenseFilesLocations( this IBackstageServiceCollection serviceCollection )
            => serviceCollection
                .AddSingleton<IEvaluationLicenseFilesLocations>( services => new EvaluationLicenseFilesLocations( services ) );
    }
}