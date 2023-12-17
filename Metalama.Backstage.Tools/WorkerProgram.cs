// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Program;
using System;
using System.Diagnostics;
using System.IO;

namespace Metalama.Backstage.Tools;

internal class WorkerProgram : EmbeddedProgram, IWorkerProgram
{
    private readonly IPlatformInfo _platformInfo;
    private readonly ILogger _logger;
    private readonly IProcessExecutor _processExecutor;

    public WorkerProgram( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._platformInfo = serviceProvider.GetRequiredBackstageService<IPlatformInfo>();
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "BackstagePrograms" );
        this._processExecutor = serviceProvider.GetRequiredBackstageService<IProcessExecutor>();
    }

    public void Start( string arguments )
    {
        var workerDirectory = this.ExtractProgram( "net6.0", "Metalama.Backstage.Worker" );

        var executableFileName = this._platformInfo.DotNetExePath;
        var allArguments = $"\"{Path.Combine( workerDirectory, "Metalama.Backstage.Worker.dll" )}\" " + arguments;

        var processStartInfo = new ProcessStartInfo()
        {
            FileName = executableFileName, Arguments = allArguments, UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden
        };

        this._logger.Info?.Log( $"Starting '{executableFileName}{(arguments == "" ? "" : " ")}{allArguments}'." );

        _ = this._processExecutor.Start( processStartInfo );
    }
}