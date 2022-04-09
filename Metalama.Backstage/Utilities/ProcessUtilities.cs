// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
    public static ProcessKind ProcessKind
    {
        get
        {
            switch ( Process.GetCurrentProcess().ProcessName.ToLowerInvariant() )
            {
                case "devenv":
                    return ProcessKind.DevEnv;

                case "servicehub.roslyncodeanalysisservice":
                    return ProcessKind.RoslynCodeAnalysisService;

                case "csc":
                case "vbcscompiler":
                    return ProcessKind.Compiler;

                case "dotnet":
                    var commandLine = Environment.CommandLine.ToLowerInvariant();

#pragma warning disable CA1307
                    if ( commandLine.Contains( "jetbrains.resharper.roslyn.worker.exe" ) )
                    {
                        return ProcessKind.Rider;
                    }
                    else
                    {
                        return commandLine.Contains( "vbcscompiler.dll" ) || commandLine.Contains( "csc.dll" )
                            ? ProcessKind.Compiler
                            : ProcessKind.Other;
                    }
#pragma warning restore CA1307

                default:
                    return ProcessKind.Other;
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

            if ( !RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                // TODO: actual implementation.
                logger.Trace?.Log( "Attended mode detected because of the platform is not Windows and Environment.UserInteractive is true." );

                return false;
            }

            /*
            if ( SystemServiceLocator.GetService<IContainerDetectionService>( false )?.IsRunningInContainer() ?? false )
            {
                log = "Unattended mode detected because of containerized environment.";
    
                return true;
            }
            */

            if ( Environment.OSVersion.Version.Major >= 6 && Process.GetCurrentProcess().SessionId == 0 )
            {
                logger.Trace?.Log( "Unattended mode detected because SessionId = 0" );

                return true;
            }

            var unattendedProcesses = new HashSet<string>( new[] { "services", "java", "agent.worker", "runner.worker" } );

            var parentProcesses = GetParentProcesses();

            logger.Trace?.Log(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Parent processes: {0}. ",
                    string.Join( ", ", parentProcesses.Select( p => p.ProcessName ?? p.ProcessId.ToString( CultureInfo.InvariantCulture ) ).ToArray() ) ) );

            var unattendedProcessInfo = parentProcesses.FirstOrDefault( p => p.ProcessName != null && unattendedProcesses.Contains( p.ProcessName ) );

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

    /*
    public static bool TerminateProcess( int processId )
    {
        var hProcess = OpenProcess( PROCESS_TERMINATE, true, processId );

        if ( hProcess == IntPtr.Zero )
        {
            return false;
        }

        try
        {
            return TerminateProcess( hProcess, -1 );
        }
        finally
        {
            CloseHandle( hProcess );
        }
    }

    public static unsafe int[] GetAllProcessIds()
    {
        const int max = 4096;
        var buffer = new int[4096];

        int bytesReturned;

        fixed ( int* pArray = buffer )
        {
            if ( !EnumProcesses( pArray, max * sizeof(int), out bytesReturned ) )
            {
                throw new Win32Exception();
            }
        }

        var ids = new int[bytesReturned / sizeof(int)];
        Array.Copy( buffer, ids, ids.Length );

        return ids;
    }

    public static string? GetProcessName( int processId )
    {
        var hProcess = OpenProcess( PROCESS_QUERY_INFORMATION, true, processId );

        if ( hProcess == IntPtr.Zero )
        {
            return null;
        }

        var stringBuilder = new StringBuilder( 1024 );
        var success = GetProcessImageFileName( hProcess, stringBuilder, 1024 );
        CloseHandle( hProcess );

        return success != 0 ? stringBuilder.ToString() : null;
    }
    */

    public static IReadOnlyList<ProcessInfo> GetParentProcesses()
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

    public static bool IsNetCore()
    {
        var frameworkDescription = RuntimeInformation.FrameworkDescription.ToLowerInvariant();

#pragma warning disable CA1307
        return !frameworkDescription.Contains( "framework" )
            && !frameworkDescription.Contains( "native" );
#pragma warning restore CA1307
    }
}