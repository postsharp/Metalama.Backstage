// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Diagnostics;

/// <summary>
/// This tests class works with predefined default configuration set in constructor.
/// </summary>
public class DiagnosticsConfigurationTests : TestsBase
{
    private readonly string _diagnosticsJsonFilePath;

    private readonly IConfigurationManager _configurationManager;

    public DiagnosticsConfigurationTests( ITestOutputHelper logger ) : base(
        logger,
        builder =>
        {
            builder.AddService( typeof(IConfigurationManager), new Configuration.ConfigurationManager( builder.ServiceProvider ) );
            builder.AddService( typeof(IApplicationInfoProvider), new ApplicationInfoProvider( new TestApplicationInfo() ) );
        } )
    {
        var standardDirectories = this.ServiceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this._configurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>();

        // Initialize local DiagnosticsConfiguration with logging active.
        var diagnosticsConfiguration = new DiagnosticsConfiguration();

        this.FileSystem.CreateDirectory( standardDirectories.ApplicationDataDirectory );
        this._diagnosticsJsonFilePath = Path.Combine( standardDirectories.ApplicationDataDirectory, "diagnostics.json" );
        this.FileSystem.WriteAllText( this._diagnosticsJsonFilePath, diagnosticsConfiguration.ToJson() );

        this._configurationManager.Update<DiagnosticsConfiguration>(
            c =>
            {
                foreach ( var process in c.Logging.Processes.Keys.ToArray() )
                {
                    c.Logging.Processes[process] = true;
                }

                return new DiagnosticsConfiguration( c.Logging, c.Debugger, c.MiniDump );
            } );
    }

    [Fact]
    public void LocalDiagnosticsJsonFile_Exists()
    {
        var diagnosticsConfiguration = this._configurationManager.Get<DiagnosticsConfiguration>();

        foreach ( var processName in diagnosticsConfiguration.Logging.Processes.Keys.ToArray() )
        {
            Assert.True( diagnosticsConfiguration.Logging.Processes[processName] );
        }
    }

    // TODO: better name.
    [Fact]
    public void ChangingLoggingHours_UpdatesConfiguration()
    {
        // Manually set the hours to more than default.
        var hours = 10;

        this._configurationManager.Update<DiagnosticsConfiguration>(
            c =>
            {
                c.Logging.StopLoggingAfterHours = hours;

                return c;
            } );

        var diagnosticsConfiguration = this._configurationManager.Get<DiagnosticsConfiguration>();

        Assert.Equal( hours, diagnosticsConfiguration.Logging.StopLoggingAfterHours );
    }

    [Fact]
    public void ResettingConfiguration_ChangesLoggingHours()
    {
        // Manually set the hours to more than default before resetting.
        var hours = 10;

        this._configurationManager.Update<DiagnosticsConfiguration>(
            c =>
            {
                c.Logging.StopLoggingAfterHours = hours;

                return c;
            } );

        this._configurationManager.Update<DiagnosticsConfiguration>( _ => new DiagnosticsConfiguration() );
        var diagnosticsConfiguration = (DiagnosticsConfiguration) this._configurationManager.Get( typeof(DiagnosticsConfiguration) );

        Assert.Equal( 2, diagnosticsConfiguration.Logging.StopLoggingAfterHours );
    }

    [Fact]
    public void OutdatedConfiguration_DisablesLogging()
    {
        var configurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>();

        // Manually simulate the last modification of configuration happened before 3 hours.
        this.FileSystem.SetLastWriteTime( this._diagnosticsJsonFilePath, DateTime.Now.AddHours( -3 ) );
        configurationManager.Update<DiagnosticsConfiguration>( c => c );

        var diagnosticsConfiguration = this._configurationManager.Get<DiagnosticsConfiguration>();
        
        if ( diagnosticsConfiguration.LastModified != null &&
             diagnosticsConfiguration.LastModified < this.Time.Now.AddHours( -diagnosticsConfiguration.Logging.StopLoggingAfterHours ) )
        {
            configurationManager.UpdateIf<DiagnosticsConfiguration>(
                c => c.Logging.Processes.Any( p => p.Value ),
                c =>
                {
                    foreach ( var process in c.Logging.Processes.Keys )
                    {
                        c.Logging.Processes[process] = false;
                    }

                    return c;
                } );
        }

        diagnosticsConfiguration = this._configurationManager.Get<DiagnosticsConfiguration>();

        foreach ( var processName in diagnosticsConfiguration.Logging.Processes.Keys.ToArray() )
        {
            Assert.False( diagnosticsConfiguration.Logging.Processes[processName] );
        }
    }

    [Fact]
    public void NotOutdatedConfiguration_KeepsLogging()
    {
        var configurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>();

        // Manually simulate the last modification of configuration happened before 1 hours.
        this.FileSystem.SetLastWriteTime( this._diagnosticsJsonFilePath, DateTime.Now.AddHours( -1 ) );
        configurationManager.Update<DiagnosticsConfiguration>( c => c );

        var diagnosticsConfiguration = this._configurationManager.Get<DiagnosticsConfiguration>();

        if ( diagnosticsConfiguration.LastModified != null &&
             diagnosticsConfiguration.LastModified < this.Time.Now.AddHours( -diagnosticsConfiguration.Logging.StopLoggingAfterHours ) )
        {
            configurationManager.UpdateIf<DiagnosticsConfiguration>(
                c => c.Logging.Processes.Any( p => p.Value ),
                c =>
                {
                    foreach ( var process in c.Logging.Processes.Keys )
                    {
                        c.Logging.Processes[process] = false;
                    }

                    return c;
                } );
        }

        diagnosticsConfiguration = this._configurationManager.Get<DiagnosticsConfiguration>();

        foreach ( var processName in diagnosticsConfiguration.Logging.Processes.Keys.ToArray() )
        {
            Assert.True( diagnosticsConfiguration.Logging.Processes[processName] );
        }
    }

    [Fact]
    public void InitializingConfiguration_ByPassingValues_IsCorrect()
    {
        var diagnosticsConfiguration = (DiagnosticsConfiguration) this._configurationManager.Get( typeof(DiagnosticsConfiguration), true );
        var diagnosticsConfigurationJsonContent = diagnosticsConfiguration.ToJson();

        var deserializedDiagnosticsConfiguration = JsonConvert.DeserializeObject<DiagnosticsConfiguration>( diagnosticsConfigurationJsonContent );

        if ( deserializedDiagnosticsConfiguration == null )
        {
            throw new InvalidOperationException( $"Test diagnostics configuration is invalid." );
        }

        // Logging
        foreach ( var categoryKey in diagnosticsConfiguration.Logging.Categories.Keys )
        {
            Assert.Equal( diagnosticsConfiguration.Logging.Categories[categoryKey], deserializedDiagnosticsConfiguration.Logging.Categories[categoryKey] );
        }

        foreach ( var key in diagnosticsConfiguration.Logging.Processes.Keys )
        {
            Assert.Equal( diagnosticsConfiguration.Logging.Processes[key], deserializedDiagnosticsConfiguration.Logging.Processes[key] );
        }

        Assert.Equal( diagnosticsConfiguration.Logging.StopLoggingAfterHours, deserializedDiagnosticsConfiguration.Logging.StopLoggingAfterHours );
        
        // Debugger
        foreach ( var key in diagnosticsConfiguration.Debugger.Processes.Keys )
        {
            Assert.Equal( diagnosticsConfiguration.Debugger.Processes[key], deserializedDiagnosticsConfiguration.Debugger.Processes[key] );
        }
        
        // MiniDump
        foreach ( var flag in diagnosticsConfiguration.MiniDump.Flags )
        {
            Assert.Equal( diagnosticsConfiguration.MiniDump.Flags.Contains( flag ), deserializedDiagnosticsConfiguration.MiniDump.Flags.Contains( flag ) );
        }

        foreach ( var key in diagnosticsConfiguration.MiniDump.Processes.Keys )
        {
            Assert.Equal( diagnosticsConfiguration.MiniDump.Processes[key], deserializedDiagnosticsConfiguration.MiniDump.Processes[key] );
        }
        
        foreach ( var exceptionType in diagnosticsConfiguration.MiniDump.ExceptionTypes )
        {
            Assert.Equal( diagnosticsConfiguration.MiniDump.Flags.Contains( exceptionType ), deserializedDiagnosticsConfiguration.MiniDump.Flags.Contains( exceptionType ) );
        }

        // Finally test if both files are identical.
        Assert.Equal( diagnosticsConfiguration.ToJson(), deserializedDiagnosticsConfiguration.ToJson() );
    }
}