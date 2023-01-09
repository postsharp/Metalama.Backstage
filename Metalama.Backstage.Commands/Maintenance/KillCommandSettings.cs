﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Spectre.Console.Cli;
using System.ComponentModel;

namespace Metalama.Backstage.Commands.Maintenance;

internal class KillCommandSettings : BaseCommandSettings
{
    [Description( "Does not write warnings for processes that may be locking Metalama files." )]
    [CommandOption( "--no-warn" )]
    public bool NoWarn { get; init; }
}