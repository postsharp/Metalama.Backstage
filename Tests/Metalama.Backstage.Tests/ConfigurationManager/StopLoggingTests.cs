// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.ConfigurationManager;

public class StopLoggingTests : TestsBase
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

    public StopLoggingTests( ITestOutputHelper logger ) : base(
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
    public void DiagnosticsJsonFileExists()
    {
        Assert.NotNull( this.FileSystem.ReadAllText( this._diagnosticsJsonFilePath ) );
        var diagnosticsFileContents = this._configurationManager.Get( typeof(DiagnosticsConfiguration), false ).ToJson();

        Assert.Contains( "\"Compiler\": true", diagnosticsFileContents, StringComparison.OrdinalIgnoreCase );
        Assert.Contains( "\"Rider\": true", diagnosticsFileContents, StringComparison.OrdinalIgnoreCase );
        Assert.Contains( "\"DevEnv\": true", diagnosticsFileContents, StringComparison.OrdinalIgnoreCase );
        Assert.Contains( "\"RoslynCodeAnalysisService\": true", diagnosticsFileContents, StringComparison.OrdinalIgnoreCase );
    }

    [Fact]
    public void DisablingLoggingUpdatesLocalFile()
    {
        this._configurationManager.Update<DiagnosticsConfiguration>( c => c.DisableLogging() );
        var diagnosticsConfiguration = this._configurationManager.Get( typeof(DiagnosticsConfiguration), true );

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
    public void DisableLoggingAfterPeriodOfTime()
    {
        // Manually make the diagnostics.json to be older than specified amount of time.
        this.FileSystem.SetLastWriteTime( this._diagnosticsJsonFilePath, DateTime.Now.AddDays( -10 ) );

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
    public void LoggingIsNotDisabledBeforePeriodOfTime()
    {
        // Manually make the diagnostics.json to be older than specified amount of time.
        this.FileSystem.SetLastWriteTime( this._diagnosticsJsonFilePath, DateTime.Now.AddDays( -2 ) );
        
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
}