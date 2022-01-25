namespace PostSharp.Backstage.Diagnostics;

internal static class LoggerFactoryExtensions
{
    public static ILogger Licensing( this ILoggerFactory loggerFactory ) => loggerFactory.GetLogger( "Licensing" );

    public static ILogger Configuration( this ILoggerFactory loggerFactory ) => loggerFactory.GetLogger( "Configuration" );

    public static ILogger Test( this ILoggerFactory loggerFactory ) => loggerFactory.GetLogger( "Test" );

    public static ILogger Telemetry( this ILoggerFactory loggerFactory ) => loggerFactory.GetLogger( "Telemetry" );
}