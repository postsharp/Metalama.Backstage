// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;

namespace Metalama.Backstage.Licensing.Tests.ConfigurationManager;

[ConfigurationFile( "test.json" )]
internal class TestConfigurationFile : ConfigurationFile
{
    public bool IsModified { get; set; }

    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        this.IsModified = ((TestConfigurationFile) configurationFile).IsModified;
    }
}