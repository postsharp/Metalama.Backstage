﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Spectre.Console.Cli;

namespace Metalama.Backstage.Desktop.Windows.Commands;

public class BaseSettings : CommandSettings
{
    [CommandOption( "--dev" )]
    public bool IsDevelopmentEnvironment { get; init; }
}