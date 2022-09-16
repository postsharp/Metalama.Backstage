// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.ConfigurationManager;

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

        this.SetUpEnvironmentVariable();
    }

    private void SetUpEnvironmentVariable()
    {
        this.EnvironmentVariableProvider.SetEnvironmentVariable( this.EnvironmentVariableProvider.EnvironmentVariableName, this._environmentVariableJsonContent );
    }

    [Fact]
    public void EnvironmentVariableExists()
    {
        var environmentVariableValue = this.EnvironmentVariableProvider.GetEnvironmentVariable( this.EnvironmentVariableProvider.EnvironmentVariableName );

        Assert.NotNull( environmentVariableValue );
    }

    [Fact]
    public void EnvironmentVariableHasCorrectValue()
    {
        var environmentVariableValue = this.EnvironmentVariableProvider.GetEnvironmentVariable( this.EnvironmentVariableProvider.EnvironmentVariableName );

        Assert.Equal( this._environmentVariableJsonContent, environmentVariableValue );
    }

    [Fact]
    public void UpdateEnvironmentVariable()
    {
        this.EnvironmentVariableProvider.SetEnvironmentVariable( this.EnvironmentVariableProvider.EnvironmentVariableName, "newValue" );
        var environmentVariableValue = this.EnvironmentVariableProvider.GetEnvironmentVariable( this.EnvironmentVariableProvider.EnvironmentVariableName );

        Assert.Equal( "newValue", environmentVariableValue );
    }

    [Fact]
    public void ExistingEnvironmentVariableIsUsedAsConfiguration()
    {
        var updatedConfiguration = this._configurationManager.Get<DiagnosticsConfiguration>( false );
        
        // TODO: Remove.
        this.Logger.WriteLine( this._environmentVariableJsonContent );
        this.Logger.WriteLine( "======" );
        this.Logger.WriteLine( updatedConfiguration.ToJson() );
        
        Assert.Equal( this._environmentVariableJsonContent, updatedConfiguration.ToJson() );
    }
}