// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands.Commands.Configuration;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;

namespace Metalama.Backstage.Commands;

public sealed class BackstageCommandOptions
{
    private readonly AnsiSupport _ansiSupport;
    private readonly Dictionary<string, ConfigurationFileCommandAdapter> _configurationFileCommandAdapters = new();

    public BackstageCommandOptions(
        ICommandServiceProviderProvider? serviceProvider = null,
        TextWriter? standardOutput = null,
        TextWriter? errorOutput = null,
        AnsiSupport ansiSupport = AnsiSupport.Detect )
    {
        this._ansiSupport = ansiSupport;
        this.ServiceProvider = serviceProvider ?? new CommandServiceProvider();
        this.StandardOutput = standardOutput ?? Console.Out;
        this.ErrorOutput = errorOutput ?? Console.Error;
        this.AddConfigurationFileAdapter<DiagnosticsConfiguration>();
    }

    internal void ConfigureConsole( AnsiConsoleSettings settings )
    {
        settings.Ansi = this._ansiSupport;
        settings.Interactive = InteractionSupport.No;
    }

    public ICommandServiceProviderProvider ServiceProvider { get; }

    public TextWriter StandardOutput { get; }

    public TextWriter ErrorOutput { get; }

    public IReadOnlyDictionary<string, ConfigurationFileCommandAdapter> ConfigurationFileCommandAdapters => this._configurationFileCommandAdapters;

    public void AddConfigurationFileAdapter( ConfigurationFileCommandAdapter adapter )
    {
        this._configurationFileCommandAdapters.Add( adapter.Alias, adapter );
    }

    public void AddConfigurationFileAdapter<T>()
        where T : ConfigurationFile, new()
    {
        var adapter = new ConfigurationFileCommandAdapter<T>();
        this._configurationFileCommandAdapters.Add( adapter.Alias, adapter );
    }
}