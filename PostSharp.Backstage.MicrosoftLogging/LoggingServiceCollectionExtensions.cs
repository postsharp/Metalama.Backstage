using PostSharp.Backstage.Extensibility;
using IMicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IPostSharpLoggerFactory = PostSharp.Backstage.Extensibility.ILoggerFactory;

namespace PostSharp.Backstage.MicrosoftLogging
{
    public static class LoggingServiceCollectionExtensions
    {
        public static BackstageServiceCollection AddMicrosoftLoggerFactory(
            this BackstageServiceCollection serviceCollection, IMicrosoftLoggerFactory microsoftLoggerFactory)
            => serviceCollection.AddSingleton<IPostSharpLoggerFactory>(new LoggerFactoryAdapter(microsoftLoggerFactory));
    }
}