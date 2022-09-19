// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Diagnostics;

public class EnvironmentVariableConfigurationTests : TestsBase
{
    private readonly string _environmentVariableJsonContent =
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
    },
    ""stopLoggingAfterHours"": 2
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

    private readonly string _localJsonContent =
        @"{
  ""logging"": {
    ""processes"": {
      ""Compiler"": false,
      ""Rider"": false,
      ""DevEnv"": false,
      ""RoslynCodeAnalysisService"": false
    },
    ""categories"": {
      ""*"": true
    },
    ""StopLoggingAfterHours"": 2
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

    public EnvironmentVariableConfigurationTests( ITestOutputHelper logger ) : base(
        logger,
        builder =>
        {
            builder.AddService( typeof(IConfigurationManager), new Configuration.ConfigurationManager( builder.ServiceProvider ) );
            builder.AddService( typeof(IApplicationInfoProvider), new ApplicationInfoProvider( new TestApplicationInfo() ) );
            builder.AddService( typeof(IEnvironmentVariableProvider), new TestEnvironmentVariableProvider() );
        } )
    {
        this._configurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>();
        var standardDirectories = this.ServiceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this.FileSystem.CreateDirectory( standardDirectories.ApplicationDataDirectory );
        var diagnosticsJsonFilePath = Path.Combine( standardDirectories.ApplicationDataDirectory, "diagnostics.json" );
        this.FileSystem.WriteAllText( diagnosticsJsonFilePath, this._localJsonContent );

        this.EnvironmentVariableProvider.SetEnvironmentVariable( this.EnvironmentVariableProvider.DefaultDiagnosticsEnvironmentVariableName, this._environmentVariableJsonContent );
    }

    [Fact]
    public void EnvironmentVariable_Exists()
    {
        var environmentVariableValue = this.EnvironmentVariableProvider.GetEnvironmentVariable( this.EnvironmentVariableProvider.DefaultDiagnosticsEnvironmentVariableName );

        Assert.NotNull( environmentVariableValue );
    }

    [Fact]
    public void EnvironmentVariable_HasCorrectValue()
    {
        var environmentVariableValue = this.EnvironmentVariableProvider.GetEnvironmentVariable( this.EnvironmentVariableProvider.DefaultDiagnosticsEnvironmentVariableName );

        Assert.Equal( this._environmentVariableJsonContent, environmentVariableValue );
    }

    [Fact]
    public void UpdateEnvironmentVariable()
    {
        this.EnvironmentVariableProvider.SetEnvironmentVariable( this.EnvironmentVariableProvider.DefaultDiagnosticsEnvironmentVariableName, "newValue" );
        var environmentVariableValue = this.EnvironmentVariableProvider.GetEnvironmentVariable( this.EnvironmentVariableProvider.DefaultDiagnosticsEnvironmentVariableName );

        Assert.Equal( "newValue", environmentVariableValue );
    }

    [Fact]
    public void ExistingEnvironmentVariable_OverridesConfiguration()
    {
        var diagnosticsConfiguration = this._configurationManager.Get( typeof(DiagnosticsConfiguration), true );
        var diagnosticsFileContents = diagnosticsConfiguration.ToJson();

        Assert.Equal( this._environmentVariableJsonContent, diagnosticsFileContents );
    }
}