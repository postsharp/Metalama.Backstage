// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.DotNetTools.Commands.Licensing;
using Metalama.DotNetTools.Commands.Logging;
using Metalama.DotNetTools.Commands.Maintenance;
using Metalama.DotNetTools.Commands.Telemetry;
using System.CommandLine;

namespace Metalama.DotNetTools.Commands;

internal class TheRootCommand : RootCommand
{
    public TheRootCommand( ICommandServiceProviderProvider commandServiceProvider )
        : base( "Manages user options of Metalama" )
    {
        this.Add( new LicenseCommand( commandServiceProvider ) );
        this.Add( new TelemetryCommand( commandServiceProvider ) );
        this.Add( new DiagnosticsCommand( commandServiceProvider ) );
        this.Add( new CleanUpCommand( commandServiceProvider ) );
        this.Add( new WelcomeCommand( commandServiceProvider ) );

        var verboseOption = new Option<bool>( "--verbose", "Set detailed verbosity level" );
        verboseOption.AddAlias( "-v" );
        this.AddGlobalOption( verboseOption );
    }
}