// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands.Commands.Configuration;
using Metalama.Backstage.Commands.Commands.Licensing;
using Metalama.Backstage.Commands.Commands.Maintenance;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Telemetry;
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
            new KillCompilerServerCommand( commandServiceProvider ),
            new WelcomeCommand( commandServiceProvider ) );
    }

    public static readonly Dictionary<string, ConfigurationFile> ConfigurationCategories
        = new Dictionary<string, ConfigurationFile>()
        {
            { "diag", new DiagnosticsConfiguration() },
            { "design-time", new DiagnosticsConfiguration() }, // TODO: Replace this with DesignTimeConfiguration().
            { "telemetry", new TelemetryConfiguration() },
        };
}