// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands
{
    public abstract class ConfigurationCommandsTestsBase<TConfiguration> : CommandsTestsBase
        where TConfiguration : ConfigurationFile
    {
        private readonly IConfigurationManager _configurationManager;

        protected string ConfigurationFilePath { get; }

        public ConfigurationCommandsTestsBase( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base( logger, serviceBuilder )
        {
            this._configurationManager = this.ServiceProvider.GetRequiredService<IConfigurationManager>();
            this.ConfigurationFilePath = this._configurationManager.GetFileName<TConfiguration>();
        }

        protected void AssertConfigurationFileExists( bool shouldExist )
            => Assert.Equal( shouldExist, this.FileSystem.FileExists( this.ConfigurationFilePath ) );

        protected TConfiguration GetConfiguration()
            => this._configurationManager.Get<TConfiguration>();
    }
}