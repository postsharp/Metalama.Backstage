// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;

namespace Metalama.Backstage.Welcome;

[ConfigurationFile( "welcome.json" )]
public class WelcomeConfiguration : ConfigurationFile
{
    public bool IsFirstStart { get; set; } = true;

    public bool IsWelcomePagePending { get; set; } = true;

    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (WelcomeConfiguration) configurationFile;
        this.IsFirstStart = source.IsFirstStart;
        this.IsWelcomePagePending = source.IsWelcomePagePending;
    }
}