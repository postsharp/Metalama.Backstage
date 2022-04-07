// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Metalama.Backstage.Telemetry;

internal class ExceptionReporter : IExceptionReporter
{
    private const string _errorContextDataSlot = "__PSErrorContext";

    private readonly TelemetryQueue _uploadManager;
    private readonly TelemetryConfiguration _configuration;
    private readonly IDateTimeProvider _time;
    private readonly IApplicationInfo _applicationInfo;
    private readonly IStandardDirectories _directories;
    private readonly ILogger _logger;

    private readonly Regex _stackFrameRegex = new( @"\S+\([^\)]*\)" );

    public ExceptionReporter( TelemetryQueue uploadManager, IServiceProvider serviceProvider )
    {
        this._uploadManager = uploadManager;
        this._configuration = serviceProvider.GetRequiredService<IConfigurationManager>().Get<TelemetryConfiguration>();
        this._time = serviceProvider.GetRequiredService<IDateTimeProvider>();
        this._applicationInfo = serviceProvider.GetRequiredService<IApplicationInfo>();
        this._directories = serviceProvider.GetRequiredService<IStandardDirectories>();
        this._logger = serviceProvider.GetLoggerFactory().Telemetry();
    }

    public IEnumerable<string> CleanStackTrace( string stackTrace )
    {
        foreach ( Match match in this._stackFrameRegex.Matches( stackTrace ) )
        {
            yield return match.Value;
        }
    }

    public static void SetContext( Exception exception, object contextInfo ) => exception.Data[_errorContextDataSlot] = contextInfo;

    public bool ShouldReportException( Exception exception )
    {
        switch ( exception )
        {
            case IOException _:
            case SecurityException _:
            case UnauthorizedAccessException _:
            case WebException _:
            case OperationCanceledException _:
                return false;
        }

        if ( exception.InnerException != null && !this.ShouldReportException( exception.InnerException ) )
        {
            return false;
        }

        if ( exception is AggregateException aggregateException && aggregateException.InnerExceptions.Any( e => !this.ShouldReportException( e ) ) )
        {
            return false;
        }

        return true;
    }

    private string ComputeExceptionHash( string? version, string exceptionTypeName, string stackTrace )
    {
        var signature = new StringBuilder( 1024 );
        signature.Append( version ?? "?" );
        signature.Append( ':' );
        signature.Append( exceptionTypeName );
        signature.Append( ':' );

        var firstFrame = true;
        var lastFrameIsUser = false;

        foreach ( var stackFrame in this.CleanStackTrace( stackTrace ) )
        {
            var writeStackFrame = stackFrame;

#pragma warning disable CA1307
            if ( stackFrame.Contains( "#user" ) )
#pragma warning restore CA1307
            {
                if ( lastFrameIsUser )
                {
                    continue;
                }
                else
                {
                    writeStackFrame = "#user";
                    lastFrameIsUser = true;
                }
            }
            else
            {
                lastFrameIsUser = true;
            }

            if ( firstFrame )
            {
                firstFrame = false;
            }
            else
            {
                signature.Append( ',' );
            }

            signature.Append( writeStackFrame );
        }

        byte[] hashBytes;

        using ( var md5 = new MD5Managed() )
        {
            hashBytes = md5.ComputeHash( Encoding.UTF8.GetBytes( signature.ToString().Normalize() ) );
        }

        var hash = new StringBuilder( hashBytes.Length * 2 );

        foreach ( var t in hashBytes )
        {
            hash.AppendFormat( CultureInfo.InvariantCulture, "{0:x2}", t );
        }

        return hash.ToString();
    }

    private bool ShouldReportIssue( string hash )
    {
        if ( this._configuration.Issues.TryGetValue( hash, out var currentStatus ) )
        {
            if ( currentStatus is ReportingStatus.Ignored or ReportingStatus.Reported )
            {
                this._logger.Info?.Log( $"The issue {hash} should not be reported because its status is {currentStatus}." );

                return false;
            }
        }

        return this._configuration.ConfigurationManager.Update<TelemetryConfiguration>(
            c =>
            {
                if ( c.Issues.TryGetValue( hash, out var raceStatus ) )
                {
                    if ( raceStatus is ReportingStatus.Ignored or ReportingStatus.Reported )
                    {
                        this._logger.Info?.Log( $"The issue {hash} should not be reported because another process is reporting it." );

                        return false;
                    }
                }

                c.Issues[hash] = currentStatus;

                return true;
            } );
    }

    public void ReportException( Exception e, ExceptionReportingKind exceptionReportingKind = ExceptionReportingKind.Exception )
    {
        this._logger.Trace?.Log( $"Reporting an exception of type {e.GetType().Name}." );

        var reportingAction = exceptionReportingKind == ExceptionReportingKind.Exception
            ? this._configuration.ExceptionReportingAction
            : this._configuration.PerformanceProblemReportingAction;

        if ( reportingAction != ReportingAction.Yes )
        {
            this._logger.Trace?.Log( $"The issue will not be reported because the reporting action in the user profile is set to {reportingAction}." );

            return;
        }

        // Compute a signature for this exception.
        var hash = this.ComputeExceptionHash(
            this._applicationInfo.Version,
            e.GetType().FullName!,
            ExceptionSensitiveDataHelper.Instance.RemoveSensitiveData( e.StackTrace ) );

        // Check if this exception has already been reported.

        if ( !this.ShouldReportIssue( hash ) )
        {
            return;
        }

        // Create the exception report file.
        var directory = this._directories.TelemetryExceptionsDirectory;

        if ( !Directory.Exists( directory ) )
        {
            Directory.CreateDirectory( directory );
        }

        var fileName = Path.Combine( directory, "exception-" + hash + "-" + Guid.NewGuid().ToString() + ".xml" );

        var writer = new XmlTextWriter( fileName, Encoding.UTF8 ) { Formatting = Formatting.Indented };
        writer.WriteStartDocument();
        writer.WriteStartElement( "ErrorReport" );
        writer.WriteElementString( "InvariantHash", hash );
        writer.WriteElementString( "Time", XmlConvert.ToString( this._time.Now, XmlDateTimeSerializationMode.RoundtripKind ) );
        writer.WriteElementString( "ClientId", this._configuration.DeviceId.ToString() );
        writer.WriteStartElement( "Application" );
        writer.WriteElementString( "Name", this._applicationInfo.Name );
        writer.WriteElementString( "Version", this._applicationInfo.Version );
        writer.WriteEndElement();

        var currentProcess = Process.GetCurrentProcess();
        writer.WriteStartElement( "Process" );
        writer.WriteElementString( "Name", currentProcess.ProcessName );
        writer.WriteElementString( "ProcessorArchitecture", XmlConvert.ToString( IntPtr.Size * 8 ) );
        writer.WriteElementString( "SessionId", XmlConvert.ToString( currentProcess.SessionId ) );
        writer.WriteElementString( "TotalProcessorTime", XmlConvert.ToString( currentProcess.TotalProcessorTime ) );
        writer.WriteElementString( "WorkingSet", XmlConvert.ToString( currentProcess.WorkingSet64 ) );
        writer.WriteElementString( "PeakWorkingSet", XmlConvert.ToString( currentProcess.PeakWorkingSet64 ) );
        writer.WriteElementString( "ManagedHeap", XmlConvert.ToString( GC.GetTotalMemory( false ) ) );
        writer.WriteEndElement();
        writer.WriteStartElement( "Environment" );
        writer.WriteElementString( "OSVersion", Environment.OSVersion.Version.ToString() );
        writer.WriteElementString( "ProcessorCount", XmlConvert.ToString( Environment.ProcessorCount ) );
        writer.WriteElementString( "Version", Environment.Version.ToString() );
        writer.WriteEndElement();

        writer.WriteStartElement( "Exception" );

        ExceptionXmlFormatter.WriteException( writer, e );

        writer.WriteEndElement();

        writer.WriteStartElement( "Assemblies" );

        foreach ( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
        {
            var assemblyName = assembly.GetName();
            writer.WriteStartElement( "Assembly" );
            writer.WriteElementString( "Name", ExceptionSensitiveDataHelper.Instance.RemoveSensitiveData( assemblyName.Name ) );
            writer.WriteElementString( "Version", assemblyName.Version?.ToString() ?? "<unknown>" );

            try
            {
                if ( !assembly.IsDynamic && !string.IsNullOrEmpty( assembly.Location ) )
                {
                    writer.WriteElementString( "FileVersion", FileVersionInfo.GetVersionInfo( assembly.Location ).FileVersion );
                }
            }
            catch ( NotSupportedException ) { }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.Close();

        if ( reportingAction == ReportingAction.Yes )
        {
            this._uploadManager.EnqueueFile( fileName );
        }
        else
        {
            // TODO: Open some UI?    
        }
    }
}