namespace PostSharp.Backstage.Extensibility
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger<T>();
    }
}