// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

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