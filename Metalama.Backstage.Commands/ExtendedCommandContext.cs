// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Spectre.Console.Cli;
using System;

namespace Metalama.Backstage.Commands;

public sealed class ExtendedCommandContext
{
    public CommandContext CommandContext { get; }

    public ConsoleWriter Console { get; }

    public IServiceProvider ServiceProvider { get; }

    public BackstageCommandOptions BackstageCommandOptions { get; }

    internal ExtendedCommandContext( CommandContext commandContext, BaseCommandSettings settings )
    {
        this.CommandContext = commandContext;
        this.BackstageCommandOptions = (BackstageCommandOptions) commandContext.Data!;
        this.Console = new ConsoleWriter( this.BackstageCommandOptions );
        this.ServiceProvider = this.BackstageCommandOptions.ServiceProvider.GetServiceProvider( this.Console, settings.Verbose );
    }
}