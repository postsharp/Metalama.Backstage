// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Metalama.Backstage;

internal abstract partial class ProcessManagerBase : IProcessManager
{
    private static readonly ImmutableArray<KillableModuleSpec> _processesToKill = ImmutableArray.Create(
        new KillableModuleSpec( "VBCSCompiler", KillableModuleKind.Both ),
        new KillableModuleSpec( "MSBuild", KillableModuleKind.Both ) );

    private static readonly ImmutableArray<KillableModuleSpec> _processesToWarn = ImmutableArray.Create(
        new KillableModuleSpec( "servicehub.roslyncodeanalysisservice", KillableModuleKind.StandaloneProcess, "Visual Studio" ),
        new KillableModuleSpec( "jetbrains.resharper.roslyn.worker", KillableModuleKind.DotNet, "Rider/Resharper" ),
        new KillableModuleSpec( "jetbrains.roslyn.worker", KillableModuleKind.DotNet, "Rider/Resharper" ),
        new KillableModuleSpec( "omnisharp", KillableModuleKind.DotNet, "Visual Studio Code / Omnisharp" ) );

    protected ILogger Logger { get; }

    protected ProcessManagerBase( IServiceProvider serviceProvider )
    {
        this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "ProcessManager" );
    }

    protected static Process[] GetDotnetProcesses() => Process.GetProcessesByName( "dotnet" );

#pragma warning disable CA1307
    protected IEnumerable<KillableProcess> GetDotNetCompilerProcesses( ImmutableArray<KillableModuleSpec> processNames )
        => GetDotnetProcesses()
            .Select(
                p => (Process: p, Module: p.Modules.OfType<ProcessModule>()
                          .FirstOrDefault( m => processNames.Any( n => n.IsDotNet && m.ModuleName!.Contains( n.Name ) ) )) )
            .Where( p => p.Module != null )
            .Select( p => new KillableProcess( p.Process, this.Logger, p.Module!.FileName ) );
#pragma warning restore CA1307

    protected abstract IEnumerable<KillableProcess> GetProcesses( ImmutableArray<KillableModuleSpec> processNames );

    public virtual void KillCompilerProcesses( bool shouldEmitWarnings )
    {
        foreach ( var process in this.GetProcesses( _processesToKill ) )
        {
            process.ShutdownOrKill();
        }

        if ( shouldEmitWarnings )
        {
            // Report a warning when processes may be locking but cannot be closed.
            foreach ( var moduleSpec in _processesToWarn )
            {
                foreach ( var process in this.GetProcesses( ImmutableArray.Create( moduleSpec ) ) )
                {
                    this.Logger.Warning?.Log( $"The process {process.Process.Id} ({moduleSpec.DisplayName ?? moduleSpec.Name}) must be closed manually." );
                }
            }
        }
    }
}