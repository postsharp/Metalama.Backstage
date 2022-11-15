// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Metalama.Backstage.Utilities;

internal class WindowsProcessManager : ProcessManagerBase
{
    private readonly ILogger _logger;

    public WindowsProcessManager( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "WindowsProcessManager" );
    }

    protected sealed override bool KillVbcsCompilerProcess()
    {
        var compilerProcesses = this.GetVbcsCompilerProcesses().Where( p => p.MainModule != null );

        var success = true;

        foreach ( var process in compilerProcesses )
        {
            // Start process that should shutdown VBCSCompiler.
            try
            {
                var shutdownProcess = new Process() { StartInfo = new ProcessStartInfo() { FileName = process.MainModule!.FileName, Arguments = "-shutdown" } };

                this._logger.Trace?.Log( $"Shutting down '{process.ProcessName}' (PID: {process.Id})." );

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
                    this._logger.Trace?.Log( $"Killing '{process.ProcessName}' (PID: {process.Id})." );

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

        // Kill all dotnet processes accessing VBSCompiler.
        var dotnetProcesses = this.GetDotnetProcesses()
            .Where(
                p =>
                {
                    var commandLine = GetCommandLine( p );
                    
                    if ( commandLine != null 
#pragma warning disable CA1307
                         && commandLine.Contains( "VBCSCompiler" ) )
#pragma warning restore CA1307
                    {
                        return true;
                    }

                    return false;
                } )
            .ToList();

        foreach ( var process in dotnetProcesses )
        {
            try
            {
                this._logger.Trace?.Log( $"Killing '{process.ProcessName}' (PID: {process.Id})." );

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

            if ( !process.HasExited )
            {
                this._logger.Error?.Log( $"Process '{process.ProcessName}' (PID: {process.Id}) could not be terminated." );

                success = false;
            }
        }

        return success;
    }

    private static string? GetCommandLine( Process process )
    {
#pragma warning disable CA1416
        try
        {
            using ManagementObjectSearcher searcher =
                new( "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id );

            using ( var objects = searcher.Get() )
            {
                return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
            }
        }
        catch
        {
            return null;
        }
#pragma warning restore CA1416
    }
}