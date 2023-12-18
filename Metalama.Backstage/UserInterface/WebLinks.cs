// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.UserInterface;

#pragma warning disable CA1822

public class WebLinks : IBackstageService
{
    public string AfterSetup => "https://www.postsharp.net/links/TODO";

    public string GetTeamTeamTrial => "https://www.postsharp.net/links/TODO";

    public string VisualStudioMarketplace => "https://www.postsharp.net/links/TODO";
    
    public string PrivacyPolicy => "https://www.postsharp.net/links/TODO";
    
    public string LicenseAgreement => "https://www.postsharp.net/links/TODO";
}