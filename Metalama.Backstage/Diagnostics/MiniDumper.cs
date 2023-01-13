// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Backstage.Diagnostics;

internal class MiniDumper : IMiniDumper
{
    public static bool IsSupported => RuntimeInformation.IsOSPlatform( OSPlatform.Windows );

    private static int _isAppDomainInitialized;
    private static volatile WeakReference<MiniDumper>? _latestDumper;

    private readonly ILogger _logger;
    private readonly MiniDumpConfiguration _configuration;
    private readonly bool _isProcessEnabled;
    private readonly MiniDumpKind _flags;
    private readonly ProcessKind _processKind;

    [StructLayout( LayoutKind.Sequential, Pack = 4 )] // Pack=4 is important! So it works also for x64!
    private struct MiniDumpExceptionInformation
    {
        public uint ThreadId;
        public IntPtr ExceptionPointers;

        [MarshalAs( UnmanagedType.Bool )]
        public bool ClientPointers;
    }

    public MiniDumper( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Dumper" );
        this._configuration = serviceProvider.GetRequiredBackstageService<IConfigurationManager>().Get<DiagnosticsConfiguration>().MiniDump;

        var applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
        this._processKind = applicationInfo.ProcessKind;
        this._isProcessEnabled = this._configuration.Processes.TryGetValue( applicationInfo.ProcessKind.ToString(), out var isEnabled ) && isEnabled;

        foreach ( var flag in this._configuration.Flags )
        {
            if ( Enum.TryParse<MiniDumpKind>( flag, out var parsedFlag ) )
            {
                this._flags |= parsedFlag;
            }
            else
            {
                var possibleValues = string.Join( ",", Enum.GetNames( typeof(MiniDumpKind) ) );
                this._logger.Warning?.Log( $"Cannot parse the dump flag '{flag}'. The flag was ignored. Possible values are: {possibleValues}." );
            }
        }

        // The MiniDumper class is instantiated for each project, but the handler must be global.
        // We will use the latest available configuration in the process.
        if ( this._isProcessEnabled )
        {
            if ( Interlocked.Exchange( ref _isAppDomainInitialized, 1 ) == 0 )
            {
                AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
            }
        }

        _latestDumper = new WeakReference<MiniDumper>( this );
    }

    private static void OnFirstChanceException( object? sender, FirstChanceExceptionEventArgs e )
    {
        var dumperRef = _latestDumper;

        if ( dumperRef == null || !dumperRef.TryGetTarget( out var dumper ) )
        {
            return;
        }

        if ( dumper.MustWrite( e.Exception ) )
        {
            dumper.Write();
        }
    }

    [DllImport(
        "dbghelp.dll",
        EntryPoint = "MiniDumpWriteDump",
        CallingConvention = CallingConvention.StdCall,
        CharSet = CharSet.Unicode,
        ExactSpelling = true,
        SetLastError = true )]
    private static extern bool MiniDumpWriteDump(
        IntPtr hProcess,
        uint processId,
        IntPtr hFile,
        uint dumpType,
        ref MiniDumpExceptionInformation expParam,
        IntPtr userStreamParam,
        IntPtr callbackParam );

    [DllImport( "kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true )]
    private static extern uint GetCurrentThreadId();

    [DllImport( "kernel32.dll", EntryPoint = "GetCurrentProcess", ExactSpelling = true )]
    private static extern IntPtr GetCurrentProcess();

    [DllImport( "kernel32.dll", EntryPoint = "GetCurrentProcessId", ExactSpelling = true )]
    private static extern uint GetCurrentProcessId();

    public bool MustWrite( Exception exception )
        => IsSupported && this._isProcessEnabled
                       && exception is not TaskCanceledException and not OperationCanceledException
                       && (this._configuration.ExceptionTypes.Contains( exception.GetType().Name ) || this._configuration.ExceptionTypes.Contains( "*" ));

    public string? Write()
    {
        try
        {
            var directory = Path.Combine( Path.GetTempPath(), "Metalama", "CrashReports" );

            if ( !Directory.Exists( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            var fileName = Path.Combine( directory, $"{this._processKind}-{Guid.NewGuid()}.dmp" );

            this._logger.Info?.Log( $"Saving a dump to '{fileName}.'" );

            using ( var file = new FileStream( fileName, FileMode.Create, FileAccess.Write, FileShare.None ) )
            {
                MiniDumpExceptionInformation exp = default;
                exp.ThreadId = GetCurrentThreadId();
                exp.ClientPointers = false;

                // Marshal.GetExceptionPointer is not defined in .NET Standard but is present in both .NET Framework and .NET Core.
                var getExceptionPointers = typeof(Marshal).GetMethod( "GetExceptionPointers" );

                if ( getExceptionPointers != null )
                {
                    exp.ExceptionPointers = (IntPtr) getExceptionPointers.Invoke( null, Array.Empty<object>() )!;
                }

                // ReSharper disable once PossibleNullReferenceException
                var hFile = file.SafeFileHandle.DangerousGetHandle();

                var bRet = MiniDumpWriteDump(
                    GetCurrentProcess(),
                    GetCurrentProcessId(),
                    hFile,
                    (uint) this._flags,
                    ref exp,
                    IntPtr.Zero,
                    IntPtr.Zero );

                if ( !bRet )
                {
                    this._logger.Error?.Log( $"MiniDumpWriteDump has failed with error code 0x{Marshal.GetLastWin32Error():x8}." );

                    return null;
                }
            }

            var compressedFileName = fileName + ".gz";

            this._logger.Info?.Log( $"Compressing dump to '{compressedFileName}.'" );

            using ( var readStream = File.OpenRead( fileName ) )
            using ( var writeStream = new GZipStream( File.OpenWrite( compressedFileName ), CompressionMode.Compress ) )
            {
                readStream.CopyTo( writeStream );
            }

            File.Delete( fileName );

            return compressedFileName;
        }
        catch ( Exception e )
        {
            this._logger.Error?.Log( e.ToString() );

            return null;
        }
    }
}