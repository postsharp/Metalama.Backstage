// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Metalama.Backstage.Commands.Commands.Configuration;

public abstract class ConfigurationFileCommandAdapter
{
    protected ConfigurationFileCommandAdapter() { }

    public abstract string Alias { get; }

    public virtual string? EnvironmentVariableName => null;

    internal abstract void Print( ExtendedCommandContext context );

    internal abstract void Reset( ExtendedCommandContext context );

    internal abstract void Edit( ExtendedCommandContext context );

    internal abstract string GetFilePath( IServiceProvider contextServiceProvider );
}

#pragma warning disable SA1402

public class ConfigurationFileCommandAdapter<T> : ConfigurationFileCommandAdapter
    where T : ConfigurationFile, new()
{
    private readonly ConfigurationFileAttribute _attribute;

    public ConfigurationFileCommandAdapter()
    {
        this._attribute = typeof(T).GetCustomAttribute<ConfigurationFileAttribute>()
                          ?? throw new InvalidOperationException(
                              $"'{nameof(ConfigurationFileAttribute)}' custom attribute not found for '{typeof(T).FullName}' type." );
    }

    public override string Alias => this._attribute.Alias;

    public override string? EnvironmentVariableName => this._attribute.EnvironmentVariableName;

    internal override void Print( ExtendedCommandContext context )
    {
        var configurationManager = context.ServiceProvider.GetRequiredService<IConfigurationManager>();

        var configuration = configurationManager.Get( typeof(T) );
        var filePath = configurationManager.GetFilePath( typeof(T) );

        context.Console.WriteMessage( $"The file '{filePath}' contains the following configuration:" );
        context.Console.WriteMessage( "" );
        context.Console.WriteMessage( configuration.ToJson() );
    }

    internal override void Reset( ExtendedCommandContext context )
    {
        var configurationManager = context.ServiceProvider.GetRequiredService<IConfigurationManager>();

        configurationManager.Update<T>( _ => new T() );

        context.Console.WriteSuccess( $"The configuration '{this.Alias}' has been reset." );
    }

    internal override void Edit( ExtendedCommandContext context )
    {
        var configurationManager = context.ServiceProvider.GetRequiredService<IConfigurationManager>();

        configurationManager.CreateIfMissing<T>();

        var filePath = configurationManager.GetFilePath<T>();
        context.Console.WriteSuccess( $"Opening '{filePath}' in the default editor." );

        Process.Start( new ProcessStartInfo( filePath ) { UseShellExecute = true } );
    }

    internal override string GetFilePath( IServiceProvider serviceProvider )
    {
        var configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();

        return configurationManager.GetFilePath<T>();
    }
}