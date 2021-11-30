using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration.Evaluation;

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Extension methods for setting standard license files locations services in an <see cref="IBackstageServiceCollection" />.
    /// </summary>
    public static class StandardLicenseFilesLocationsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds license file location services to the specified <see cref="IBackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IBackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IBackstageServiceCollection" /> so that additional calls can be chained.</returns>
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