// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Logging
{
    public class DiagnosticsConfigurationCommandTests : ConfigurationCommandsTestsBase<DiagnosticsConfiguration>
    {
        public DiagnosticsConfigurationCommandTests( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base( logger, serviceBuilder )
        {
        }

        private string FormatExpectedOutput( DiagnosticsConfiguration expectedConfiguration )
            => @$"The file '{this.ConfigurationFilePath}' contains the following configuration:

{expectedConfiguration.ToJson()}
";

        [Fact]
        public async Task PrintBeforeEditPrintsDefault()
        {
            this.AssertConfigurationFileExists( false );

            await this.TestCommandAsync( $"diag print", this.FormatExpectedOutput( new DiagnosticsConfiguration() ) );

            this.AssertConfigurationFileExists( false );

            Assert.Empty( this.StartedProcesses );
        }

        [Fact]
        public async Task PrintAfterEditPrintsDefault()
        {
            this.AssertConfigurationFileExists( false );

            await this.TestCommandAsync( $"diag edit", "" );

            this.AssertConfigurationFileExists( true );

            Assert.Single( this.StartedProcesses, p => p.DeepEquals( new ProcessStartInfo( this.ConfigurationFilePath ) { UseShellExecute = true } ) );

            await this.TestCommandAsync( $"diag print", this.FormatExpectedOutput( new DiagnosticsConfiguration() ) );
        }

        [Fact]
        public async Task PrintReflectsModifications()
        {
            var nonDefaultConfiguration = new DiagnosticsConfiguration();
            nonDefaultConfiguration.Debugger.Processes[ProcessKind.Compiler] = true;
            nonDefaultConfiguration.Logging.Processes[ProcessKind.DevEnv] = true;

            this.FileSystem.WriteAllText( this.ConfigurationFilePath, nonDefaultConfiguration.ToJson() );

            await this.TestCommandAsync( $"diag print", this.FormatExpectedOutput( nonDefaultConfiguration ) );
        }

        [Fact]
        public async Task PrintAfterResetPrintsDefaultConfiguration()
        {
            var nonDefaultConfiguration = new DiagnosticsConfiguration();
            nonDefaultConfiguration.Debugger.Processes[ProcessKind.Compiler] = true;
            nonDefaultConfiguration.Logging.Processes[ProcessKind.DevEnv] = true;

            this.FileSystem.WriteAllText( this.ConfigurationFilePath, nonDefaultConfiguration.ToJson() );

            await this.TestCommandAsync( $"diag print", this.FormatExpectedOutput( nonDefaultConfiguration ) );

            await this.TestCommandAsync( $"diag reset", "" );

            this.AssertConfigurationFileExists( true );

            Assert.Empty( this.StartedProcesses );

            await this.TestCommandAsync( $"diag print", this.FormatExpectedOutput( new DiagnosticsConfiguration() ) );
        }
    }
}