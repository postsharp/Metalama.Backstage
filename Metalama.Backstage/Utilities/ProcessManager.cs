// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Metalama.Backstage.Utilities;

public class ProcessManager : IProcessManager
{
    private readonly ILogger _logger;

    public ProcessManager( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "ProcessManager" );
    }
    
    public bool KillVBCSCompilerProcess()
    {
        if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
        {
            if ( !this.TerminateVbcsCompilerProcessesOnWindows() )
            {
                return false;
            }
        }
        else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
        {
            if ( !this.TerminateVBCSCompilerProcessOnLinux() )
            {
                return false;
            }
        }
        else if ( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
        {
            if ( !this.TerminateVBCSCompilerProcessOnMac() )
            {
                return false;
            }
        }
        else
        {
            throw new NotSupportedException( "Killing VBCSCompiler process is not supported on the current platform." );
        }
        
        return true;
    }

    private bool TerminateVbcsCompilerProcessesOnWindows()
    {
        var processes = Process.GetProcessesByName( "VBCSCompiler" );

        var success = true;

        foreach ( var process in processes )
        {
            // Start process that should shutdown VBCSCompiler.
            try
            {
                if ( process.MainModule == null )
                {
                    continue;
                }

                var shutdownProcess = new Process() { StartInfo = new ProcessStartInfo() { FileName = process.MainModule.FileName, Arguments = "-shutdown" } };

                shutdownProcess.Start();
                shutdownProcess.WaitForExit();
            }
            catch ( Exception e )
            {
                this._logger.Error?.Log(
                    $"Could not shutdown process {process.ProcessName} ({process.Id}): {e.Message}." );
            }

            // Try killing the process directly, if shutting down VBCSCompiler.exe didn't work.
            if ( !process.HasExited )
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch ( InvalidOperationException e )
                {
                    this._logger.Warning?.Log( $"Could not kill process '{process.ProcessName}' (PID: {process.Id}), because it has already exited: {e.Message}." );
                }
                catch ( Exception e )
                {
                    this._logger.Error?.Log( $"Could not kill process '{process.ProcessName}' (PID: {process.Id}): {e.Message}." );
                }
            }

            if ( !process.HasExited )
            {
                this._logger.Error?.Log( $"Process '{process.ProcessName}' (PID: {process.Id}) could not be terminated." );

                success = false;
            }
        }

        return success;
    }

    private bool TerminateVBCSCompilerProcessOnLinux()
    {
        var processesToKill = Process.GetProcessesByName( "dotnet" );

        var success = true;

        // Find dotnet processes using VBCSCompiler module.
        foreach ( var process in processesToKill )
        {
            foreach ( ProcessModule module in process.Modules )
            {
#pragma warning disable CA1307
                if ( module.ModuleName != null && module.ModuleName.Contains( "VBCSCompiler" ) )
#pragma warning restore CA1307
                {
                    this._logger.Trace?.Log( $"Found module {module.ModuleName} running in {process.ProcessName} (PID: {process.Id}). Attempting to shut it down." );

                    // Start process that should shutdown VBCSCompiler.
                    var shutdownModuleProcess = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = "dotnet",
                            Arguments = $"{module.FileName} -shutdown",
                            RedirectStandardOutput = true
                        }
                    };

                    shutdownModuleProcess.Start();
                    shutdownModuleProcess.WaitForExit();

                    // Try to kill the parent process (dotnet) of VBCSCompiler.dll, if shutdown didn't work.
                    if ( !process.HasExited )
                    {
                        this._logger.Warning?.Log( $"Couldn't shut down module '{module.ModuleName}' running in '{process.ProcessName}' (PID: {process.Id}). Attempting to kill the process." );

                        try
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                        catch ( InvalidOperationException e )
                        {
                            this._logger.Warning?.Log( $"Could not kill process '{process.ProcessName}' (PID: {process.Id}), because it has already exited: {e.Message}" );
                        }
                        catch ( Exception e )
                        {
                            this._logger.Error?.Log( $"{e.Message}" );
                        }
                    }

                    if ( !process.HasExited )
                    {
                        this._logger.Error?.Log( $"Parent process '{process.ProcessName}' (PID: {process.Id}) of module '{module.FileName}' could not be terminated." );

                        success = false;
                    }

                    break;
                } 
            }
        }

        return success;
    }

    private bool TerminateVBCSCompilerProcessOnMac()
    {
        var processesToKill = Process.GetProcessesByName( "dotnet" );

        var success = true;

        // For each dotnet process get list of open files the process uses, then kill the process.
        foreach ( var process in processesToKill )
        {
            var listOpenFilesProcess = new Process() { StartInfo = new ProcessStartInfo() { FileName = "lsof", Arguments = $"-p {process.Id}", RedirectStandardOutput = true } };
            listOpenFilesProcess.Start();

            var processUsesVbcsCompiler = false;

            while ( !listOpenFilesProcess.StandardOutput.EndOfStream )
            {
                var outputLine = listOpenFilesProcess.StandardOutput;

                if ( outputLine.ReadLine() != null )
                {
#pragma warning disable CA1307
                    processUsesVbcsCompiler = outputLine.ReadLine()!.Contains( "VBCSCompiler.dll" );
#pragma warning restore CA1307
                }

                if ( processUsesVbcsCompiler )
                {
                    break;
                }
            }

            listOpenFilesProcess.WaitForExit();

            if ( listOpenFilesProcess.ExitCode != 0 )
            {
                this._logger.Error?.Log( $"Could not list open files for process '{process.ProcessName}' (PID:{process.Id})." );

                success = false;
            }

            if ( processUsesVbcsCompiler )
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch ( InvalidOperationException e )
                {
                    this._logger.Warning?.Log( $"Could not kill process '{process.ProcessName}' (PID: {process.Id}), because it has already exited: {e.Message}" );
                }
                catch ( Exception e )
                {
                    this._logger.Error?.Log( $"{e.Message}" );
                }
            }

            if ( !process.HasExited )
            {
                this._logger.Error?.Log( $"Process '{process.ProcessName}' (PID: {process.Id}) could not be terminated." );

                success = false;
            }
        }

        return success;
    }
}