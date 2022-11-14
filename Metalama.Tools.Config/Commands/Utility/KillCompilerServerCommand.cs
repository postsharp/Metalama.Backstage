// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Utility;

internal class KillCompilerServerCommand : CommandBase
{
    public KillCompilerServerCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "compiler-shutdown",
        "Tells VBCSCompiler process to shutdown." )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
    }

    private void Execute( bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        console.Out.WriteLine( "Attempting to kill VBCSCompiler process. " );

        var processManager = this.CommandServices.ServiceProvider.GetRequiredService<IProcessManager>();

        var success = processManager.KillVBCSCompilerProcess();
        
        console.Out.WriteLine(
            success
                ? "All VBCSCompiler processes were terminated."
                : "Some VBCSCompiler processes could not be terminated." );
    }
}