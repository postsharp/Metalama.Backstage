using System;

namespace PostSharp.Backstage.Extensibility
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}