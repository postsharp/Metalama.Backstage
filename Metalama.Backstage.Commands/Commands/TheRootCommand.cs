// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.DotNetTools.Commands.DesignTime;
using Metalama.DotNetTools.Commands.Licensing;
using Metalama.DotNetTools.Commands.Logging;
using Metalama.DotNetTools.Commands.Maintenance;
using Metalama.DotNetTools.Commands.Telemetry;
using System.Collections.Immutable;
using System.CommandLine;

namespace Metalama.DotNetTools.Commands;

public static class BackstageCommandFactory
{
    public static ImmutableArray<Command> CreateCommands( ICommandServiceProviderProvider commandServiceProvider )
    {
        return ImmutableArray.Create<Command>(
            new LicenseCommand( commandServiceProvider ),
            new TelemetryCommand( commandServiceProvider ),
            new DiagnosticsCommand( commandServiceProvider ),
            new DesignTimeCommand( commandServiceProvider ),
            new CleanUpCommand( commandServiceProvider ),
            new KillCompilerServerCommand( commandServiceProvider ),
            new WelcomeCommand( commandServiceProvider ) );
    }
}