﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using System.Collections.Immutable;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Diagnostics;

public class EnvironmentVariableConfigurationTests : TestsBase
{
    private readonly IConfigurationManager _configurationManager;

    public EnvironmentVariableConfigurationTests( ITestOutputHelper logger ) : base(
        logger,
        builder =>
        {
            builder.AddService( typeof(IConfigurationManager), new Configuration.ConfigurationManager( builder.ServiceProvider ) );
            builder.AddService( typeof(IApplicationInfoProvider), new ApplicationInfoProvider( new TestApplicationInfo() ) );
        } )
    {
        this._configurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>();
        var standardDirectories = this.ServiceProvider.GetRequiredBackstageService<IStandardDirectories>();

        // Initialize local DiagnosticsConfiguration.
        this.FileSystem.CreateDirectory( standardDirectories.ApplicationDataDirectory );
        this.FileSystem.WriteAllText( this._configurationManager.GetFileName( typeof(DiagnosticsConfiguration) ), new DiagnosticsConfiguration().ToJson() );

        // Set up environment variable DiagnosticsConfiguration.
        var environmentConfiguration = new DiagnosticsConfiguration() with
        {
            Logging = new LoggingConfiguration() { Processes = ImmutableDictionary<ProcessKind, bool>.Empty.Add( ProcessKind.Compiler, true ) }
        };

        this.EnvironmentVariableProvider.Environment.Add( DiagnosticsConfiguration.EnvironmentVariableName, environmentConfiguration.ToJson() );
    }

    [Fact]
    public void ExistingEnvironmentVariable_OverridesConfiguration()
    {
        var diagnosticsConfiguration = this._configurationManager.Get<DiagnosticsConfiguration>();

        Assert.True( diagnosticsConfiguration.Logging.Processes[ProcessKind.Compiler] );
    }
}