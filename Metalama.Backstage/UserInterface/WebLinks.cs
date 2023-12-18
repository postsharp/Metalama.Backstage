// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.UserInterface;

public class WebLinks : IBackstageService
{
    public string AfterSetup => "https://www.postsharp.net/links/TODO";

    public object GetTeamTeamTrial { get; }

    public object VisualStudioMarketplace { get; }
}