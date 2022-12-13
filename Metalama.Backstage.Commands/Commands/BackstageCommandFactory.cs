// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands.Commands.Configuration;
using Metalama.Backstage.Commands.Commands.Licensing;
using Metalama.Backstage.Commands.Commands.Maintenance;
using Metalama.Backstage.Commands.Commands.Telemetry;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.CommandLine;

namespace Metalama.Backstage.Commands.Commands;

public static class BackstageCommandFactory
{
    public static ImmutableArray<Command> CreateCommands( ICommandServiceProviderProvider commandServiceProvider )
    {
        return ImmutableArray.Create<Command>(
            new ConfigurationCommand( commandServiceProvider ),
            new CleanUpCommand( commandServiceProvider ),
            new LicenseCommand( commandServiceProvider ),
            new TelemetryCommand( commandServiceProvider ),
            new KillCompilerServerCommand( commandServiceProvider ),
            new WelcomeCommand( commandServiceProvider ) );
    }

    public static Dictionary<string, ConfigurationFile> ConfigurationFilesByCategory { get; set; } = new Dictionary<string, ConfigurationFile>()
    {
        { "diag", new DiagnosticsConfiguration() }
    };
}