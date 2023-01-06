// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Spectre.Console.Cli;
using System.ComponentModel;

namespace Metalama.Backstage.Commands.Commands;

public class CommonCommandSettings : CommandSettings
{
    [Description( "Makes the engineering tool verbose (but not msbuild or dotnet)." )]
    [CommandOption( "--verbose" )]
    public bool Verbose { get; protected set; }
}