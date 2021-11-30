using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Registration.Evaluation
{
    /// <summary>
    /// Extension methods for setting evaluation license files locations services in an <see cref="IBackstageServiceCollection" />.
    /// </summary>
    public static class EvaluationLicenseRegistrationServiceCollectionExtension
    {
        /// <summary>
        /// Adds evaluation license files locations service to the specified <see cref="IBackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IBackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IBackstageServiceCollection" /> so that additional calls can be chained.</returns>
        internal static BackstageServiceCollection AddStandardEvaluationLicenseFilesLocations(
            this BackstageServiceCollection serviceCollection)
            => serviceCollection
                .AddSingleton<IEvaluationLicenseFilesLocations>(services =>
                    new EvaluationLicenseFilesLocations(services.ToServiceProvider()));

        public static BackstageServiceCollection AddFirstRunEvaluationLicenseActivator(
            this BackstageServiceCollection serviceCollection)
            => serviceCollection
                .AddSingleton<IFirstRunLicenseActivator>(services =>
                    new EvaluationLicenseRegistrar(services.ToServiceProvider()));
    }
}