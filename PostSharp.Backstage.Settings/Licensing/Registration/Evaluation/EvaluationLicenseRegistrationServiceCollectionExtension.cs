// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Registration.Evaluation
{
    /// <summary>
    /// Extension methods for setting evaluation license files locations services in an <see cref="ServiceProviderBuilder" />.
    /// </summary>
    public static class EvaluationLicenseRegistrationServiceCollectionExtension
    {
        /// <summary>
        /// Adds evaluation license files locations service to the specified <see cref="ServiceProviderBuilder" />.
        /// </summary>
        /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
        /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
        internal static ServiceProviderBuilder AddStandardEvaluationLicenseFilesLocations( this ServiceProviderBuilder serviceProviderBuilder )
        {
            return serviceProviderBuilder
                .AddSingleton<IEvaluationLicenseFilesLocations>(
                        new EvaluationLicenseFilesLocations( serviceProviderBuilder.ServiceProvider ) );
        }

        public static ServiceProviderBuilder AddFirstRunEvaluationLicenseActivator( this ServiceProviderBuilder serviceProviderBuilder )
        {
            return serviceProviderBuilder
                .AddSingleton<IFirstRunLicenseActivator>(
                        new EvaluationLicenseRegistrar( serviceProviderBuilder.ServiceProvider ) );
        }
    }
}