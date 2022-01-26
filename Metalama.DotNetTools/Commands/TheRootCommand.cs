﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.DotNetTools.Commands.Licensing;
using Metalama.DotNetTools.Commands.Logging;
using Metalama.DotNetTools.Commands.Telemetry;
using System.CommandLine;

namespace Metalama.DotNetTools.Commands;

internal class TheRootCommand : RootCommand
{
    public TheRootCommand( ICommandServiceProvider commandServiceProvider )
        : base( "Manages user options of Metalama" )
    {
        this.Add( new LicenseCommand( commandServiceProvider ) );
        this.Add( new TelemetryCommand( commandServiceProvider ) );
        this.Add( new DiagnosticsCommand( commandServiceProvider ) );
        this.Add( new WelcomeCommand( commandServiceProvider ) );

        var verboseOption = new Option<bool>( "--verbose", "Set detailed verbosity level" );
        verboseOption.AddAlias( "-v" );
        this.AddGlobalOption( verboseOption );
    }
}