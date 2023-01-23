// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Configuration;

namespace Metalama.Backstage.Welcome;

[ConfigurationFile( "welcome.json" )]
[UsedImplicitly]
internal record WelcomeConfiguration : ConfigurationFile
{
    public bool IsFirstStart { get; init; } = true;

    public bool IsWelcomePagePending { get; init; } = true;
}