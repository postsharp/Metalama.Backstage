// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Spectre.Console.Cli;

namespace Metalama.Backstage.Worker.WebServer;

internal class WebServerCommandSettings : CommandSettings
{
    [CommandOption( "--port" )]
    public int Port { get; set; } = 5252;
}