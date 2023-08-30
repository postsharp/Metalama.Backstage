// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Metalama.Backstage.Utilities;

// ReSharper disable UnusedMember.Local
// ReSharper disable StringLiteralTypo
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable InconsistentNaming
#pragma warning disable SA1310, IDE1006 // Naming conventions.

public static class ProcessUtilities
{
    [PublicAPI]
    public static ProcessKind ProcessKind
    {
        get
        {
            // Note that the same logic is duplicated in Metalama.Framework.CompilerExtensions.ProcessKindHelper and cannot 
            // be shared. Any change here must be done there too.

            var processName = Process.GetCurrentProcess().ProcessName.ToLowerInvariant();

            switch ( processName )
            {
                case "devenv":
                    return ProcessKind.DevEnv;

                case "servicehub.roslyncodeanalysisservice":
                    return ProcessKind.RoslynCodeAnalysisService;

                case "servicehub.host":
                    {
                        var commandLine = Environment.CommandLine.ToLowerInvariant();

#pragma warning disable CA1307
                        if ( commandLine.Contains( "$codelensservice$" ) )
                        {
                            return ProcessKind.CodeLensService;
                        }
                        else
                        {
                            return ProcessKind.Other;
                        }
#pragma warning restore CA1307
                    }

                case "visualstudio":
                    return ProcessKind.VisualStudioMac;

                case "csc":
                case "vbcscompiler":
                    return ProcessKind.Compiler;

                case "resharpertestrunner":
                case "resharpertestrunner64":
                    return ProcessKind.ResharperTestRunner;

                case "microsoft.codeanalysis.languageserver":
                    return ProcessKind.LanguageServer;

                case "dotnet":
                    {
                        var commandLine = Environment.CommandLine.ToLowerInvariant();

#pragma warning disable CA1307
                        if ( commandLine.Contains( "jetbrains.resharper.roslyn.worker" ) ||
                             commandLine.Contains( "jetbrains.roslyn.worker" ) )
                        {
                            return ProcessKind.Rider;
                        }
                        else if ( commandLine.Contains( "vbcscompiler.dll" ) || commandLine.Contains( "csc.dll" ) )
                        {
                            return ProcessKind.Compiler;
                        }
                        else if ( commandLine.Contains( "omnisharp.dll" ) )
                        {
                            return ProcessKind.OmniSharp;
                        }
                        else if ( commandLine.Contains( "resharpertestrunner.dll" ) )
                        {
                            return ProcessKind.ResharperTestRunner;
                        }
                        else
                        {
                            return ProcessKind.Other;
                        }
#pragma warning restore CA1307
                    }

                default:
                    if ( processName.StartsWith( "linqpad", StringComparison.Ordinal ) )
                    {
                        return ProcessKind.LinqPad;
                    }
                    else
                    {
                        return ProcessKind.Other;
                    }
            }
        }
    }

    [StructLayout( LayoutKind.Sequential )]
    private struct PROCESS_BASIC_INFORMATION
    {
        public IntPtr Reserved1;
        public IntPtr PebBaseAddress;
        public IntPtr Reserved2_0;
        public IntPtr Reserved2_1;
        public IntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
    }

    [DllImport( "kernel32" )]
    private static extern bool TerminateProcess( IntPtr hProcess, int uExitCode );

    [DllImport( "kernel32" )]
    private static extern IntPtr GetCurrentProcess();

    [DllImport( "ntdll" )]
    private static extern int NtQueryInformationProcess(
        IntPtr processHandle,
        int processInformationClass,
        ref PROCESS_BASIC_INFORMATION processInformation,
        int processInformationLength,
        out int returnLength );

    [DllImport( "kernel32" )]
    private static extern IntPtr OpenProcess( uint dwDesiredAccess, bool bInheritHandle, int dwProcessId );

    [DllImport( "kernel32" )]
    private static extern bool CloseHandle( IntPtr hObject );

    [DllImport( "psapi", CharSet = CharSet.Unicode )]
    private static extern int GetProcessImageFileName(
        IntPtr hProcess,
        [Out] [MarshalAs( UnmanagedType.LPWStr )]
        StringBuilder lpImageFileName,
        int nSize );

    [DllImport( "psapi" )]
    private static extern unsafe bool EnumProcesses( int* pProcessIds, int cb, out int pBytesReturned );

    private const uint PROCESS_QUERY_INFORMATION = 0x0400;

    // private const uint PROCESS_TERMINATE = 0x0001;

    // ReSharper restore MemberCanBePrivate.Local
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    // ReSharper restore InconsistentNaming

    private static int _isCurrentProcessUnattended;

    [PublicAPI]
    public static bool IsCurrentProcessUnattended( ILoggerFactory loggerFactory )
    {
        var logger = loggerFactory.GetLogger( "ProcessUtilities" );

        if ( _isCurrentProcessUnattended == 0 )
        {
            _isCurrentProcessUnattended = Detect() ? 1 : 2;
        }

        return _isCurrentProcessUnattended == 1;

        bool Detect()
        {
            if ( !Environment.UserInteractive )
            {
                logger.Trace?.Log( "Unattended mode detected because Environment.UserInteractive = false." );

                return true;
            }

            IReadOnlyList<ProcessInfo>? parentProcesses;

            // Mac/OSX is always considered user interactive.
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
            {
                logger.Trace?.Log( "Unattended mode NOT detected: Mac/OSX platform." );

                return false;
            }
            else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
            {
                // Check if the we are running in Linux based Docker container.
                if ( IsRunningInDockerContainer( logger ) )
                {
                    logger.Trace?.Log( "Unattended mode detected because of Docker containerized environment." );

                    return true;
                }

                if ( !TryGetParentProcessesOnLinux( logger, out parentProcesses ) )
                {
                    logger.Warning?.Log( "Unattended mode detected because the detection was not successful." );

                    return true;
                }
            }
            else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                if ( Environment.OSVersion.Version.Major >= 6 && Process.GetCurrentProcess().SessionId == 0 )
                {
                    logger.Trace?.Log( "Unattended mode detected because SessionId = 0 on Windows." );

                    return true;
                }

                parentProcesses = GetParentProcessesOnWindows();
            }
            else
            {
                logger.Warning?.Log( "Unattended mode detected because the platform is unknonw." );

                return true;
            }

            // Check the parent processes.
            var unattendedProcesses = new HashSet<string>(
                new[]
                {
                    "services",
                    "java",               // TeamCity, Atlassian Bamboo (can also be "bamboo"), Jenkins, GoCD
                    "bamboo",             // Atlassian Bamboo
                    "agent.worker",       // Azure Pipelines
                    "runner.worker",      // GitHub Actions
                    "buildkite-agent",    // BuildKite
                    "circleci-agent",     // CircleCI (Docker, but has specific process name)
                    "agent",              // Semaphore CI (Linux)
                    "sshd: travis [priv]" // Travis CI (Linux)
                } );

            logger.Trace?.Log(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Parent processes: {0}. ",
                    string.Join( ", ", parentProcesses.Select( p => p.ProcessName ?? p.ProcessId.ToString( CultureInfo.InvariantCulture ) ).ToArray() ) ) );

            var unattendedProcessInfo = parentProcesses.FirstOrDefault( p => p.ImagePath != null && unattendedProcesses.Contains( p.ImagePath ) );

            if ( unattendedProcessInfo != null )
            {
                logger.Trace?.Log(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        " Unattended mode detected because of parent process '{0}'.",
                        unattendedProcessInfo.ProcessName ) );

                return true;
            }

            logger.Trace?.Log( "Unattended mode NOT detected." );

            return false;
        }
    }

    private static IReadOnlyList<ProcessInfo> GetParentProcessesOnWindows()
    {
        var processes = new List<ProcessInfo>();
        var currentProcess = GetCurrentProcess();
        var parents = new HashSet<int>();

        var hProcess = currentProcess;

        for ( /* Intentionally empty */; hProcess != IntPtr.Zero; )
        {
            string? processName = null;
            var stringBuilder = new StringBuilder( 1024 );

            if ( GetProcessImageFileName( hProcess, stringBuilder, 1024 ) != 0 )
            {
                processName = stringBuilder.ToString();
            }

            var pbi = default(PROCESS_BASIC_INFORMATION);
            var status = NtQueryInformationProcess( hProcess, 0, ref pbi, Marshal.SizeOf( pbi ), out _ );

            if ( status != 0 )
            {
                throw new Win32Exception( status );
            }

            int parentProcessId;
            int processId;

            try
            {
                parentProcessId = pbi.InheritedFromUniqueProcessId.ToInt32();
            }
            catch ( ArgumentException )
            {
                // not found
                parentProcessId = 0;
            }

            try
            {
                processId = pbi.UniqueProcessId.ToInt32();
            }
            catch ( ArgumentException )
            {
                // not found
                processId = 0;
            }

            if ( !parents.Add( processId ) )
            {
                // There is a loop.
                break;
            }

            if ( processes.Count > 64 )
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Cannot have more than 64 parents. Parent processes: {0}.",
                        string.Join( ", ", processes.Select( pi => pi.ProcessId.ToString( CultureInfo.InvariantCulture ) ).ToArray() ) ) );
            }

            processes.Add( new ProcessInfo( processId, processName ) );

            var hParentProcess = parentProcessId != 0 ? OpenProcess( PROCESS_QUERY_INFORMATION, true, parentProcessId ) : IntPtr.Zero;

            CloseHandle( hProcess );

            hProcess = hParentProcess;
        }

        CloseHandle( hProcess );

        return processes.ToArray();
    }

    [PublicAPI]
    public static IReadOnlyList<ProcessInfo> GetParentProcesses( ILogger? logger = null )
    {
        if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
        {
            if ( !TryGetParentProcessesOnLinux( logger, out var list ) )
            {
                throw new IOException( "Cannot get the process list." );
            }

            return list;
        }
        else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
        {
            return GetParentProcessesOnWindows();
        }
        else
        {
            throw new NotSupportedException( "Getting parent processes in not supported on the current platform." );
        }
    }

    private static bool TryGetParentProcessesOnLinux( ILogger? logger, [NotNullWhen( true )] out IReadOnlyList<ProcessInfo>? list )
    {
        var processes = new List<ProcessInfo>();
        var parents = new HashSet<int>();

        // Get current process ID and command name, then add it to list of processes.
        var currentProcessId = Process.GetCurrentProcess().Id;
        var parentProcessId = currentProcessId;

        while ( parentProcessId != 0 )
        {
            // Read command name of the process.
            string? processName;

            try
            {
                processName = File.ReadAllText( "/proc/" + parentProcessId + "/comm" ).Trim();
            }
            catch ( Exception e )
            {
                logger?.Error?.Log( $"Could not read '/proc/{parentProcessId}/comm' file: {e.Message}" );
                processName = null;
            }

            // Read status file of the process.
            string? processStatus;

            try
            {
                processStatus = File.ReadAllText( "/proc/" + parentProcessId + "/stat" );
            }
            catch ( Exception e )
            {
                logger?.Error?.Log( $"Could not read '/proc/{parentProcessId}/stat' file: {e.Message}" );

                list = null;

                return false;
            }

            var processStatusArray = processStatus.Split( ' ' );

            // Try parse PPID from 4th value of status information, then add the process to list of processes.
            try
            {
                parentProcessId = int.Parse( processStatusArray[3], CultureInfo.InvariantCulture );
            }
            catch ( Exception e )
            {
                logger?.Error?.Log( $"Could not parse PPID from process '{parentProcessId}' status file: {e.Message}" );

                list = null;

                return false;
            }

            if ( !parents.Add( parentProcessId ) )
            {
                // There is a loop.
                break;
            }

            processes.Add( new ProcessInfo( parentProcessId, processName ) );
        }

        list = processes.ToArray();

        return true;
    }

    private static bool IsRunningInDockerContainer( ILogger logger )
    {
        // If the process is running inside a Docker container,
        // init (pid '1') process control group collection will have /docker/ as a part of the groups hierarchies.
        string? processesControlGroup = null;
        var controlGroupFile = "/proc/1/cgroup";

        try
        {
            processesControlGroup = File.ReadAllText( controlGroupFile );
        }
        catch ( Exception e )
        {
            logger.Error?.Log( $"Could not read '{controlGroupFile}' file: {e.Message}" );
        }

        var isRunningInsideDockerContainer = false;

        if ( !string.IsNullOrEmpty( processesControlGroup ) )
        {
#pragma warning disable CS8602, CA1307
            isRunningInsideDockerContainer = processesControlGroup.Contains( "docker" );
#pragma warning restore CS8602, CA1307
        }

        return isRunningInsideDockerContainer;
    }

    internal static bool IsNetCore()
    {
        var frameworkDescription = RuntimeInformation.FrameworkDescription.ToLowerInvariant();

#pragma warning disable CA1307
        return !frameworkDescription.Contains( "framework" )
               && !frameworkDescription.Contains( "native" );
#pragma warning restore CA1307
    }
}