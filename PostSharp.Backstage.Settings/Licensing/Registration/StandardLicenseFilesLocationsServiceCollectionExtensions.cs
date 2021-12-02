// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration.Evaluation;

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Extension methods for setting standard license files locations services in an <see cref="ServiceProviderBuilder" />.
    /// </summary>
    public static class StandardLicenseFilesLocationsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds license file location services to the specified <see cref="ServiceProviderBuilder" />.
        /// </summary>
        /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
        /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
        public static ServiceProviderBuilder AddStandardLicenseFilesLocations( this ServiceProviderBuilder serviceProviderBuilder )
        {
            return serviceProviderBuilder
                .AddSingleton<IStandardLicenseFileLocations>( new StandardLicenseFilesLocations( serviceProviderBuilder.ServiceProvider ) )
                .AddStandardEvaluationLicenseFilesLocations();
        }
    }
}