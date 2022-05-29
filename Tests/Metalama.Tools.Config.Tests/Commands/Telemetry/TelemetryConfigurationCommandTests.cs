// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Telemetry
{
    public class TelemetryConfigurationCommandTests : ConfigurationCommandsTestsBase<TelemetryConfiguration>
    {
        public TelemetryConfigurationCommandTests( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base( logger, serviceBuilder )
        {
        }


        [Fact]
        public void TelemetryAskedByDefault()
        {
            this.AssertConfigurationFileExists( false );

            var configuration = this.GetConfiguration();
            Assert.Equal( ReportingAction.Ask, configuration.ReportUsage );
        }

        [Fact]
        public async Task TelemetryEnableEnablesTelemetry()
        {
            this.AssertConfigurationFileExists( false );

            await this.TestCommandAsync( "telemetry enable", "Telemetry has been enabled.\r\n" );

            this.AssertConfigurationFileExists( true );

            var configuration = this.GetConfiguration();
            Assert.Equal( ReportingAction.Yes, configuration.ReportUsage );
        }

        [Fact]
        public async Task TelemetryDisableDisablesTelemetry()
        {
            this.FileSystem.WriteAllText(
                this.ConfigurationFilePath,
                new TelemetryConfiguration() { ReportUsage = ReportingAction.Yes }.ToJson() );

            this.AssertConfigurationFileExists( true );

            var configuration = this.GetConfiguration();
            Assert.Equal( ReportingAction.Yes, configuration.ReportUsage );

            await this.TestCommandAsync( "telemetry disable", "Telemetry has been disabled.\r\n" );

            this.AssertConfigurationFileExists( true );

            Assert.Equal( ReportingAction.No, configuration.ReportUsage );
        }

        [Fact]
        public async Task ResetDeviceIdResetsDeviceId()
        {
            this.FileSystem.WriteAllText(
                this.ConfigurationFilePath,
                new TelemetryConfiguration().ToJson() );

            this.AssertConfigurationFileExists( true );

            var configuration = this.GetConfiguration();
            var originalDeviceId = configuration.DeviceId;

            await this.TestCommandAsync( "telemetry reset-device-id", "The device id has been reset.\r\n" );

            this.AssertConfigurationFileExists( true );

            Assert.NotEqual( originalDeviceId, configuration.DeviceId );
        }
    }
}