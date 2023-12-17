// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;

namespace Metalama.Backstage.Tools;

internal class BackstageToolsExecutor : IBackstageToolsExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPlatformInfo _platformInfo;
    private readonly ILogger _logger;
    private readonly IProcessExecutor _processExecutor;
    private readonly IBackstageToolsLocator _locator;

    public BackstageToolsExecutor( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._platformInfo = serviceProvider.GetRequiredBackstageService<IPlatformInfo>();
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "BackstageToolExecutor" );
        this._processExecutor = serviceProvider.GetRequiredBackstageService<IProcessExecutor>();
        this._locator = serviceProvider.GetRequiredBackstageService<IBackstageToolsLocator>();
    }

    public Process Start( BackstageTool tool, string arguments )
    {
        if ( this._locator.ToolsMustBeExtracted )
        {
            // This method can be called from different processes, including tools themselves. These processes do not have 
            // the IBackstageToolsExtractor service, but of course they are guaranteed to run when tools have already been extracted.
            this._serviceProvider.GetBackstageService<IBackstageToolsExtractor>()?.ExtractAll();
        }

        var workerDirectory = this._locator.GetToolDirectory( tool );

        var dotnetPath = this._platformInfo.DotNetExePath;
        var programPath = Path.Combine( workerDirectory, $"{tool.Name}.dll" );

        if ( !File.Exists( programPath ) )
        {
            throw new FileNotFoundException( $"The file '{programPath}' does not exist.", programPath );
        }

        var allArguments = $"\"{programPath}\" " + arguments;

        var processStartInfo = new ProcessStartInfo()
        {
            FileName = dotnetPath, Arguments = allArguments, UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden
        };

        this._logger.Info?.Log( $"Starting '{dotnetPath}{(arguments == "" ? "" : " ")}{allArguments}'." );

        var process = this._processExecutor.Start( processStartInfo );

        if ( process == null )
        {
            throw new Exception( $"Cannot start {tool.Name}." );
        }

        return process;
    }
}