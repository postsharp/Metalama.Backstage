// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Diagnostics;

/// <summary>
/// This tests class works with predefined default configuration <see cref="_diagnosticsJsonFileContent"/>.
/// </summary>
public class DiagnosticsConfigurationTests : TestsBase
{
    private readonly string _diagnosticsJsonFilePath;

    private readonly string _diagnosticsJsonFileContent =
        @"{
  ""logging"": {
    ""processes"": {
      ""Compiler"": true,
      ""Rider"": true,
      ""DevEnv"": true,
      ""RoslynCodeAnalysisService"": true
    },
    ""categories"": {
      ""*"": true
    }
  },
  ""debugger"": {
    ""processes"": {
      ""Compiler"": false,
      ""Rider"": false,
      ""DevEnv"": false,
      ""RoslynCodeAnalysisService"": false
    }
  },
  ""miniDump"": {
    ""processes"": {
      ""Compiler"": false,
      ""Rider"": false,
      ""DevEnv"": false,
      ""RoslynCodeAnalysisService"": false
    },
    ""flags"": [
      ""WithDataSegments"",
      ""WithProcessThreadData"",
      ""WithHandleData"",
      ""WithPrivateReadWriteMemory"",
      ""WithUnloadedModules"",
      ""WithFullMemoryInfo"",
      ""WithThreadInfo"",
      ""FilterMemory"",
      ""WithoutAuxiliaryState""
    ],
    ""ExceptionTypes"": [
      ""*""
    ]
  }
}";

    private readonly IConfigurationManager _configurationManager;

    public DiagnosticsConfigurationTests( ITestOutputHelper logger ) : base(
        logger,
        builder =>
        {
            builder.AddService( typeof(IConfigurationManager), new Configuration.ConfigurationManager( builder.ServiceProvider ) );
            builder.AddService( typeof(IApplicationInfoProvider), new ApplicationInfoProvider( new TestApplicationInfo() ) );
            builder.AddService( typeof(IEnvironmentVariableProvider), new TestEnvironmentVariableProvider() );
        } )
    {
        var standardDirectories = this.ServiceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this._configurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>();

        this.FileSystem.CreateDirectory( standardDirectories.ApplicationDataDirectory );
        this._diagnosticsJsonFilePath = Path.Combine( standardDirectories.ApplicationDataDirectory, "diagnostics.json" );
        this.FileSystem.WriteAllText( this._diagnosticsJsonFilePath, this._diagnosticsJsonFileContent );
    }

    [Fact]
    public void LocalDiagnosticsJsonFile_Exists()
    {
        Assert.NotNull( this.FileSystem.ReadAllText( this._diagnosticsJsonFilePath ) );
        var diagnosticsFileContents = this._configurationManager.Get( typeof(DiagnosticsConfiguration), false ).ToJson();

        Assert.Contains( "\"Compiler\": true", diagnosticsFileContents, StringComparison.OrdinalIgnoreCase );
        Assert.Contains( "\"Rider\": true", diagnosticsFileContents, StringComparison.OrdinalIgnoreCase );
        Assert.Contains( "\"DevEnv\": true", diagnosticsFileContents, StringComparison.OrdinalIgnoreCase );
        Assert.Contains( "\"RoslynCodeAnalysisService\": true", diagnosticsFileContents, StringComparison.OrdinalIgnoreCase );
    }

    [Fact]
    public void ChangingLoggingHours_UpdatesConfiguration()
    {
        this._configurationManager.Update<DiagnosticsConfiguration>( c => c.SetStopLoggingAfterHours( 10 ) );
        var diagnosticsConfiguration = (DiagnosticsConfiguration) this._configurationManager.Get( typeof(DiagnosticsConfiguration), true );

        Assert.Equal( 10, diagnosticsConfiguration.Logging.StopLoggingAfterHours );
    }

    [Fact]
    public void ResettingConfiguration_ChangesLoggingHours()
    {
        // Manually set the hours to more than default.
        this._configurationManager.Update<DiagnosticsConfiguration>( c => c.SetStopLoggingAfterHours( 10 ) );

        this._configurationManager.Update<DiagnosticsConfiguration>( c => c.Reset() );
        var diagnosticsConfiguration = (DiagnosticsConfiguration) this._configurationManager.Get( typeof(DiagnosticsConfiguration), true );

        Assert.Equal( 2, diagnosticsConfiguration.Logging.StopLoggingAfterHours );
    }

    [Fact]
    public void DisablingLogging_UpdatesLocalFile()
    {
        this._configurationManager.Update<DiagnosticsConfiguration>( c => c.DisableLogging() );
        var diagnosticsFileContents = this.FileSystem.ReadAllText( this._diagnosticsJsonFilePath );

        Assert.DoesNotContain(
            @"{
  ""logging"": {
    ""processes"": {
      ""Compiler"": true,
      ""Rider"": true,
      ""DevEnv"": true,
      ""RoslynCodeAnalysisService"": true
    }",
            diagnosticsFileContents,
            StringComparison.OrdinalIgnoreCase );

        Assert.Contains(
            @"{
  ""logging"": {
    ""processes"": {
      ""Compiler"": false,
      ""Rider"": false,
      ""DevEnv"": false,
      ""RoslynCodeAnalysisService"": false
    }",
            diagnosticsFileContents,
            StringComparison.OrdinalIgnoreCase );
    }

    [Fact]
    public void DisableLogging_AfterPeriodOfTime()
    {
        // Manually make the diagnostics.json to be older than specified amount of time.
        this.FileSystem.SetLastWriteTime( this._diagnosticsJsonFilePath, DateTime.Now.AddHours( -3 ) );

        var diagnosticsConfiguration = this._configurationManager.Get( typeof(DiagnosticsConfiguration), true );
        var diagnosticsFileContents = diagnosticsConfiguration.ToJson();

        Assert.DoesNotContain(
            @"{
  ""logging"": {
    ""processes"": {
      ""Compiler"": true,
      ""Rider"": true,
      ""DevEnv"": true,
      ""RoslynCodeAnalysisService"": true
    }",
            diagnosticsFileContents,
            StringComparison.OrdinalIgnoreCase );

        Assert.Contains(
            @"{
  ""logging"": {
    ""processes"": {
      ""Compiler"": false,
      ""Rider"": false,
      ""DevEnv"": false,
      ""RoslynCodeAnalysisService"": false
    }",
            diagnosticsFileContents,
            StringComparison.OrdinalIgnoreCase );
    }

    [Fact]
    public void DoNotDisableLogging_BeforePeriodOfTime()
    {
        // Manually make the diagnostics.json to be older than specified amount of time.
        this.FileSystem.SetLastWriteTime( this._diagnosticsJsonFilePath, DateTime.Now.AddHours( -1 ) );
        
        var diagnosticsConfiguration = this._configurationManager.Get( typeof(DiagnosticsConfiguration), true );
        var diagnosticsFileContents = diagnosticsConfiguration.ToJson();

        Assert.Contains(
            @"{
  ""logging"": {
    ""processes"": {
      ""Compiler"": true,
      ""Rider"": true,
      ""DevEnv"": true,
      ""RoslynCodeAnalysisService"": true
    }",
            diagnosticsFileContents,
            StringComparison.OrdinalIgnoreCase );
    }

    [Fact]
    public void JsonPropertyAttribute_SerializesPropertiesCorrectly()
    {
        var diagnosticsConfiguration = (DiagnosticsConfiguration) this._configurationManager.Get( typeof(DiagnosticsConfiguration), true );

        var deserializedDiagnosticsConfiguration = JsonConvert.DeserializeObject<DiagnosticsConfiguration>( this._diagnosticsJsonFileContent );

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
        
        // TODO: Maybe better test with actual positions being the same.
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